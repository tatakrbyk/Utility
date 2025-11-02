#region  test
# 📡 Remote Config - Complete Setup Guide

## 🎯 Ne İşe Yarar?

Remote Config ile **oyunu yeniden build etmeden**:
- ✅ Hangi assetlerin preload edileceğini değiştir
- ✅ Download stratejisini optimize et
- ✅ A/B testing yap
- ✅ Event content'i kontrol et
- ✅ Memory settings'i ayarla

---

## 📋 JSON Örnekleri

### 1️⃣ Basic Config (Başlangıç)

```json
{
  "preloadKeys": [
    "UI_Icon_Coin",
    "UI_Icon_Gem",
    "Audio_Click"
  ],
  "downloadKeys": [],
  "preloadLabels": [
    "Essential"
  ],
  "maxConcurrentDownloads": 3,
  "unusedAssetLifetime": 60
}
```

**Kullanım:** Oyun başladığında sadece essential UI ve audio yükle.

---

### 2️⃣ Event-Based Config (Halloween Event)

```json
{
  "preloadKeys": [
    "UI_Icon_Coin",
    "UI_Icon_Gem"
  ],
  "downloadKeys": [
    "Event_Halloween_Boss",
    "Event_Halloween_Levels"
  ],
  "preloadLabels": [
    "Essential",
    "Event_Halloween"
  ],
  "maxConcurrentDownloads": 2,
  "unusedAssetLifetime": 45
}
```

**Kullanım:** Halloween event döneminde otomatik olarak event content'i indir.

---

### 3️⃣ A/B Testing Config

**Group A (Aggressive Preload)**
```json
{
  "preloadKeys": [
    "UI_Icon_Coin",
    "UI_Icon_Gem",
    "UI_Icon_Star",
    "Audio_Click",
    "Audio_Win",
    "Enemy_Zombie",
    "Enemy_Skeleton",
    "PowerUp_Speed",
    "PowerUp_Shield"
  ],
  "downloadKeys": [
    "Level_01",
    "Level_02",
    "Level_03"
  ],
  "preloadLabels": [
    "Essential",
    "Gameplay_Common"
  ],
  "maxConcurrentDownloads": 5,
  "unusedAssetLifetime": 90
}
```

**Group B (Minimal Preload)**
```json
{
  "preloadKeys": [
    "UI_Icon_Coin",
    "Audio_Click"
  ],
  "downloadKeys": [],
  "preloadLabels": [
    "Essential"
  ],
  "maxConcurrentDownloads": 2,
  "unusedAssetLifetime": 30
}
```

**Analytics ile ölç:**
- Retention rate
- Average session length
- Load time
- Memory usage

---

### 4️⃣ Low-End Device Config

```json
{
  "preloadKeys": [
    "UI_Icon_Coin"
  ],
  "downloadKeys": [],
  "preloadLabels": [],
  "maxConcurrentDownloads": 1,
  "unusedAssetLifetime": 20
}
```

**Kullanım:** 1GB RAM altı cihazlar için aggressive cleanup.

---

### 5️⃣ High-End Device Config

```json
{
  "preloadKeys": [
    "UI_Icon_Coin",
    "UI_Icon_Gem",
    "UI_Icon_Star",
    "Audio_Click",
    "Audio_Win",
    "Audio_Lose"
  ],
  "downloadKeys": [
    "Level_01",
    "Level_02",
    "Level_03",
    "Level_04",
    "Level_05"
  ],
  "preloadLabels": [
    "Essential",
    "Gameplay_Common",
    "UI_Extended"
  ],
  "maxConcurrentDownloads": 10,
  "unusedAssetLifetime": 300
}
```

**Kullanım:** 4GB+ RAM cihazlar için maximum preload.

---

### 6️⃣ Region-Based Config

**North America**
```json
{
  "preloadKeys": ["UI_EN", "Audio_EN"],
  "preloadLabels": ["Region_NA"],
  "maxConcurrentDownloads": 5,
  "unusedAssetLifetime": 60
}
```

**Asia**
```json
{
  "preloadKeys": ["UI_ZH", "Audio_ZH"],
  "preloadLabels": ["Region_Asia"],
  "maxConcurrentDownloads": 3,
  "unusedAssetLifetime": 45
}
```

---

## 🛠️ CDN Setup

### 1. Klasör Yapısı

```
your-cdn.com/
├── config/
│   ├── preload_config.json          (Default)
│   ├── preload_config_ab_test_a.json
│   ├── preload_config_ab_test_b.json
│   ├── preload_config_lowend.json
│   ├── preload_config_highend.json
│   └── preload_config_event.json
├── addressables/
│   ├── Android/
│   │   ├── catalog.json
│   │   └── [bundles]
│   └── iOS/
│       ├── catalog.json
│       └── [bundles]
```

