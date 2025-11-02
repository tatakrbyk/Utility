using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/*
 * 🚀 AddressableManager V3.0 - Production Ready
 * 
 * ✨ KEY FEATURES:
 * - Remote Config: Sunucudan preload/download stratejisi çekme
 * - Concurrency Limiter: Mobil için bandwidth kontrolü
 * - Reference Counting: Erken release önleme
 * - Auto Cleanup: 60 saniye kullanılmayanları otomatik temizleme
 * - Label Support: Batch asset loading
 * - Progress Tracking: Her operasyonda progress callback
 * - Memory Optimization: 64-capacity dictionaries, GC-free
 */

/// <summary>
/// 🚀 Production-Ready Addressable Manager V3.0
/// Remote Config + Concurrency Control + Auto Memory Management
/// </summary>
public class AddressableManager : Singleton<AddressableManager>
{
    #region Data Structures
    
    // Core tracking (64 capacity = less rehashing (don't resize with copy) = less GC)
    private Dictionary<string, AsyncOperationHandle> _cachedHandles = new Dictionary<string, AsyncOperationHandle>(64); /* key = UI_ICON_COIN, value = Handle(Access Result, Release etc.) */
    private Dictionary<string, int> _refCounts = new Dictionary<string, int>(64); //  Counts how many places an asset is used. for important relesae
    private Dictionary<GameObject, string> _instances = new Dictionary<GameObject, string>(64);
    private HashSet<string> _preloaded = new HashSet<string>();
    private Dictionary<string, float> _lastUsedTime = new Dictionary<string, float>(64);
    
    // Concurrency control
    private SemaphoreSlim _downloadLimiter;
    
    #endregion
    
    #region Settings
    
    [Header("⚙️ General Settings")]
    [SerializeField] private bool _autoCleanupOnSceneChange = true; // Automatically clean up unused assets when the scene changes
    [SerializeField] private int _maxConcurrentDownloads = 3; // Critical for mobile bandwidth,  It determines the maximum number of downloads that can be performed simultaneously(meaning async).
    
    [Header("☁️ Remote Config")]
    [SerializeField] private bool _enableRemotePreload = false;
    [SerializeField] private string _remoteConfigUrl = "https://your-cdn.com/config/preload_config.json"; // TODO(taha): If used remote downloand update link
    [Tooltip("Retry attempts if remote config fails")]
    [SerializeField] private int _remoteConfigRetries = 3;
    
    [Header("📊 Memory Management")]
    [SerializeField] private bool _autoUnloadUnused = true; // Start auto cleanup if enabled
    [SerializeField] private float _unusedAssetLifetime = 60f; // seconds
    [SerializeField] private float _memoryCheckInterval = 30f; // seconds
    
    #endregion
    
    #region Remote Config Data Structure
    
    [Serializable]
    public class PreloadConfig
    {
        public List<string> preloadKeys = new List<string>();     // Keys to preload at startup
        public List<string> downloadKeys = new List<string>();    // Keys to download at startup
        public List<string> preloadLabels = new List<string>();   // Labels to preload
        public int maxConcurrentDownloads = 3;                    // Override default
        public float unusedAssetLifetime = 60f;                   // Override default
    }
    
    #endregion
    
    #region Initialization
    
    protected override async UniTask OnInitializeAsync()
    {
        try
        {
            // Initialize Addressables
            await Addressables.InitializeAsync().ToUniTask();
            
            // Initialize concurrency limiter
            _downloadLimiter = new SemaphoreSlim(_maxConcurrentDownloads);
            
            Log("✓ Addressables initialized");
            
            // Load remote config if enabled
            if (_enableRemotePreload)
            {
                await LoadAndApplyRemoteConfig();
            }
            
            // Start auto cleanup if enabled
            if (_autoUnloadUnused)
            {
                StartMemoryMonitoring().Forget();
            }
        }
        catch (Exception e)
        {
            LogError($"Init failed: {e.Message}");
        }
    }
    
    #endregion
    
    #region Remote Config
    
