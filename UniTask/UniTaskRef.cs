yield return null;await UniTask.Yield();Bir sonraki kareye bekle.
yield return new WaitForSeconds(3f);await UniTask.Delay(TimeSpan.FromSeconds(3f));Zaman tabanlı bekleme.
yield return new WaitForEndOfFrame();await UniTask.WaitForEndOfFrame();Kare sonunu bekle.
yield return StartCoroutine(MyCoroutine());await MyAsyncMethod();Başka bir asenkron metodu bekle.

// Cancellition Token in MonoBehaviour
await UniTask.Delay(..., cancellationToken: destroyCancellationToken);

UniTask.WhenAll(task1, task2, ...)	Tüm görevlerin bitmesini bekle. Performans artışı için paralel çalışır.
UniTask.WhenAny(task1, task2, ...)	Görevlerden herhangi birinin bitmesini bekle.
UniTask.WaitUntil(koşul)	Koşul sağlanana kadar bekle (yield return new WaitUntil(...) karşılığı).

// Üç farklı asenkron görevin aynı anda başlamasını ve hepsinin bitmesini bekle
await UniTask.WhenAll(
        LoadPlayerDataAsync(),
        DownloadAssetsAsync(),
        StartIntroAnimationAsync()