// ============================================================
// 🌀 DOTween - TÜM KOMUTLAR + AÇIKLAMALAR
// ============================================================

// ====================
// DOTWEEN TEMEL METODLAR
// ====================
DOTween.Init();                           /* DOTween sistemini başlatır (bir kez çağırmak yeterli). */
DOTween.Clear();                          /* Tüm aktif tweenleri ve ayarları sıfırlar. */
DOTween.ClearCachedTweens();              /* Bellekteki tween cache’lerini temizler. */
DOTween.KillAll();                        /* Tüm tweenleri anında siler. */
DOTween.PauseAll();                       /* Tüm tweenleri duraklatır. */
DOTween.PlayAll();                        /* Tüm tweenleri devam ettirir. */
DOTween.RestartAll();                     /* Tüm tweenleri en baştan oynatır. */
DOTween.CompleteAll();                    /* Tüm tweenleri bitiş değerine atlar. */
DOTween.FlipAll();                        /* Yönünü tersine çevirir (ileri ↔ geri). */
DOTween.RewindAll();                      /* Tüm tweenleri başa sarar. */
DOTween.TogglePauseAll();                 /* Pause ↔ Play arasında geçiş yapar. */

DOTween.To(getter, setter, endValue, duration); /* Manuel değer tween’i (herhangi bir değişkeni animasyonla değiştirir). */
DOTween.ToAlpha(getter, setter, endValue, duration); /* Alfa (şeffaflık) tween’i yapar. */
DOTween.Sequence();                       /* Boş bir tween zinciri (Sequence) oluşturur. */

DOTween.SetTweensCapacity(200, 50);       /* Bellek optimizasyonu için kapasite belirler. */
DOTween.IsTweening(target);               /* Belirtilen nesne şu anda tween’leniyor mu kontrol eder. */
DOTween.Kill(target);                     /* Sadece belirli hedefin tweenlerini öldürür. */
DOTween.Complete(target);                 /* Belirli tweenleri bitirir. */
DOTween.Pause(target);                    /* Belirli tweenleri duraklatır. */
DOTween.Play(target);                     /* Belirli tweenleri devam ettirir. */
DOTween.Restart(target);                  /* Belirli tweenleri baştan oynatır. */
DOTween.Rewind(target);                   /* Belirli tweenleri başa sarar. */


// ====================
// SEQUENCE KOMUTLARI
// ====================
Sequence seq = DOTween.Sequence();        /* Yeni bir tween sırası oluşturur. */
seq.Append(Tween);                        /* Tween’i sıraya ekler (ardışık çalışır). */
seq.Prepend(Tween);                       /* Tween’i sıranın başına ekler. */
seq.Join(Tween);                          /* Tween’i mevcut aşamayla eş zamanlı çalıştırır. */
seq.Insert(1f, Tween);                    /* Belirli zamanda tween ekler. */
seq.AppendInterval(1f);                   /* Araya bekleme süresi ekler. */
seq.PrependInterval(1f);                  /* Başına bekleme süresi ekler. */
seq.InsertCallback(1f, () => {});         /* Belirli sürede fonksiyon çağırır. */
seq.AppendCallback(() => {});             /* Tween bitince fonksiyon çağırır. */

seq.OnStart(() => {});                    /* Sequence başlarken çağrılır. */
seq.OnPlay(() => {});                     /* Oynatıldığında çağrılır. */
seq.OnUpdate(() => {});                   /* Her frame güncellenir. */
seq.OnComplete(() => {});                 /* Tamamlandığında çağrılır. */
seq.OnKill(() => {});                     /* Yok edildiğinde çağrılır. */

seq.SetDelay(1f);                         /* Başlamadan önce bekleme süresi. */
seq.SetLoops(2, LoopType.Yoyo);           /* Kaç defa döneceği ve loop tipi. */
seq.SetEase(Ease.OutBounce);              /* Hız eğrisi. */
seq.SetAutoKill(true);                    /* Bittikten sonra otomatik silinsin mi. */
seq.SetUpdate(true);                      /* TimeScale’den bağımsız mı oynasın. */
seq.Play();                               /* Başlatır. */
seq.Pause();                              /* Durdurur. */
seq.Kill();                               /* Yok eder. */
seq.Restart();                            /* Baştan oynatır. */
seq.Complete();                           /* Son değere atlar. */
seq.Rewind();                             /* Başa döner. */


// ====================
// TWEENER ORTAK AYARLAR
// ====================
t.SetDelay(1f);                           /* Tween başlamadan önce bekleme süresi. */
t.SetLoops(3, LoopType.Yoyo);             /* Kaç defa döneceği ve tipi. */
t.SetEase(Ease.InOutSine);                /* Hız eğrisi. */
t.SetId("moveX");                         /* Tween’e özel ID atar. */
t.SetAutoKill(true);                      /* Bittikten sonra otomatik silinsin mi. */
t.SetUpdate(UpdateType.Normal, true);     /* Update tipi (Normal, Late, Fixed). */
t.OnStart(() => {});                      /* Tween başladığında çağrılır. */
t.OnPlay(() => {});                       /* Oynatıldığında çağrılır. */
t.OnUpdate(() => {});                     /* Her kare güncellenir. */
t.OnComplete(() => {});                   /* Bittiğinde çağrılır. */
t.OnKill(() => {});                       /* Yok edildiğinde çağrılır. */
t.Play();                                 /* Tween’i oynatır. */
t.Pause();                                /* Tween’i durdurur. */
t.Kill();                                 /* Tween’i yok eder. */
t.Rewind();                               /* Tween’i başa döndürür. */
t.Complete();                             /* Anında bitiş değerine gider. */