    /// <summary>
    /// Load remote configuration from server
    /// </summary>
    private async UniTask LoadAndApplyRemoteConfig()
    {
        if (string.IsNullOrEmpty(_remoteConfigUrl))
        {
            LogError("Remote Config URL is empty");
            return;
        }
        
        Log($"Fetching Remote Config: {_remoteConfigUrl}");
        
        for (int attempt = 0; attempt < _remoteConfigRetries; attempt++)
        {
            try
            {
                using (var uwr = UnityWebRequest.Get(_remoteConfigUrl))
                {
                    // Add timeout
                    uwr.timeout = 10;
                    
                    await uwr.SendWebRequest().ToUniTask();
                    
                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        LogError($"Remote Config failed (attempt {attempt + 1}): {uwr.error}");
                        
                        if (attempt < _remoteConfigRetries - 1)
                        {
                            await UniTask.Delay(TimeSpan.FromSeconds(2));
                            continue;
                        }
                        return;
                    }
                    
                    string jsonText = uwr.downloadHandler.text;
                    var config = JsonUtility.FromJson<PreloadConfig>(jsonText);
                    
                    if (config != null)
                    {
                        // Apply remote settings
                        if (config.maxConcurrentDownloads > 0)
                        {
                            _maxConcurrentDownloads = config.maxConcurrentDownloads;
                            _downloadLimiter?.Dispose();
                            _downloadLimiter = new SemaphoreSlim(_maxConcurrentDownloads);
                            Log($"Max concurrent downloads set to: {_maxConcurrentDownloads}");
                        }
                        
                        if (config.unusedAssetLifetime > 0)
                        {
                            _unusedAssetLifetime = config.unusedAssetLifetime;
                            Log($"Unused asset lifetime set to: {_unusedAssetLifetime}s");
                        }
                        
                        // Download remote keys
                        if (config.downloadKeys != null && config.downloadKeys.Count > 0)
                        {
                            Log($"Starting download of {config.downloadKeys.Count} remote keys");
                            await DownloadAsync(config.downloadKeys, progress => 
                                Log($"Remote download progress: {progress * 100:F0}%"));
                        }
                        
                        // Preload remote keys
                        if (config.preloadKeys != null && config.preloadKeys.Count > 0)
                        {
                            Log($"Preloading {config.preloadKeys.Count} remote keys");
                            await PreloadAsync<UnityEngine.Object>(
                                config.preloadKeys.ToArray(),
                                progress => Log($"Remote preload progress: {progress * 100:F0}%"));
                        }
                        
                        // Preload remote labels
                        if (config.preloadLabels != null && config.preloadLabels.Count > 0)
                        {
                            foreach (var label in config.preloadLabels)
                            {
                                Log($"Preloading label: {label}");
                                await PreloadLabelAsync<UnityEngine.Object>(label);
                            }
                        }
                        
                        Log("✓ Remote Config applied successfully");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"Remote Config exception (attempt {attempt + 1}): {e.Message}");
                
                if (attempt < _remoteConfigRetries - 1)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }
        
        LogError("Remote Config failed after all retries");
    }
    
    #endregion
    
    #region Catalog Management
    
    /*
    bool hasUpdate = await AddressableManager.Instance.CheckForCatalogUpdates();
        
        if (hasUpdate)
        {
            ShowPopup("New content available!");
        }
    */
    /// <summary>
    /// Check for catalog updates (hot-update support)
    /// </summary>
    public async UniTask<bool> CheckForCatalogUpdates()
    {
        try
        {
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            var catalogs = await checkHandle.ToUniTask();
            
            if (catalogs != null && catalogs.Count > 0)
            {
                Log($"Found {catalogs.Count} catalog updates");
                
                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                await updateHandle.ToUniTask();
                
                Addressables.Release(updateHandle);
                Log("✓ Catalogs updated");
                return true;
            }
            
            Addressables.Release(checkHandle);
            return false;
        }
        catch (Exception e)
        {
            LogError($"Catalog update failed: {e.Message}");
            return false;
        }
    }
    
    #endregion
    
    #region Load Single Asset
    
    /*
        var sfx = await AddressableManager.Instance.LoadAsync<AudioClip>("SFX_Click");
        AudioSource.PlayClipAtPoint(sfx, Camera.main.transform.position);
    */
    /// <summary>
    /// Load asset with automatic reference counting
    /// </summary>
    public async UniTask<T> LoadAsync<T>(string key) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(key))
        {
            LogError("Key is null or empty");
            return null;
        }
        
