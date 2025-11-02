using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks; // Singleton için gerekli
using UnityEngine.Purchasing;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

#if UNITY_ANDROID
using UnityEngine.Android;
using Unity.Notifications.Android; // Yerel Bildirimler için
#elif UNITY_IOS
using Unity.Notifications.iOS; // Yerel Bildirimler için
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using Firebase.RemoteConfig;
using Firebase.Messaging;
using System; // TimeSpan için
#endif


// ================================================================
// ⛳️ MobileManager Sınıfı
// ================================================================
public class MobileManager : Singleton<MobileManager>,
    IStoreListener,
    IUnityAdsInitializationListener,
    IUnityAdsLoadListener,
    IUnityAdsShowListener
{
    // ================================================================
    // ⬇️ REKLAM & IAP ID'leri ⬇️
    // ================================================================
#if UNITY_ANDROID
    private const string GameId = "4000000"; // GolfBattle Android ID'niz (DEĞİŞTİRİN!)
    private const string InterstitialAdId = "Interstitial_Android";
    private const string RewardedAdId = "Rewarded_Android";
#elif UNITY_IOS
    private const string GameId = "4000001"; // GolfBattle iOS ID'niz (DEĞİŞTİRİN!)
    private const string InterstitialAdId = "Interstitial_iOS";
    private const string RewardedAdId = "Rewarded_iOS";
#endif
    private const bool TestMode = true; // Yayınlarken FALSE yapmayı UNUTMAYIN!

    // IAP Ürün ID'leri (Google Play / App Store ile eşleşmeli)
    private const string ProductIdRemoveAds = "golfbattle_remove_ads";
    private const string ProductIdCoinPack = "golfbattle_big_coin_pack";

    // ================================================================
    // ⚙️ SERVİS & VERİ DEĞİŞKENLERİ
    // ================================================================
    private static IStoreController _storeController;
    private static IExtensionProvider _storeExtension;
    private bool _isAdsInitialized = false;
    private bool _isIAPInitialized = false;
    private bool _isFirebaseInitialized = false;

    // Async başlangıç metodu (Singleton'dan override edildi)
    protected override async UniTask OnInitializeAsync()
    {
        Debug.Log("🚀 MobileManager ASYNC Başlatılıyor...");

        // Önce kritik ayarları yap
        ConfigureScreen();
        ConfigureQualityBasedOnDevice();

        // Servisleri asenkron olarak başlat
        await UniTask.WhenAll(
            InitializeIAPAsync(),
            InitializeUnityAdsAsync(),
#if FIREBASE_ENABLED
            InitializeFirebaseAsync()
#else
            UniTask.CompletedTask
#endif
        );

        // Diğer senkron başlatmalar
#if FIREBASE_ENABLED
        InitializeFCM();
#endif
        
        Debug.Log("✅ MobileManager Başlatma Tamamlandı.");
    }
    
    // Uygulama duruş/odaklanma durumları
    void OnApplicationPause(bool paused) => Debug.Log(paused ? "⏸️ Oyun durdu" : "▶️ Oyun aktif");
    void OnApplicationFocus(bool focus) => Debug.Log($"🔵 Odak: {focus}");
    protected override void OnApplicationQuit() 
    {
        Debug.Log("🔴 Uygulama kapanıyor...");
        PlayerPrefs.Save(); // Çıkışta tüm PlayerPrefs verilerinin kaydedildiğinden emin ol
        base.OnApplicationQuit();
    }

    /* ================================================================
        1. UYGULAMA & SİSTEM BİLGİLERİ & KONFİGÜRASYON
    ================================================================ */
    
    [ContextMenu("Print App Info")]
    void PrintAppInfo()
    {
        Debug.Log($"📱 {Application.productName} v{Application.version} ({Application.companyName})");
        Debug.Log($"Platform: {Application.platform}, Internet: {(Application.internetReachability != NetworkReachability.NotReachable)}");
        Debug.Log($"Paths: Data={Application.dataPath}, Save={Application.persistentDataPath}");
        Debug.Log($"🔋 {SystemInfo.deviceModel} | OS: {SystemInfo.operatingSystem}");
        Debug.Log($"CPU: {SystemInfo.processorType} ({SystemInfo.processorCount} cores), RAM: {SystemInfo.systemMemorySize}MB");
    }

    // [ContextMenu] ile Editör'den çalıştırılabilir
    [ContextMenu("Configure Screen")]
    void ConfigureScreen()
    {
        Screen.orientation = ScreenOrientation.Portrait; // GolfBattle için dikey varsayılır
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // VSync kapalı, FPS'i targetFrameRate kontrol etsin
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Cihazın RAM'ine göre dinamik kalite ayarı
    void ConfigureQualityBasedOnDevice()
    {
        int ramMB = SystemInfo.systemMemorySize;
        int targetQuality = 2; // Medium

        if (ramMB >= 4096) 
        {
            targetQuality = 3; // High
        }
        else if (ramMB < 2048) 
        {
            targetQuality = 0; // Low
        }

        QualitySettings.SetQualityLevel(targetQuality, true);
        Debug.Log($"⚙️ Kalite Ayarı: {QualitySettings.names[targetQuality]} ({ramMB}MB RAM)");
    }
    
    /* ================================================================
        2. İNTERNET KONTROLÜ
    ================================================================ */
    
    public async UniTask<bool> CheckInternetAsync()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("❌ Offline (Application Reachability)");
            return false;
        }

        var req = UnityEngine.Networking.UnityWebRequest.Get("https://google.com");
        await req.SendWebRequest().ToUniTask();

        bool isOnline = req.result == UnityEngine.Networking.UnityWebRequest.Result.Success;
        Debug.Log(isOnline ? "✅ Online (Request Check)" : "❌ Offline (Request Failed)");
        return isOnline;
    }

    /* ================================================================
        3. ANDROID İZİNLERİ
    ================================================================ */
    
    // Örn: Oyuncunun mikrofon ile sesli sohbet yapması gerekiyorsa.
    public void RequestPermission(string permission)
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(permission))
        {
            Permission.RequestUserPermission(permission);
            Debug.Log($"📲 Android izin istendi: {permission}");
        }