// ====================
// TRANSFORM ANİMASYONLARI
// ====================
transform.DOMove(Vector3(0,5,0), 1f);     /* Objeyi belirtilen konuma taşır. */
transform.DOMoveX(3f, 1f);                /* X ekseninde hareket. */
transform.DOMoveY(2f, 1f);                /* Y ekseninde hareket. */
transform.DOMoveZ(1f, 1f);                /* Z ekseninde hareket. */
transform.DOLocalMove(Vector3.zero, 1f);  /* Yerel pozisyona göre hareket. */
transform.DOLocalRotate(Vector3.forward * 45, 1f);  /* Yerel rotasyon. */
transform.DORotate(Vector3.up * 90, 1f);  /* Objeyi döndürür. */
transform.DOScale(2f, 1f);                /* Ölçeği değiştirir. */
transform.DOPunchPosition(Vector3.up, 0.5f, 10, 1f); /* Kısa sarsılma hareketi. */
transform.DOShakePosition(1f, 1f);        /* Rastgele sarsar. */
transform.DOJump(Vector3.up * 3, 2f, 1, 2f); /* Zıplama hareketi. */
transform.DOPath(pathPoints, 3f, PathType.CatmullRom); /* Belirli yoldan hareket. */
transform.DOPath(new Vector3[] {
            Vector3.zero, Vector3.up * 3, Vector3.right * 3
        }, 3f, PathType.CatmullRom);                        /* Yol boyunca hareket. */


// ====================
// UI ANİMASYONLARI
// ====================
CanvasGroup canvasGroup = null;
Image image = null;
Text text = null;
RectTransform rect = null;


canvasGroup.DOFade(0f, 1f);               /* CanvasGroup’un alfa (şeffaflığını) değiştirir. */
image.DOFade(0.5f, 1f);                   /* Image alfa geçişi. */
image.DOColor(Color.red, 1f);              /* Image rengi değişir. */
text.DOFade(0f, 1f);                      /* Text şeffaflığı değişir. */
text.DOColor(Color.green, 1f);             /* Text rengi değişir. */
text.DOText("Merhaba", 2f, true);          /* Yazıyı yavaş yavaş değiştirir (typewriter efekti). */
rectTransform.DOAnchorPos(Vector2.zero, 1f); /* UI öğesinin konumunu değiştirir. */
rectTransform.DOSizeDelta(new Vector2(200, 100), 1f); /* Boyutu değiştirir. */
rectTransform.DOPivot(new Vector2(0.5f, 1f), 1f); /* Pivot noktasını değiştirir. */


// ====================
// MATERIAL / SPRITE
// ====================
Material mat = null;
SpriteRenderer sprite = null;

material.DOColor(Color.blue, 1f);          /* Materyalin rengini değiştirir. */
material.DOFade(0.3f, 1f);                /* Materyalin saydamlığını değiştirir. */
spriteRenderer.DOColor(Color.yellow, 1f);  /* Sprite rengini değiştirir. */
spriteRenderer.DOFade(0f, 1f);             /* Sprite saydamlığını değiştirir. */


// ====================
// CAMERA
// ====================
Camera cam = null;

camera.DOFieldOfView(60f, 1f);             /* FOV (zoom) değişimi. */
camera.DOAspect(1.77f, 1f);                /* Aspect ratio geçişi. */
camera.DOOrthoSize(5f, 1f);                /* Ortho kamera boyutu değişimi. */
camera.DORect(new Rect(0,0,1,1), 1f);      /* Kamera viewport dikdörtgeni. */


// ====================
// AUDIO
// ====================
AudioSource audio = null;
   
audioSource.DOFade(0f, 1f);                /* Ses seviyesini düşürür (fade out). */
audioSource.DOPitch(2f, 1f);               /* Sesin pitch’ini değiştirir. */
audioSource.DOStereoPan(1f, 1f);           /* Sesin stereo yönünü değiştirir. */


// ====================
// LIGHT
// ====================
Light light = null;

light.DOColor(Color.white, 1f);            /* Işık rengini değiştirir. */
light.DOIntensity(5f, 1f);                 /* Işık yoğunluğunu değiştirir. */
light.DORange(10f, 1f);                    /* Işığın menzilini değiştirir. */


// ====================
// RIGIDBODY / 2D
// ====================
Rigidbody rb = null;
Rigidbody2D rb2D = null;

rigidbody.DOMove(Vector3(5,0,0), 1f);      /* Fizik nesnesini hareket ettirir. */
rigidbody.DORotate(Vector3(0,180,0), 1f);  /* Fizik nesnesini döndürür. */
rigidbody2D.DOMove(Vector2(3,1), 1f);      /* 2D fizik nesnesi hareket ettirir. */
rigidbody2D.DORotate(90f, 1f);             /* 2D nesneyi döndürür. */
rigidbody2D.DOJump(Vector2.up * 3, 2f, 1, 2f); /* 2D zıplama animasyonu. */


// ====================
// DİĞER KULLANIMLAR
// ====================
DOTween.Kill(target);                      /* Belirli tween(ler)i yok eder. */
DOTween.Complete(target);                  /* Hedef tween’i hemen bitirir. */
DOTween.PlayBackwards(target);             /* Geri oynatır. */
DOTween.PlayForward(target);               /* İleri oynatır. */
DOTween.TogglePause(target);               /* Pause ↔ Play arasında geçiş. */