### 2. CDN Headers

```http
Content-Type: application/json
Cache-Control: public, max-age=300
Access-Control-Allow-Origin: *
ETag: "version-1.0.0"
```

**Cache Control:**
- `max-age=300` (5 minutes) - Frequent updates
- `max-age=3600` (1 hour) - Stable config
- `no-cache` - Debug mode

---

## 🔧 Unity Integration

### Basic Setup

```csharp
// Inspector'dan ayarla
[Header("☁️ Remote Config")]
[SerializeField] private bool _enableRemotePreload = true;
[SerializeField] private string _remoteConfigUrl = "https://your-cdn.com/config/preload_config.json";
[SerializeField] private int _remoteConfigRetries = 3;
```

### Dynamic URL (A/B Testing)

```csharp
public class RemoteConfigManager : MonoBehaviour
{
    private string GetConfigUrl()
    {
        // A/B testing
        string abGroup = PlayerPrefs.GetString("AB_Group", "A");
        string baseUrl = "https://your-cdn.com/config/";
        
        switch (abGroup)
        {
            case "A":
                return baseUrl + "preload_config_ab_test_a.json";
            case "B":
                return baseUrl + "preload_config_ab_test_b.json";
            default:
                return baseUrl + "preload_config.json";
        }
    }
    
    private async void Start()
    {
        // Set config URL
        var mgr = AddressableManager.Instance;
        // mgr._remoteConfigUrl = GetConfigUrl(); // Set before init
        
        await mgr.InitializeAsync();
    }
}
```

### Device-Based Config

```csharp
public class DeviceBasedConfig : MonoBehaviour
{
    private string GetConfigUrlByDevice()
    {
        string baseUrl = "https://your-cdn.com/config/";
        
        // Check RAM
        int ramMB = SystemInfo.systemMemorySize;
        
        if (ramMB < 2048) // < 2GB
            return baseUrl + "preload_config_lowend.json";
        else if (ramMB > 4096) // > 4GB
            return baseUrl + "preload_config_highend.json";
        else
            return baseUrl + "preload_config.json";
    }
}
```

### Region-Based Config

```csharp
public class RegionBasedConfig : MonoBehaviour
{
    private async UniTask<string> GetConfigUrlByRegion()
    {
        string baseUrl = "https://your-cdn.com/config/";
        
        // Get region from IP (example)
        string region = await GetUserRegion();
        
        switch (region)
        {
            case "NA":
                return baseUrl + "preload_config_na.json";
            case "EU":
                return baseUrl + "preload_config_eu.json";
            case "Asia":
                return baseUrl + "preload_config_asia.json";
            default:
                return baseUrl + "preload_config.json";
        }
    }
    
    private async UniTask<string> GetUserRegion()
    {
        // Use IP geolocation service
        using (var www = UnityWebRequest.Get("https://ipapi.co/region/"))
        {
            await www.SendWebRequest();
            return www.downloadHandler.text;
        }
    }
}
```

---

## 📊 Analytics Integration

### Track Remote Config Performance

```csharp
public class RemoteConfigAnalytics : MonoBehaviour
{
    private async void Start()
    {
        var mgr = AddressableManager.Instance;
        
        // Track before
        var startTime = Time.realtimeSinceStartup;
        var startMemory = Profiler.GetTotalAllocatedMemoryLong();
        
        // Initialize (remote config loads here)
        await mgr.InitializeAsync();
        
        // Track after
        var loadTime = Time.realtimeSinceStartup - startTime;
        var memoryUsed = Profiler.GetTotalAllocatedMemoryLong() - startMemory;
        
        // Send to analytics
        Analytics.CustomEvent("RemoteConfig_Loaded", new Dictionary<string, object>
        {
            { "load_time", loadTime },
            { "memory_mb", memoryUsed / (1024f * 1024f) },
            { "config_url", mgr._remoteConfigUrl },
            { "device_ram", SystemInfo.systemMemorySize }
        });
    }
}
```

### A/B Test Results

```csharp
public class ABTestTracker : MonoBehaviour
{
    private void TrackGameplay()
    {
        string abGroup = PlayerPrefs.GetString("AB_Group");
        
        Analytics.CustomEvent("Gameplay_Session", new Dictionary<string, object>
        {
            { "ab_group", abGroup },
            { "session_length", sessionTime },
            { "levels_completed", levelsCompleted },
            { "memory_peak", peakMemory }
        });
    }
}
```

---

## 🧪 Testing

### Local Testing

1. **Create test JSON file:**
```json
{
  "preloadKeys": ["Test_Asset"],
  "downloadKeys": [],
  "preloadLabels": [],
  "maxConcurrentDownloads": 1,
  "unusedAssetLifetime": 10
}
```