#endif
    }
    
    /* ================================================================
        4. PLAYER PREFS YÖNETİMİ
    ================================================================ */
    // NOT: PlayerPrefs basit veri kaydı için kullanılır. Hassas veriler için (para, skor vb.) şifreleme önerilir.
    
    [ContextMenu("Save & Load Player Data")]
    public void ManagePlayerData()
    {
        PlayerPrefs.SetInt("PlayerCoins", 500); // Örnek: Coin miktarı
        PlayerPrefs.SetString("LastPlayedScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        
        Debug.Log($"🧠 Coin: {PlayerPrefs.GetInt("PlayerCoins")} | Son Sahne: {PlayerPrefs.GetString("LastPlayedScene")}");
    }

    /* ================================================================
        5. IN-APP PURCHASE (IAP) ASYNC
    ================================================================ */

    private UniTask InitializeIAPAsync()
    {
        if (_isIAPInitialized) return UniTask.CompletedTask;
        
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(ProductIdRemoveAds, ProductType.NonConsumable);
        builder.AddProduct(ProductIdCoinPack, ProductType.Consumable);
        
        UnityPurchasing.Initialize(this, builder);
        _isIAPInitialized = true;
        
        Debug.Log("🛒 IAP Başlatma tetiklendi.");
        return UniTask.CompletedTask;
    }

    public void BuyProduct(string productId)
    {
        if (!_isIAPInitialized || _storeController == null)
        {
            Debug.LogWarning("🛒 IAP henüz hazır değil veya kontrolcü null.");
            return;
        }
        _storeController.InitiatePurchase(productId);
    }
    
    // IStoreListener Metotları
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        _storeExtension = extensions;
        Debug.Log("✅ IAP hazır: Ürünler yüklendi.");
    }
    public void OnInitializeFailed(InitializationFailureReason error) => Debug.LogError($"❌ IAP Hatası: {error}");
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason) => Debug.LogError($"❌ Satın alma hatası: {product.definition.id} - {reason}");
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == ProductIdRemoveAds)
        {
            PlayerPrefs.SetInt("HasRemoveAds", 1);
            LogEvent("purchase_remove_ads");
        }
        else if (args.purchasedProduct.definition.id == ProductIdCoinPack)
        {
            // Oyuncuya parayı ekle (Örn: +1000 Coin)
            LogEvent("purchase_coin_pack", new Dictionary<string, object> { { "coins_given", 1000 } });
        }
        return PurchaseProcessingResult.Complete;
    }

    /* ================================================================
        6. UNITY ADS ASYNC
    ================================================================ */

    private UniTask InitializeUnityAdsAsync()
    {
        if (_isAdsInitialized) return UniTask.CompletedTask;
        Advertisement.Initialize(GameId, TestMode, this);
        return UniTask.CompletedTask;
    }
    
    public void LoadInterstitialAd() => Advertisement.Load(InterstitialAdId, this);
    public void LoadRewardedAd() => Advertisement.Load(RewardedAdId, this);

    public void ShowAd(string adId)
    {
        if (!_isAdsInitialized || !Advertisement.IsReady(adId))
        {
            Debug.LogWarning($"📺 Reklam gösterilemiyor. ID: {adId}. Yüklü mü: {Advertisement.IsReady(adId)}");
            return;
        }
        Advertisement.Show(adId, this);
    }
    
    // IUnityAdsInitializationListener
    public void OnInitializationComplete()
    {
        _isAdsInitialized = true;
        Debug.Log("✅ Unity Ads Başlatıldı. Reklamlar yükleniyor...");
        LoadInterstitialAd();
        LoadRewardedAd();
    }
    public void OnInitializationFailed(UnityAdsInitializationError error, string message) => Debug.LogError($"❌ Unity Ads Hatası: {error} - {message}");

    // IUnityAdsLoadListener
    public void OnUnityAdsAdLoaded(string placementId) => Debug.Log($"📺 Reklam yüklendi: {placementId}");
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message) => Debug.LogError($"❌ Reklam yüklenemedi: {error} - {message}");

    // IUnityAdsShowListener
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) => Debug.LogError($"❌ Gösterim hatası: {error} - {message}");
    public void OnUnityAdsShowStart(string placementId) => Debug.Log($"▶️ Reklam başladı: {placementId}");
    public void OnUnityAdsShowClick(string placementId) => Debug.Log($"🖱️ Reklam tıklandı: {placementId}");
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState state)
    {
        Debug.Log($"✅ Reklam tamamlandı: {placementId} - {state}");
        
        if (placementId == RewardedAdId && state == UnityAdsShowCompletionState.COMPLETED)
        {
            // GolfBattle Ödül Mantığı: Oyuncuya ödül ver (Örn: +1 Top veya Can)
            LogEvent("reward_granted", new Dictionary<string, object> { { "reward_type", "extra_ball" } });
        }
        
        // Yenisini yükle
        if (placementId == InterstitialAdId) LoadInterstitialAd();
        else if (placementId == RewardedAdId) LoadRewardedAd();
    }

    /* ================================================================
        7. FIREBASE ANALYTICS / REMOTE CONFIG ASYNC
    ================================================================ */
