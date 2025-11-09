using System.Collections;
using UnityEngine;

namespace DavidJalbert.TinyCarControllerAdvance
{
    // Bu, TCCAPlayer nesnesine eklenecek ana kontrolcümüzdür.
    public class VehicleController : MonoBehaviour
    {

        [Header("INSPECTOR (DEBUG)")]
        [SerializeField] private float currentVelocityMagnitude; // Anlık Hız (Inspector'da görünecek)
        [SerializeField] private float currentForceDisplay; // Anlık Kuvvet (Inspector'da görünecek)

        [Header("Fırlatma Parametreleri")]
        [Tooltip("Maksimum geri çekme mesafesi (ekranda piksel/dünya birimi).")]
        public float maxDrawDistance = 3f;
        [Tooltip("Arabaya uygulanacak maksimum fırlatma kuvveti (TCCAPlayer'a iletilen motorDelta çarpanı).")]
        public float maxForce = 50f;
        [Tooltip("Fırlatıldıktan sonra arabanın durma sürtünme çarpanı.")]
        public float frictionFactor = 0.98f;
        [Tooltip("Fırlatma anında ivmelenme için tam gazın uygulanacağı süre (saniye).")]
        public float launchMotorBurstDuration = 0.1f;

        [Header("Görsel Bileşenler (LineRenderer/Sprite)")]
        public LineRenderer directionLineRenderer; // DirectionVisualObject'e atılacak
        public LineRenderer forceLineRenderer;
        public GameObject controlMarkerObject; // Kontrol işareti (örneğin, bir sprite)
        [Tooltip("Tekerleklerin gösteri amaçlı dönerken kullanacağı motorDelta değeri.")]
        public float spinMotorDelta = 0.5f;

        // Özel Layer: Oyuncunun arabayı tıklayacağı layer.
        public LayerMask vehicleLayer;

        private TCCAPlayer player;
        private bool isControllable = true; // Araba durduğunda true olacak
        private bool isDragging = false;
        private Vector3 dragStartPosition;
        private float currentForce = 0f;

        private const float STOPPING_THRESHOLD = 0.05f;

        public float rotationLockDuration = 0.1f; // 0.05 ile 0.2 arasında deneyin.

        private IEnumerator ReleaseRotationLock(Rigidbody rb)
        {
            // Belirtilen süre kadar bekleyin. (Örn: 0.1 saniye)
            yield return new WaitForSeconds(rotationLockDuration);

            // Kilidi kaldırın (Varsayılan ayarlara geri dönün)
            // Bu, arabanın havada takla atmasını veya yanal dönmesini tekrar sağlar.
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
            }
        }
        void Awake()
        {
            player = GetComponent<TCCAPlayer>();
        }

        void Start()
        {
            // Başlangıçta işaretçiyi aç, aracı 'hazır' moda getir.
            SetControllable(true);

            // Görsel bileşenleri başlangıçta gizle
            if (directionLineRenderer != null) directionLineRenderer.gameObject.SetActive(false);
            if (forceLineRenderer != null) forceLineRenderer.gameObject.SetActive(false);
        
        }