2. **Host locally with Python:**
```bash
python -m http.server 8000
```

3. **Set URL in Unity:**
```
http://localhost:8000/preload_config.json
```

### Production Testing

1. **Deploy to staging:**
```
https://staging-cdn.com/config/preload_config.json
```

2. **Test with small user group (5%)**

3. **Monitor metrics:**
   - Load time
   - Crash rate
   - Memory usage
   - User retention

4. **Gradually roll out (5% → 25% → 50% → 100%)**

---

## 🚨 Error Handling

### What if remote config fails?

**The system is safe:**
```csharp
// Remote config is optional
if (_enableRemotePreload)
{
    await LoadAndApplyRemoteConfig();
    // If fails, game continues normally
}
```

**Fallback strategy:**
1. Retry 3 times (configurable)
2. If still fails, use default settings
3. Log error for analytics
4. Game works perfectly without remote config

### Debug Mode

```csharp
[SerializeField] private bool _enableDetailedLogs = true;

// Logs:
// "Fetching Remote Config: https://..."
// "Remote Config applied successfully"
// "Remote Config failed (attempt 1): timeout"
```

---

## 💡 Best Practices

### 1. Start Conservative
```json
{
  "preloadKeys": ["UI_Icon_Coin", "Audio_Click"],
  "maxConcurrentDownloads": 2
}
```

### 2. Monitor and Optimize
- Check analytics
- Increase preload gradually
- A/B test changes

### 3. Event-Based Updates
```json
// During Christmas event
{
  "preloadKeys": [...christmas assets...],
  "preloadLabels": ["Event_Christmas"]
}

// After event
{
  "preloadKeys": [...normal assets...],
  "preloadLabels": ["Essential"]
}
```

### 4. Cache Busting
```
Add query param: ?v=1.0.1
https://cdn.com/config/preload_config.json?v=1.0.1
```

### 5. Versioning
```json
{
  "version": "1.0.1",
  "minGameVersion": "1.0.0",
  "preloadKeys": [...]
}
```

---

## 🎯 Common Use Cases

### Use Case 1: Seasonal Events
**Problem:** Halloween event bitince gereksiz assetler bellekte  
**Solution:** Remote config ile event bitince preload listesini güncelle

### Use Case 2: New Level Release
**Problem:** Yeni level eklenince herkes download etsin  
**Solution:** `downloadKeys`'e ekle, kullanıcılar otomatik indirir

### Use Case 3: Performance Issues
**Problem:** Low-end cihazlarda memory sorunları  
**Solution:** Device detection + farklı config URL'leri

### Use Case 4: Regional Content
**Problem:** Her bölgede farklı content  
**Solution:** IP-based config selection

### Use Case 5: Beta Testing
**Problem:** Yeni feature'ı önce test et  
**Solution:** Beta group için ayrı config URL

---

## 📈 Success Metrics

Monitor these after remote config deployment:

- **Load Time:** Should decrease with better preload
- **Memory Usage:** Should be optimal for device
- **Crash Rate:** Should not increase
- **Retention Rate:** Should improve with optimizations
- **Download Size:** Should match expectations

---

## 🔐 Security

### CDN Security
- Use HTTPS (required)
- CORS headers set correctly
- Rate limiting on CDN
- DDoS protection

### JSON Validation
```csharp
// System validates JSON automatically
// Invalid JSON = fallback to defaults
```

### Version Control
```json
{
  "minGameVersion": "1.5.0",
  "preloadKeys": [...]
}

// Game version 1.4.0 will ignore this config
```

---

## ✅ Quick Start Checklist

- [ ] Create JSON file
- [ ] Upload to CDN
- [ ] Set HTTPS URL in Unity
- [ ] Enable remote preload
- [ ] Test locally first
- [ ] Deploy to staging
- [ ] Monitor metrics
- [ ] Roll out gradually

---

## 🎁 Bonus: Server-Side Logic

**Advanced: Dynamic config per user**

```javascript
// Node.js example
app.get('/config/preload_config.json', (req, res) => {
  const userId = req.query.userId;
  const userTier = getUserTier(userId); // VIP, Premium, Free
  
  let config;
  if (userTier === 'VIP') {
    config = getHighEndConfig();
  } else if (userTier === 'Premium') {
    config = getMidConfig();
  } else {
    config = getBasicConfig();
  }
  
  res.json(config);
});
```

---

## 🏆 Conclusion

Remote Config = **Production Superpower** 🚀

- No rebuild required
- Instant updates
- A/B testing ready
- Region-specific content
- Device-specific optimization

**Your game, always optimized!**
#endregion