        // Return cached with ref increment
        if (_cachedHandles.TryGetValue(key, out var handle))
        {
            AddRef(key);
            UpdateLastUsed(key);
            return handle.Result as T;
        }
        
        try
        {
            var op = Addressables.LoadAssetAsync<T>(key);
            var result = await op.ToUniTask();
            
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _cachedHandles[key] = op;
                _refCounts[key] = 1;
                UpdateLastUsed(key);
                Log($"✓ Loaded: {key}");
                return result;
            }
            else
            {
                LogError($"✗ Failed to load: {key}");
                return null;
            }
        }
        catch (Exception e)
        {
            LogError($"Exception loading {key}: {e.Message}");
            return null;
        }
    }
            
    // public class LoadingScreen : MonoBehaviour
    // {
    //     public Slider progressBar;
        
    //     private async void LoadBigAsset()
    //     {
    //         var asset = await AddressableManager.Instance.LoadAsync<GameObject>(
    //             "Boss_Dragon",
    //             progress => {
    //                 progressBar.value = progress; // 0.0 - 1.0
    //                 percentText.text = $"{progress * 100:F0}%";
    //             }
    //         );
    //     }
    // }
    /// <summary>
    /// Load asset with progress callback
    /// </summary>
    public async UniTask<T> LoadAsync<T>(string key, Action<float> onProgress) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(key))
            return null;
        
        // If cached, return immediately
        if (_cachedHandles.ContainsKey(key))
        {
            onProgress?.Invoke(1f);
            return await LoadAsync<T>(key);
        }
        
        try
        {
            var op = Addressables.LoadAssetAsync<T>(key);
            
            while (!op.IsDone)
            {
                onProgress?.Invoke(op.PercentComplete);
                await UniTask.Yield();
            }
            
            onProgress?.Invoke(1f);
            
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                _cachedHandles[key] = op;
                _refCounts[key] = 1;
                UpdateLastUsed(key);
                return op.Result;
            }
            
            return null;
        }
        catch (Exception e)
        {
            LogError($"Load failed: {e.Message}");
            return null;
        }
    }
    
    #endregion
    
    #region Load Multiple Assets
    
    
    
    //     private async void PreloadPowerUps()
    //     {
    //         var keys = new List<string> { 
    //             "PowerUp_Speed", 
    //             "PowerUp_Shield", 
    //             "PowerUp_Magnet" 
    //         };
            
    //         var powerUps = await AddressableManager.Instance.LoadMultipleAsync<GameObject>(
    //             keys,
    //             progress => Debug.Log($"Loading: {progress * 100:F0}%")
    //         );
            
    //         // powerUps[0] = Speed prefab
    //         // powerUps[1] = Shield prefab
    //         // powerUps[2] = Magnet prefab
    //     }

    /// <summary>
    /// Load multiple assets with progress
    /// </summary>
    public async UniTask<List<T>> LoadMultipleAsync<T>(IList<string> keys, Action<float> onProgress = null) where T : UnityEngine.Object
    {
        if (keys == null || keys.Count == 0)
            return new List<T>();
        
        var results = new List<T>(keys.Count);
        int completed = 0;
        
        foreach (var key in keys)
        {
            var asset = await LoadAsync<T>(key);
            results.Add(asset);
            
            completed++;
            onProgress?.Invoke((float)completed / keys.Count);
        }
        
        return results;
    }
    
    /*
    var assets = await AddressableManager.Instance.LoadByLabelAsync<GameObject>(
            $"Level_{levelNum:D2}",
            progress => loadingBar.value = progress
        );
    */
    /// <summary>
    /// Load all assets with specific label
    /// </summary>
    public async UniTask<List<T>> LoadByLabelAsync<T>(string label, Action<float> onProgress = null) where T : UnityEngine.Object
    {
        try
        {
            var op = Addressables.LoadAssetsAsync<T>(label, null);
            
            while (!op.IsDone)
            {
                onProgress?.Invoke(op.PercentComplete);
                await UniTask.Yield();
            }
            
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                var list = new List<T>(op.Result);
                
                // ✓ FIX: Handle'ı da cache'le!
                foreach (var asset in list)
                {
                    var key = $"{label}_{asset.name}";
                    
                    if (!_cachedHandles.ContainsKey(key))
                    {
                        // HANDLE'I CACHE'E EKLE
                        _cachedHandles[key] = op;
                        _refCounts[key] = 1;
                        UpdateLastUsed(key);
                    }
                    else
                    {
                        // Zaten cache'de varsa sadece ref arttır
                        AddRef(key);
                        UpdateLastUsed(key);
                    }
                }
                
                Log($"✓ Loaded label '{label}': {list.Count} assets");
                return list;
            }
            
            return new List<T>();
        }
        catch (Exception e)
        {
            LogError($"Label load failed: {e.Message}");
            return new List<T>();
        }
    }
    #endregion
    
    #region Preloading
    
      /*
        await AddressableManager.Instance.PreloadAsync<Sprite>(
            new[] { "UI_Icon_Coin", "UI_Icon_Gem", "UI_Icon_Star" },
            progress => loadingText.text = $"Loading UI: {progress * 100:F0}%"
        );
        
        // Artık bu icon'lar instant yüklenir
        var coin = AddressableManager.Instance.Get<Sprite>("UI_Icon_Coin"); // 0ms
    */
    /// <summary>
    /// Preload assets for instant access
    /// </summary>
    public async UniTask PreloadAsync<T>(string[] keys, Action<float> onProgress = null) where T : UnityEngine.Object
    {
        int total = keys.Length;
        int loaded = 0;
        
        foreach (var key in keys)
        {
            if (!_preloaded.Contains(key))
            {
                await LoadAsync<T>(key);
                _preloaded.Add(key);
            }
            
            loaded++;
            onProgress?.Invoke((float)loaded / total);
        }
        
        Log($"✓ Preloaded {loaded} assets");
    }
    
  /*
  // All "Essential" label preload 
        await AddressableManager.Instance.PreloadLabelAsync<GameObject>("Essential");
        
        // play game (essential assets ready)
        SceneManager.LoadScene("MainMenu");
  */
    /// <summary>
    /// Preload by label
    /// </summary>
    public async UniTask PreloadLabelAsync<T>(string label, Action<float> onProgress = null) where T : UnityEngine.Object
    {
        var assets = await LoadByLabelAsync<T>(label, onProgress);
        
        foreach (var asset in assets)
        {
            if (asset != null)
                _preloaded.Add($"{label}_{asset.name}");
        }
    }
    
    #endregion
    
    #region Download & Caching
    
    /*
    private async void LoadHalloweenEvent()
    {
        var keys = new List<string> { "Event_Halloween" };
        
        // Boyut kontrol
        var sizeBytes = await AddressableManager.Instance.GetDownloadSizeAsync(keys);
        float sizeMB = sizeBytes / (1024f * 1024f);
        
        if (sizeMB > 0)
        {
            bool accept = await ShowDownloadPrompt($"Download {sizeMB:F1}MB?");
            
            if (accept)
            {
                await AddressableManager.Instance.DownloadAsync(keys);
            }
        }
    }
    */
    /// <summary>
    /// Get download size for keys
    /// </summary>
    public async UniTask<long> GetDownloadSizeAsync(IList<string> keys)
    {
        try
        {
            var op = Addressables.GetDownloadSizeAsync((IEnumerable<object>)keys);
            var size = await op.ToUniTask();
            Addressables.Release(op);
            return size;
        }
        catch
        {
            return 0;
        }
    }
    
    /*
    private async void DownloadNextLevels()
    {
        var keys = new List<string> { "Level_10", "Level_11", "Level_12" };
        
        bool success = await AddressableManager.Instance.DownloadAsync(
            keys,
            progress => {
                downloadBar.value = progress;
                downloadText.text = $"Downloading: {progress * 100:F0}%";
            }
        );
        
        if (success)
        {
            Debug.Log("Levels downloaded, play offline!");
        }
    }
    */
    /// <summary>
    /// Download assets with concurrency control and progress
    /// </summary>
    public async UniTask<bool> DownloadAsync(IList<string> keys, Action<float> onProgress = null)
    {
        if (_downloadLimiter == null || keys == null || keys.Count == 0)
            return true;
        
        int totalKeys = keys.Count;
        int completedDownloads = 0;
        var downloadTasks = new List<UniTask<bool>>();
        
        foreach (var key in keys)
        {
            // Wait for semaphore slot
            await _downloadLimiter.WaitAsync();
            
            var task = UniTask.Create(async () =>
            {
                bool success = false;
                AsyncOperationHandle op = default;
                
                try
                {
                    op = Addressables.DownloadDependenciesAsync(key, false);
                    
                    while (!op.IsDone)
                    {
                        float currentProgress = (completedDownloads + op.PercentComplete) / totalKeys;
                        onProgress?.Invoke(currentProgress);
                        await UniTask.Yield();
                    }
                    
                    success = op.Status == AsyncOperationStatus.Succeeded;
                }
                catch (Exception e)
                {
                    LogError($"Download failed for {key}: {e.Message}");
                }
                finally
                {
                    Interlocked.Increment(ref completedDownloads);
                    if (op.IsValid())
                        Addressables.Release(op);
                    _downloadLimiter.Release();
                }
                
                return success;
            });
            
            downloadTasks.Add(task);
        }
        
        var results = await UniTask.WhenAll(downloadTasks);
        onProgress?.Invoke(1f);
        
        bool allSuccess = true;
        foreach (var result in results)
        {
            if (!result)
                allSuccess = false;
        }
        
        if (allSuccess)
            Log($"✓ All {totalKeys} downloads complete (Max Concurrent: {_maxConcurrentDownloads})");
        else
            LogError($"Some downloads failed out of {totalKeys}");
        
        return allSuccess;
    }
    
    /// <summary>
    /// TODO(taha): Make_Clear asset cache
    /// </summary>
    public void ClearCache()
    {
        Caching.ClearCache();
        Log("✓ Cache cleared");
    }
    
    #endregion
    
    #region Instantiate
    

    /*
     private async void SpawnEnemy()
    {
        var enemy = await AddressableManager.Instance.SpawnAsync(
            "Enemy_Zombie",
            spawnPoint.position,
            Quaternion.identity
        );
        
        enemy.GetComponent<Enemy>().SetTarget(player);
    }
    */
    /// <summary>
    /// Instantiate prefab with tracking
    /// </summary>
    public async UniTask<GameObject> SpawnAsync(string key, Transform parent = null)
    {
        try
        {
            var op = Addressables.InstantiateAsync(key, parent);
            var instance = await op.ToUniTask();
            
            if (instance != null)
            {
                _instances[instance] = key;
                AddRef(key);
                UpdateLastUsed(key);
                return instance;
            }
        }
        catch (Exception e)
        {
            LogError($"Spawn failed: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Spawn at position/rotation
    /// </summary>
    public async UniTask<GameObject> SpawnAsync(string key, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        try
        {
            var op = Addressables.InstantiateAsync(key, pos, rot, parent);
            var instance = await op.ToUniTask();
            
            if (instance != null)
            {
                _instances[instance] = key;
                AddRef(key);
                UpdateLastUsed(key);
                Log($"✓ Spawned: {key}");
                return instance;
            }
        }
        catch (Exception e)
        {
            LogError($"Spawn failed: {e.Message}");
        }
        
        return null;
    }
    
    /*
        AddressableManager.Instance.Despawn(gameObject);

    */
    /// <summary>
    /// Destroy and release instance
    /// </summary>
    public void Despawn(GameObject instance)
    {
        if (instance == null)
            return;
        
        if (_instances.TryGetValue(instance, out var key))
        {
            _instances.Remove(instance);
            RemoveRef(key);
            Addressables.ReleaseInstance(instance);
            Log($"✓ Despawned: {key}");
        }
        else
        {
            LogError("Instance not tracked by AddressableManager");
        }
    }
    
    #endregion
    
    #region Scene Management

/*
    public class LevelLoader : MonoBehaviour
    {
    private async void LoadLevel(int levelNum)
    {
        // Loading screen göster
        loadingScreen.SetActive(true);
        
        // Scene yükle
        var scene = await AddressableManager.Instance.LoadSceneAsync(
            $"Scene_Level_{levelNum:D2}",
            LoadSceneMode.Single,
            progress => loadingBar.value = progress
        );
        
        // Scene yüklendi
        loadingScreen.SetActive(false);
    }
}*/
    /// <summary>
    /// Load scene with progress
    /// </summary>
    public async UniTask<SceneInstance> LoadSceneAsync(string key, LoadSceneMode mode = LoadSceneMode.Additive, Action<float> onProgress = null)
    {
        try
        {
            var op = Addressables.LoadSceneAsync(key, mode);
            
            while (!op.IsDone)
            {
                onProgress?.Invoke(op.PercentComplete);
                await UniTask.Yield();
            }
            
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                AddRef(key);
                Log($"✓ Scene loaded: {key}");
                return op.Result;
            }
        }
        catch (Exception e)
        {
            LogError($"Scene load failed: {e.Message}");
        }
        
        return default;
    }
    
    /*
    public class LevelLoader : MonoBehaviour
{
    private SceneInstance _currentScene;
    
    private async void LoadLevel(int levelNum)
    {
        // Eski scene'i kaldır
        if (_currentScene.Scene.isLoaded)
        {
            await AddressableManager.Instance.UnloadSceneAsync(_currentScene);
        }
        
        // Yeni scene yükle
        _currentScene = await AddressableManager.Instance.LoadSceneAsync(
            $"Scene_Level_{levelNum:D2}"
        );
    }
}
    */
    /// <summary>
    /// Unload scene
    /// </summary>
    public async UniTask UnloadSceneAsync(SceneInstance scene)
    {
        try
        {
            await Addressables.UnloadSceneAsync(scene).ToUniTask();
            Log("✓ Scene unloaded");
        }
        catch (Exception e)
        {
            LogError($"Scene unload failed: {e.Message}");
        }
    }
    
    #endregion
    
    #region Reference Counting
    
    private void AddRef(string key)
    {
        if (_refCounts.ContainsKey(key))
            _refCounts[key]++;
        else
            _refCounts[key] = 1;
    }
    
    private void RemoveRef(string key)
    {
        if (!_refCounts.ContainsKey(key))
            return;
        
        _refCounts[key]--;
        
        if (_refCounts[key] <= 0)
            ForceRelease(key);
    }
    
    private void UpdateLastUsed(string key)
    {
        _lastUsedTime[key] = Time.unscaledTime;
    }
    
    #endregion
    
    #region Memory Management
    
    /// <summary>
    /// Release asset (respects ref count)
    /// </summary>
    public void Release(string key)
    {
        if (_cachedHandles.ContainsKey(key))
            RemoveRef(key);
    }
    
    /* AddressableManager.Instance.ForceRelease("Enemies");*/ 
    /// <summary>
    /// Force release (ignores ref count)
    /// </summary>
    public void ForceRelease(string key)
    {
        if (_cachedHandles.TryGetValue(key, out var handle))
        {
            if (handle.IsValid())
                Addressables.Release(handle);
            
            _cachedHandles.Remove(key);
            _refCounts.Remove(key);
            _preloaded.Remove(key);
            _lastUsedTime.Remove(key);
            Log($"Released: {key}");
        }
    }

    /*
    if ref count == 0 
        private async void LoadNextLevel()
    {
        // release unused assets
        AddressableManager.Instance.ReleaseUnused();
        
        // new level load
        await LoadLevel(currentLevel + 1);
    }
    */
    /// <summary>
    /// Release all unused assets (ref count = 0)
    /// </summary>
    public void ReleaseUnused()
    {
        var toRelease = new List<string>();
        
        foreach (var kvp in _refCounts)
        {
            if (kvp.Value <= 0)
                toRelease.Add(kvp.Key);
        }
        
        foreach (var key in toRelease)
            ForceRelease(key);
        
        if (toRelease.Count > 0)
            Log($"✓ Released {toRelease.Count} unused assets");
    }
    
    /// <summary>
    /// Release all assets
    /// </summary>
    public void ReleaseAll()
    {
        foreach (var handle in _cachedHandles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
        
        _cachedHandles.Clear();
        _refCounts.Clear();
        _instances.Clear();
        _preloaded.Clear();
        _lastUsedTime.Clear();
        
        Log("✓ Released all assets");
    }
    
    /// <summary>
    /// Auto cleanup old assets
    /// </summary>
    private async UniTaskVoid StartMemoryMonitoring()
    {
        while (Application.isPlaying)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_memoryCheckInterval));
            
            var now = Time.unscaledTime;
            var toRelease = new List<string>();
            
            foreach (var kvp in _lastUsedTime)
            {
                if (now - kvp.Value > _unusedAssetLifetime)
                {
                    if (_refCounts.TryGetValue(kvp.Key, out var refs) && refs <= 0)
                    {
                        toRelease.Add(kvp.Key);
                    }
                }
            }
            
            foreach (var key in toRelease)
                ForceRelease(key);
            
            if (toRelease.Count > 0)
                Log($"Auto-released {toRelease.Count} old assets");
        }
    }
    
    #endregion
    
    #region Utilities
    
    /// <summary>
    /// Check if asset is loaded
    /// </summary>
    public bool IsLoaded(string key) => _cachedHandles.ContainsKey(key);
    
    /// <summary>
    /// Get reference count
    /// </summary>
    public int GetRefCount(string key)
    {
        return _refCounts.TryGetValue(key, out var count) ? count : 0;
    }
    
    /* AddressableManager.Instance.Get<Sprite>("UI_Icon_Coin");*/
    /// <summary>
    /// Get cached asset
    /// </summary>
    public T Get<T>(string key) where T : UnityEngine.Object
    {
        if (_cachedHandles.TryGetValue(key, out var handle))
        {
            UpdateLastUsed(key);
            return handle.Result as T;
        }
        return null;
    }
    
    /// <summary>
    /// Get memory usage stats
    /// </summary>
    public string GetStats()
    {
        return $"Loaded: {_cachedHandles.Count} | Instances: {_instances.Count} | Preloaded: {_preloaded.Count} | RefCounts: {_refCounts.Count}";
    }
    
    /// <summary>
    /// Get detailed stats
    /// </summary>
    public Dictionary<string, int> GetDetailedStats()
    {
        return new Dictionary<string, int>
        {
            { "CachedHandles", _cachedHandles.Count },
            { "RefCounts", _refCounts.Count },
            { "Instances", _instances.Count },
            { "Preloaded", _preloaded.Count },
            { "LastUsedTracking", _lastUsedTime.Count }
        };
    }
    
    #endregion
    
    #region Scene Events
    
    // call: Singleton 
    // Automatically clean up unused assets when the scene changes
    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_autoCleanupOnSceneChange && mode == LoadSceneMode.Single)
        {
            ReleaseUnused();
            Log($"Auto-cleanup on scene load: {scene.name}");
        }
    }
    
    #endregion
    
    #region Logging
    
    private void Log(string msg)
    {
        Debug.Log($"[AddressableManager] {msg}");
    }
    
    private void LogError(string msg)
    {
        Debug.LogError($"[AddressableManager] {msg}");
    }
    
    #endregion
    
    #region Cleanup
    
    protected override void OnDestroy()
    {
        ReleaseAll();
        _downloadLimiter?.Dispose();
        base.OnDestroy();
    }
    
    #endregion
}