        void Update()
        {
            

            // INSPECTOR DEBUG: Anlık değerleri takip et
            currentVelocityMagnitude = player.getRigidbody().velocity.magnitude;
            currentForceDisplay = currentForce;
            // 1. Durum Kontrolü
            CheckCarStatus();

            if (isControllable)
            {
                // Kontrol İşaretini Yönet
                if (controlMarkerObject != null) controlMarkerObject.SetActive(true);

                // Tekerlekleri Döndür (Göstermelik)
                // Hareketi engellemek için motorDelta değerini TCCAPlayer'a FixedUpdate içinde göndermeyeceğiz.
                // Sadece görsel efekti (duman/ses) tetiklemek için TCCAWheel'in inputMotor'unu kullanacağız.
                // Ancak TCCAPlayer'daki FixedUpdate motorDelta'yı her zaman set ettiği için, 
                // ya TCCAPlayer'ı düzenlemeli ya da bu değeri her frame sıfırlamalıyız.

                // Basit çözüm: Tekerlekleri döndürerek efektleri tetikle, ama fiziksel hareket olmasın.
                SetCarSpin(spinMotorDelta);
                //if (!isDragging)
                //{
                // **[DÜZELTME A] BAŞLANGIÇ HAREKETİNİ SIFIRLAMA:**
                // isControllable durumunda, sürükleme yoksa HAREKETİ ZORLA SIFIRLA.
                // Bu, spinMotorDelta'nın FixedUpdate'te neden olduğu istenmeyen hareketi engeller.
                player.getRigidbody().velocity = Vector3.zero;
                player.getRigidbody().angularVelocity = Vector3.zero;
                // }

                HandleInput();
            }
            else
            {
                // Araba hareket ediyorken kontrol işaretini kapat
                if (controlMarkerObject != null) controlMarkerObject.SetActive(false);

                // Tekerlek döndürmeyi durdur
                SetCarSpin(0);

                // Sürtünme Uygula (Hız Kesme)
                ApplyFriction();
            }
        }

        void HandleInput()
        {
            // Fare Tıklaması
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Araç üzerine tıklandı mı?
                if (Physics.Raycast(ray, out hit, 100f, vehicleLayer))
                {
                    if (hit.collider.gameObject == player.getCarBody().gameObject || hit.collider.GetComponentInParent<TCCAPlayer>() == player)
                    {
                        if (isControllable) // Sadece kontrol edilebilir durumdayken nişan almaya başla
                        {
                            isDragging = true;
                            dragStartPosition = GetMouseWorldPosition();

                            // Nişan alma görselini aç
                            if (directionLineRenderer != null) directionLineRenderer.gameObject.SetActive(true);
                            if (forceLineRenderer != null) forceLineRenderer.gameObject.SetActive(true);
                        }
                    }
                }
            }

            // Fare Basılı Tutma (Nişan Alma)
            if (isDragging && Input.GetMouseButton(0))
            {
                Vector3 currentDragPosition = GetMouseWorldPosition();
                Vector3 dragVector = dragStartPosition - currentDragPosition; // Geri çekme vektörü

                float drawDistance = dragVector.magnitude;

                // 1. Force Değeri Hesaplama
                float clampedDistance = Mathf.Clamp(drawDistance, 0, maxDrawDistance);
                currentForce = clampedDistance / maxDrawDistance * maxForce;

                // 2. Rotasyon Hesaplama
                if (dragVector.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(dragVector.normalized, Vector3.up);
                    player.getCarBody().setRotation(targetRotation);
                }

                // 3. Görsel Güncellemeler (Yön/Force)
                UpdateVisuals(dragVector.normalized, currentForce);
            }

