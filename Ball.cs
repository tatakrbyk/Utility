using UnityEngine;

public class BasketballThrow : MonoBehaviour
{
    [Header("Throw Settings")]
    public float maxThrowPower = 15f;
    public float minThrowPower = 3f;
    public float upwardForceMultiplier = 1.2f;
    public float lateralForceMultiplier = 0.8f;

    [Header("Swipe Detection")]
    public float minSwipeDistance = 50f;
    public float swipePowerMultiplier = 0.1f;

    [Header("Ball Physics")]
    public float rotationSpeed = 5f;

    private Rigidbody rb;
    private Camera mainCamera;
    private bool isDragging = false;
    private bool isThrown = false;
    private Vector3 startTouchPosition;
    private Vector3 endTouchPosition;
    private Vector3 throwDirection;
    private float throwPower;
    private Vector3 originalPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        originalPosition = transform.position;

        SetupBallPhysics();
    }

    void SetupBallPhysics()
    {
        rb.useGravity = false;
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (!isThrown)
        {
            HandleTouchInput();
        }

        // Debug için klavye kontrolü
        HandleDebugInput();
    }

    void HandleTouchInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag(Input.mousePosition);
        }
    }

    void StartDrag(Vector3 touchPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            startTouchPosition = touchPosition;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void EndDrag(Vector3 touchPosition)
    {
        if (!isDragging) return;

        endTouchPosition = touchPosition;
        CalculateThrowParameters();
        ThrowBall();

        isDragging = false;
        isThrown = true;
    }

    void CalculateThrowParameters()
    {
        Vector3 swipeVector = endTouchPosition - startTouchPosition;
        float swipeDistance = swipeVector.magnitude;

        // Swipe mesafesine göre güç hesapla
        throwPower = Mathf.Clamp(swipeDistance * swipePowerMultiplier, minThrowPower, maxThrowPower);

        // Swipe yönünü dünya koordinatlarına çevir
        throwDirection = ConvertSwipeToWorldDirection(swipeVector.normalized);

        Debug.Log($"Swipe Direction: {swipeVector.normalized}, World Direction: {throwDirection}");
        Debug.Log($"Swipe Distance: {swipeDistance}, Throw Power: {throwPower}");
    }

    Vector3 ConvertSwipeToWorldDirection(Vector3 swipeDirection)
    {
        // Kameranın yönünü hesaba kat
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Y eksenini düzleştir (yatay hareket için)
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Swipe vektörünü dünya koordinatlarına dönüştür
        Vector3 worldDirection = Vector3.zero;

        // X ekseni: Sol/Sağ swipe -> Yanal hareket
        worldDirection += cameraRight * swipeDirection.x * lateralForceMultiplier;

        // Y ekseni: Yukarı/Aşağı swipe -> Yükseklik ve ileri hareket
        if (swipeDirection.y > 0) // Yukarı swipe
        {
            worldDirection += Vector3.up * swipeDirection.y * upwardForceMultiplier;
            worldDirection += cameraForward * swipeDirection.y; // İleri momentum
        }
        else // Aşağı swipe - daha az ileri momentum
        {
            worldDirection += Vector3.up * 0.1f; // Minimum yükseklik
            worldDirection += cameraForward * 0.3f; // Zayıf ileri momentum
        }

        return worldDirection.normalized;
    }

    void ThrowBall()
    {
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;

        // Ana fırlatma kuvveti
        Vector3 force = throwDirection * throwPower;
        rb.AddForce(force, ForceMode.Impulse);

        // Topa gerçekçi dönme efekti ekle
        AddBallRotation();

        Debug.Log($"Throwing ball with force: {force}");
    }

    void AddBallRotation()
    {
        // Swipe yönüne göre topa dönme ver
        Vector3 torque = new Vector3(
            -throwDirection.y * rotationSpeed,
            throwDirection.x * rotationSpeed,
            0
        );

        rb.AddTorque(torque, ForceMode.Impulse);
    }

    public void ResetBall()
    {
        isThrown = false;
        isDragging = false;
        transform.position = originalPosition;
        transform.rotation = Quaternion.identity;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Debug.Log("Ball reset");
    }

    void HandleDebugInput()
    {
        // Debug için klavye kontrolleri
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
        }

        if (Input.GetKeyDown(KeyCode.T) && !isThrown)
        {
            // Test atışı
            throwDirection = new Vector3(0, 0.5f, 1f).normalized;
            throwPower = 8f;
            ThrowBall();
            isThrown = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Top bir yere çarptığında fizik etkileşimleri
        if (collision.relativeVelocity.magnitude > 2f)
        {
            // Çarpma sesi veya efekti buraya eklenebilir
        }
    }

    // Gizmos for debugging
    void OnDrawGizmos()
    {
        if (isDragging && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector3 worldStart = mainCamera.ScreenToWorldPoint(new Vector3(startTouchPosition.x, startTouchPosition.y, mainCamera.nearClipPlane + 1f));
            Vector3 worldEnd = mainCamera.ScreenToWorldPoint(new Vector3(endTouchPosition.x, endTouchPosition.y, mainCamera.nearClipPlane + 1f));

            Gizmos.DrawLine(worldStart, worldEnd);
            Gizmos.DrawWireSphere(worldStart, 0.1f);
            Gizmos.DrawWireSphere(worldEnd, 0.1f);
        }

        if (isThrown)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, throwDirection * 2f);
        }
    }
}