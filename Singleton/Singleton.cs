using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Requires: 
 * Load UnitTask : "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
 */
/// <summary>
/// ⚡ UniTask, async & scene aware Singleton.
/// - Mobile optimize (GC-free)
/// - DontDestroyOnLoad 
/// - SceneLoaded event suppoet
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    [SerializeField] private bool _isPersistent = true;
    private bool _initialized = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
                return null;

            if (_instance != null)
                return _instance;

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        var go = new GameObject(typeof(T).Name + " (Singleton)");
                        _instance = go.AddComponent<T>();
                    }

                    if (_instance is Singleton<T> s && s._isPersistent)
                        DontDestroyOnLoad(s.gameObject);
                }
                return _instance;
            }
        }
    }

/*
    protected virtual async void Awake()
{
    if (_instance == null)
    {
        _instance = this as T;
        if (_isPersistent) DontDestroyOnLoad(gameObject);
        await InitializeAsync(); // otomatik çağrı
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }
    else if (_instance != this)
    {
        Destroy(gameObject);
    }
}
*/

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (_isPersistent)
                DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

/*
    Using Script call: 
    private async void Start()
{
    await MyManager.Instance.InitializeAsync();
}

*/
    public async UniTask InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;
        await OnInitializeAsync();
    }

    /// <summary>
    /// Override async initialization.
    /// </summary>
    protected virtual UniTask OnInitializeAsync() => UniTask.CompletedTask;

    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_applicationIsQuitting)
            OnSceneLoaded(scene, mode);
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            _instance = null;
        }
    }
}