            // Fare Bırakma (Fırlatma)
            if (isDragging && Input.GetMouseButtonUp(0))
            {
                isDragging = false;

                // Nişan alma görselini kapat
                if (directionLineRenderer != null) directionLineRenderer.gameObject.SetActive(false);
                if (forceLineRenderer != null) forceLineRenderer.gameObject.SetActive(false);

                if (currentForce > 0.1f) // Yeterli kuvvet varsa fırlat
                {
                    SetControllable(false);
                    LaunchCar(currentForce);
                }
                else
                {
                    // Kuvvet sıfırsa tekrar hazır konuma dön
                    SetControllable(true);
                }

                currentForce = 0;
            }
        }

        // Aracı fırlatır (ivmelenerek hızlanır)
        void LaunchCar(float force)
        {
            Rigidbody rb = player.getRigidbody();
            // TCCAPlayer FixedUpdate'te motorDelta'yı tekerleklere aktarır.
            // Bu sistemde motorDelta, tek bir FixedUpdate adımında araca bir itme kuvveti verir.
            // Motor torku yerine, hızını bir anda ayarlayarak fırlatabiliriz.

            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            Vector3 launchDirection = player.getCarBody().transform.forward;

            // Mevcut TCCA yapısında anlık hız uygulama:
            // Arabanın hızını ayarla (force çarpan olarak)
            player.getRigidbody().velocity = launchDirection * force;
            rb.angularVelocity = Vector3.zero;
            StartCoroutine(ReleaseRotationLock(rb));

            //rb.AddForce(launchDirection * force, ForceMode.VelocityChange);

            //player.getRigidbody().angularVelocity = Vector3.zero;
            // Ayrıca 'motorDelta'yı kısa bir an için ayarlayarak ivmelenme de sağlanabilir.
            // Örneğin, bir Coroutine ile:
            // İvmelenme hissiyatı için kısa bir motor torku uygula
            StartCoroutine(ApplyMotorBurst(1.0f, launchMotorBurstDuration));
        }

        /*
         void LaunchCar(float force)
{
            Rigidbody rb = player.getRigidbody();

            // 1️⃣ Fırlatma öncesi stabilizasyon
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Fırlatma sırasında yatay ekseni kilitle (yan devrilmeyi önler)
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // 2️⃣ Fırlatma yönü (arabanın baktığı yön)
            Vector3 launchDirection = player.getCarBody().transform.forward;

            // 3️⃣ Kuvvet uygulama (daha fiziksel, TCCA ile uyumlu)
            // Rigidbody'ye doğrudan hız set etmek yerine fiziksel itme uygula
            rb.AddForce(launchDirection * force, ForceMode.VelocityChange);

            // 4️⃣ Fırlatma sonrası hafif motor tepkisi (ivmelenme hissi)
            StartCoroutine(ApplyMotorBurst(1.0f, launchMotorBurstDuration));

            // 5️⃣ Kısa bir süre sonra rotasyon kilidini kaldır (havada dönmeye izin ver)
            StartCoroutine(ReleaseRotationLock(rb));
        }
         */
        public IEnumerator ApplyMotorBurst(float motorValue, float duration)
        {
            player.setMotor(motorValue);
            yield return new WaitForSeconds(duration);
            player.setMotor(0);
        }

        // Araba durdu mu kontrol eder ve durumu ayarlar.
        void CheckCarStatus()
        {
            // Arabanın hızı sıfıra yakınsa kontrol edilebilir duruma geç.
            if (!isDragging && player.getRigidbody().velocity.magnitude < STOPPING_THRESHOLD)
            {
                if (!isControllable)
                {
                    SetControllable(true);
                    player.immobilize(); // Hareketi tamamen durdur.
                }
            }
        }

        // Kontrol işaretini ayarlar.
        void SetControllable(bool c)
        {
            isControllable = c;
            if (controlMarkerObject != null)
            {
                // Not: SpriteRenderer'ı sizin atacağınız varsayılmıştır.
                // Burada sadece GameObject'i açıp kapatıyoruz.
                controlMarkerObject.SetActive(c);
            }
        }

        // Göstermelik tekerlek dönüşü için motorDelta'yı ayarlar.
        void SetCarSpin(float delta)
        {
            // TCCAPlayer'daki motorDelta, FixedUpdate'te tekerleklere iletilecektir.
            // Bu delta'nın arabanın hareket etmemesi için motor kodunda bir kontrol olmalı.
            // Ancak, TCCAPlayer.cs'yi değiştirmeden, sadece bu delta'yı ayarlayabiliriz.
            player.setMotor(delta);

            // *ÖNEMLİ NOT:* Bu, arabanın yavaş yavaş hareket etmesine neden olabilir.
            // Gösteri amaçlı tekerlek dönüşü için **TCCAWheel.cs** içinde bir `isStationarySpin` mantığı eklemek daha doğru olur, 
            // ancak şimdilik bu kalsın. Fırlatma anında TCCAPlayer'daki `motorDelta` sıfırlanmalıdır.
            if (!isControllable)
            {
                player.setMotor(0);
            }
        }

        // Fare pozisyonunu dünya koordinatlarına çevirir.
        Vector3 GetMouseWorldPosition()
        {
            // Bu, fareyi arabanın yüksekliğinde (ya da bir zemin düzleminde) yakalamak için basitleştirilmiş bir yaklaşımdır.
            // Gerçek Golf Battle gibi bir sistem için, bir düzlem (Plane) üzerine raycast yapmak gerekir.
            Plane plane = new Plane(Vector3.up, player.getCarBody().getPosition());
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance;

            if (plane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }

            return player.getCarBody().getPosition();
        }

        // Görsel güncellemeleri yönetir.
        void UpdateVisuals(Vector3 direction, float force)
        {
            float forceRatio = force / maxForce; // 0 ile 1 arasında kuvvet oranı

            // --- 1. DIRECTION VISUAL (Yön Görseli) ---
            if (directionLineRenderer != null)
            {
                // LineRenderer'ın başlangıç ve bitiş noktalarını ayarla
                // Başlangıç: Aracın önü (Görselin konumu)
                Vector3 startPoint = directionLineRenderer.transform.position;

                // Bitiş: Yön vektörü * Uzunluk
                float maxLineLength = maxDrawDistance * 2; // Çekme mesafesinin 2 katı uzunluk
                Vector3 endPoint = startPoint + player.getCarBody().transform.forward * maxLineLength;

                directionLineRenderer.SetPosition(0, startPoint);
                directionLineRenderer.SetPosition(1, endPoint);

                // Çizgi rotasyonu zaten aracın rotasyonuna (Quaternion.LookRotation ile ayarlandı) 
                // bağlı olmalı, bu yüzden burada ayrıca döndürmeye gerek yok.
            }

            // --- 2. FORCE VISUAL (Kuvvet Görseli) ---
            if (forceLineRenderer != null)
            {
                forceLineRenderer.gameObject.SetActive(true);

                // Geri Çekme Yönü
                Vector3 backwardsDirection = -player.getCarBody().transform.forward;

                // LineRenderer'ın uzunluğu, geri çekme mesafesiyle orantılı olmalıdır.
                float forceLineLength = forceRatio * maxDrawDistance;

                Vector3 centerPosition = player.getCarBody().getPosition();

                // Başlangıç noktası: Arabanın hemen arkası (0.5f birim gerisi)
                Vector3 startPoint = centerPosition + backwardsDirection * 0.5f;

                // Bitiş noktası: Başlangıç noktasından kuvvet çizgisi uzunluğu kadar gerisi
                Vector3 endPoint = startPoint + backwardsDirection * forceLineLength;

                // Yüksekliği koru
                float visualHeight = centerPosition.y + 0.5f;
                startPoint.y = visualHeight;
                endPoint.y = visualHeight;

                forceLineRenderer.SetPosition(0, startPoint);
                forceLineRenderer.SetPosition(1, endPoint);

                // Rotasyonu arabanın rotasyonu ile aynı yap (LineRenderer'ı düzgün göstermek için)
                forceLineRenderer.transform.rotation = player.getCarBody().getRotation();

                // Renk Değişimi (Yeşil -> Kırmızı)
                Color startColor = Color.green;
                Color endColor = Color.red;
                Color targetColor = Color.Lerp(startColor, endColor, forceRatio);

                forceLineRenderer.startColor = targetColor;
                forceLineRenderer.endColor = targetColor;

                // Kalınlık ayarı (Hissiyat için)
                forceLineRenderer.startWidth = 0.1f + forceRatio * 0.1f;
                forceLineRenderer.endWidth = 0.1f;
            }
        }

        // Fırlatıldıktan sonra sürtünme uygular (Hız Kesme)
        void ApplyFriction()
        {
            Rigidbody rb = player.getRigidbody();
            // **[DÜZELTME 3] SONDA DURMAMA ÇÖZÜMÜ:** Eğer araba yavaş ilerliyorsa, sürtünme yerine direkt durdur.
            if (rb != null && rb.velocity.magnitude < STOPPING_THRESHOLD)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                return;
            }

            // Yeterince hızlıysa ve yere temas ediyorsa sürtünmeyi uygula.
            // player.isFullyGrounded() metodu, TCCA sisteminizdeki TCCAPlayer.cs içinde mevcut olmalıdır.
            if (rb != null && player.isFullyGrounded())
            {
                rb.velocity *= frictionFactor;
                rb.angularVelocity *= frictionFactor;
            }
        }
    }
}