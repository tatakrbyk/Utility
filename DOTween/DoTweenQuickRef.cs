/*
=========================================================
 🌀 DOTWEEN QUICK REFERENCE (by ChatGPT)
---------------------------------------------------------
 Kullanım amacı:
 - Tüm DoTween fonksiyonlarını tek dosyada görmek
 - IntelliSense (otomatik tamamlama) desteği sağlamak
 - Hızlı rehber olarak kullanmak (çalıştırılmaz)
=========================================================
*/

using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public static class DoTweenQuickRef
{
    public static void Examples()
    {
        // =========================================================
        // 🎯 DOTWEEN GENEL METODLAR
        // =========================================================
        DOTween.Init();                           /* DOTween sistemini başlatır. */
        DOTween.Clear();                          /* Tüm tweenleri sıfırlar. */
        DOTween.KillAll();                        /* Tüm tweenleri yok eder. */
        DOTween.PauseAll();                       /* Hepsini duraklatır. */
        DOTween.PlayAll();                        /* Tümünü oynatır. */
        DOTween.RestartAll();                     /* Baştan başlatır. */
        DOTween.CompleteAll();                    /* Anında bitirir. */
        DOTween.FlipAll();                        /* Yönünü ters çevirir. */
        DOTween.RewindAll();                      /* Başa sarar. */
        DOTween.TogglePauseAll();                 /* Pause ↔ Play arası geçiş. */

        DOTween.To(() => 0f, x => { }, 10f, 2f);  /* Herhangi bir değeri tween’ler. */
        DOTween.Sequence();                       /* Yeni bir Sequence oluşturur. */

        // =========================================================
        // 🧩 SEQUENCE KULLANIMI
        // =========================================================
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.5f);                 /* 0.5 saniye bekler. */
        seq.Append(transform.DOMoveY(5, 1f));     /* Y ekseninde hareket ekler. */
        seq.Join(transform.DOScale(2, 1f));       /* Aynı anda ölçek değiştirir. */
        seq.Insert(0.3f, transform.DORotate(Vector3.up * 45, 1f)); /* Belirli zamanda ekler. */
        seq.OnComplete(() => Debug.Log("Sequence bitti!")); /* Bitince çağrılır. */

        // =========================================================
        // ⚙️ TWEENER AYARLARI
        // =========================================================
        Tweener t = transform.DOMoveX(5, 1f)
            .SetEase(Ease.OutBounce)              /* Hız eğrisi */
            .SetLoops(2, LoopType.Yoyo)           /* İki defa ileri-geri */
            .SetDelay(0.5f)                       /* Başlamadan önce bekleme */
            .OnStart(() => Debug.Log("Tween başladı"))
            .OnUpdate(() => Debug.Log("Tween ilerliyor"))
            .OnComplete(() => Debug.Log("Tween bitti"));

        t.Pause();                                /* Tween’i durdurur. */
        t.Play();                                 /* Devam ettirir. */
        t.Rewind();                               /* Başa döner. */
        t.Kill();                                 /* Yok eder. */

        // =========================================================
        // 🧍 TRANSFORM ANİMASYONLARI
        // =========================================================
        transform.DOMove(new Vector3(0, 5, 0), 1f);         /* Objeyi taşır. */
        transform.DOMoveX(3f, 1f);                          /* X ekseninde hareket. */
        transform.DOLocalMove(Vector3.zero, 1f);            /* Yerel pozisyona hareket. */
        transform.DORotate(Vector3.up * 90, 1f);            /* Döndürür. */
        transform.DOLocalRotate(Vector3.forward * 45, 1f);  /* Yerel rotasyon. */
        transform.DOScale(2f, 1f);                          /* Ölçeği değiştirir. */
        transform.DOPunchPosition(Vector3.up, 0.5f, 10, 1f); /* Sarsma efekti. */
        transform.DOShakePosition(1f, 1f);                  /* Rastgele sarsar. */
        transform.DOJump(Vector3.up * 3, 2f, 1, 2f);        /* Zıplama hareketi. */
        transform.DOPath(new Vector3[] {
            Vector3.zero, Vector3.up * 3, Vector3.right * 3
        }, 3f, PathType.CatmullRom);                        /* Yol boyunca hareket. */

        // =========================================================
        // 🧾 UI (Canvas, Text, Image)
        // =========================================================
        CanvasGroup canvasGroup = null;
        Image image = null;
        Text text = null;
        RectTransform rect = null;

        canvasGroup.DOFade(0f, 1f);              /* CanvasGroup saydamlığı. */
        image.DOFade(0.5f, 1f);                  /* Image şeffaflık geçişi. */
        image.DOColor(Color.red, 1f);            /* Image renk değişimi. */
        text.DOText("Merhaba Dünya", 2f, true);  /* Yazı yavaşça görünür. */
        text.DOFade(1f, 1f);                     /* Text alfa değişimi. */
        rect.DOAnchorPos(Vector2.zero, 1f);      /* UI konumu. */
        rect.DOSizeDelta(new Vector2(200, 100), 1f); /* Boyut değişimi. */
        rect.DOPivot(new Vector2(0.5f, 1f), 1f); /* Pivot ayarı. */

        // =========================================================
        // 🎨 MATERIAL / SPRITE
        // =========================================================
        Material mat = null;
        SpriteRenderer sprite = null;

        mat.DOColor(Color.green, 1f);            /* Materyal rengi. */
        mat.DOFade(0.5f, 1f);                   /* Materyal saydamlığı. */
        sprite.DOColor(Color.yellow, 1f);        /* Sprite rengi. */
        sprite.DOFade(0.3f, 1f);                 /* Sprite alfa geçişi. */

        // =========================================================
        // 🎥 CAMERA
        // =========================================================
        Camera cam = null;
        cam.DOFieldOfView(60f, 1f);              /* FOV değişimi. */
        cam.DOAspect(1.77f, 1f);                 /* Aspect ratio geçişi. */
        cam.DOOrthoSize(5f, 1f);                 /* Ortho boyut. */
        cam.DORect(new Rect(0, 0, 1, 1), 1f);    /* Viewport değişimi. */

        // =========================================================
        // 🔊 AUDIO
        // =========================================================
        AudioSource audio = null;
        audio.DOFade(0f, 1f);                    /* Ses seviyesini düşürür. */
        audio.DOPitch(2f, 1f);                   /* Pitch değiştirir. */
        audio.DOStereoPan(1f, 1f);               /* Stereo yön değişimi. */

        // =========================================================
        // 💡 LIGHT
        // =========================================================
        Light light = null;
        light.DOColor(Color.white, 1f);          /* Işık rengini değiştirir. */
        light.DOIntensity(5f, 1f);               /* Işık yoğunluğu. */
        light.DORange(10f, 1f);                  /* Işık menzili. */

        // =========================================================
        // ⚡ RIGIDBODY / 2D
        // =========================================================
        Rigidbody rb = null;
        Rigidbody2D rb2D = null;

        rb.DOMove(Vector3.one * 5, 1f);          /* Fizik nesnesini hareket ettirir. */
        rb.DORotate(Vector3.up * 180, 1f);       /* Fiziksel rotasyon. */
        rb2D.DOMove(Vector2.right * 2, 1f);      /* 2D hareket. */
        rb2D.DORotate(90f, 1f);                  /* 2D rotasyon. */
        rb2D.DOJump(Vector2.up * 3, 2f, 1, 2f);  /* 2D zıplama. */

        // =========================================================
        // 🔁 DİĞER GLOBAL KULLANIMLAR
        // =========================================================
        DOTween.Kill(transform);                 /* Hedefteki tweenleri yok eder. */
        DOTween.Complete(transform);             /* Bitirir. */
        DOTween.PlayBackwards(transform);        /* Geri oynatır. */
        DOTween.PlayForward(transform);          /* İleri oynatır. */
        DOTween.TogglePause(transform);          /* Pause ↔ Play. */
    }
}