#if FIREBASE_ENABLED
    private async UniTask InitializeFirebaseAsync()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status == DependencyStatus.Available)
        {
            _isFirebaseInitialized = true;
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            Crashlytics.IsCrashlyticsCollectionEnabled = true;

            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromHours(1));
            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
            
            LogEvent("game_start");
            Debug.Log("✅ Firebase ve Remote Config Hazır.");
        }
        else
        {
            Debug.LogError($"❌ Firebase hatası: {status}");
        }
    }

    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (!_isFirebaseInitialized) return;

        if (parameters != null)
        {
            var firebaseParams = new Parameter[parameters.Count];
            int i = 0;
            foreach (var kv in parameters)
            {
                // Değer tipine göre doğru Firebase Parameter tipini seç
                if (kv.Value is string) firebaseParams[i] = new Parameter(kv.Key, kv.Value.ToString());
                else if (kv.Value is int) firebaseParams[i] = new Parameter(kv.Key, (long)(int)kv.Value);
                else if (kv.Value is float) firebaseParams[i] = new Parameter(kv.Key, (double)(float)kv.Value);
                else firebaseParams[i] = new Parameter(kv.Key, kv.Value.ToString());
                i++;
            }
            FirebaseAnalytics.LogEvent(eventName, firebaseParams);
        }
        else FirebaseAnalytics.LogEvent(eventName);

        Debug.Log($"📊 Analytics event loglandı: {eventName}");
    }
#endif

    /* ================================================================
        8. PUSH NOTIFICATIONS (Yerel & FCM)
    ================================================================ */
    
#if FIREBASE_ENABLED
    private void InitializeFCM()
    {
        FirebaseMessaging.TokenReceived += (sender, token) => Debug.Log($"🆔 FCM Token: {token.Token}");
        FirebaseMessaging.MessageReceived += (sender, e) => Debug.Log($"📩 FCM Mesajı: {e.Message.Notification?.Title}");
        Debug.Log("🔔 Firebase Cloud Messaging başlatıldı");
    }
#endif
    
    [ContextMenu("Send Local Notification (10s)")]
    public void SendLocalNotification()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel("golf_channel", "Golf Battle Bildirimleri", Importance.Default, "Golf Battle'dan gelen genel bildirimler");
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        var notification = new AndroidNotification("Geri Dön Golf Kralı!", "Sıra sende! Yeni top ve ödüller seni bekliyor!", System.DateTime.Now.AddSeconds(10));
        AndroidNotificationCenter.SendNotification(notification, "golf_channel");
#elif UNITY_IOS
        iOSNotificationCenter.RequestAuthorization(AuthorizationOption.Alert | AuthorizationOption.Badge, (auth) => {
            if (auth)
            {
                var timeTrigger = new iOSNotificationTimeIntervalTrigger(10, false);
                var notification = new iOSNotification("_golf_notif", "Geri Dön Golf Kralı!", "Sıra sende! Yeni top ve ödüller seni bekliyor!", timeTrigger);
                iOSNotificationCenter.ScheduleNotification(notification);
            }
        });
#endif
        Debug.Log("🔔 Yerel bildirim planlandı (10sn sonra).");
    }
}

/* ================================================================
    🔹 Yardımcı: Dictionary -> JSON Serialization (Log kaydı için)
================================================================ */
[System.Serializable]
public class SerializableDictionary
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();

    public SerializableDictionary(Dictionary<string, string> dict)
    {
        foreach (var kv in dict)
        {
            keys.Add(kv.Key);
            values.Add(kv.Value);
        }
    }
}