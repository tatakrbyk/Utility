/*
===========================================================
  C# Dictionary Referans Dosyası (Unity için Optimize Rehberi)
  Hazırlayan: ChatGPT
  Amaç: Dictionary yapısını hızlı, doğru ve optimize şekilde kullanmak
===========================================================
*/

using System.Collections.Generic;
using UnityEngine;

public class DictionaryReference : MonoBehaviour
{
    // 🔹 Generic Dictionary tanımı
    private Dictionary<int, string> myDict = new Dictionary<int, string>();

    void Start()
    {
        /*---------------------------------------------------
         * 1️⃣ Add(Key, Value)
         * - Yeni bir anahtar-değer çifti ekler.
         * - Eğer aynı anahtar zaten varsa HATA fırlatır (KeyAlreadyExistsException).
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Ortalama
         * **Uzay Karmaşıklığı**: O(n) - Öğeler depolanır
         * ---------------------------------------------------*/
        myDict.Add(1, "Player");
        myDict.Add(2, "Enemy");

        /*---------------------------------------------------
         * 2️⃣ ContainsKey(Key)
         * - Anahtarın sözlükte olup olmadığını kontrol eder.
         * - Add() öncesi mutlaka kontrol et! (Performanslıdır: O(1))
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Ortalama
         * **Uzay Karmaşıklığı**: O(1) - Sadece kontrol eder, ekstra bellek kullanmaz.
         * ---------------------------------------------------*/
        if (!myDict.ContainsKey(3))
        {
            myDict.Add(3, "NPC");
        }

        /*---------------------------------------------------
         * 3️⃣ ContainsValue(Value)
         * - Değerin sözlükte olup olmadığını kontrol eder.
         * - DİKKAT: Performans olarak pahalıdır! O(n)
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Değerleri sırayla kontrol eder
         * **Uzay Karmaşıklığı**: O(1) - Değerler saklanmaz, sadece kontrol edilir.
         * ---------------------------------------------------*/
        if (myDict.ContainsValue("Player"))
        {
            Debug.Log("Player var!");
        }

        /*---------------------------------------------------
         * 4️⃣ TryGetValue(Key, out Value)
         * - Anahtarı güvenli şekilde arar, hata fırlatmaz.
         * - Aradığın anahtar yoksa false döner.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Ortalama
         * **Uzay Karmaşıklığı**: O(1) - Sadece değer döner.
         * ---------------------------------------------------*/
        if (myDict.TryGetValue(2, out string result))
        {
            Debug.Log("Bulundu: " + result);
        }

        /*---------------------------------------------------
         * 5️⃣ Remove(Key)
         * - Anahtara göre bir öğeyi siler.
         * - Dönüş değeri true/false.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Ortalama
         * **Uzay Karmaşıklığı**: O(1) - Anahtar ve değeri siler, ancak depolama alanı değişmez.
         * ---------------------------------------------------*/
        bool removed = myDict.Remove(3);

        /*---------------------------------------------------
         * 6️⃣ Clear()
         * - Tüm sözlüğü temizler (Belleği serbest bırakmaz).
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Tüm öğeleri siler.
         * **Uzay Karmaşıklığı**: O(1) - Ancak bellek serbest bırakılmaz.
         * ---------------------------------------------------*/
        myDict.Clear();

        /*---------------------------------------------------
         * 7️⃣ Count
         * - Kaç öğe olduğunu döner (O(1))
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Eleman sayısı saklanır, hızlı erişim.
         * **Uzay Karmaşıklığı**: O(1) - Sadece sayıyı döner, ekstra bellek kullanmaz.
         * ---------------------------------------------------*/
        Debug.Log("Toplam Eleman Sayısı: " + myDict.Count);

        /*---------------------------------------------------
         * 8️⃣ Foreach ile Tüm Elemanları Gezinme
         * - Performanslı ve güvenlidir.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Tüm öğeleri dolaşır.
         * **Uzay Karmaşıklığı**: O(1) - İlave bellek harcamaz.
         * ---------------------------------------------------*/
        foreach (KeyValuePair<int, string> pair in myDict)
        {
            Debug.Log($"Key: {pair.Key}, Value: {pair.Value}");
        }

        /*---------------------------------------------------
         * 9️⃣ Indexer [key]
         * - Anahtara göre direkt erişim sağlar.
         * - Eğer anahtar yoksa HATA fırlatır! (Dikkatli ol)
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Ortalama.
         * **Uzay Karmaşıklığı**: O(1) - Değer doğrudan döner.
         * ---------------------------------------------------*/
        myDict[1] = "Hero"; // Var olanı değiştirir
        // string name = myDict[999]; // ❌ Hata verir!

        /*---------------------------------------------------
         * 🔟 Keys & Values Koleksiyonları
         * - Sadece anahtar veya değer listesini almak için kullanılır.
         * - Performanslı, ama dikkat et: sadece okuma yapılmalı.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Her iki koleksiyon da sözlüğün tamamını döner.
         * **Uzay Karmaşıklığı**: O(n) - Her iki koleksiyon da tüm öğeleri içerir.
         * ---------------------------------------------------*/
        ICollection<int> allKeys = myDict.Keys;
        ICollection<string> allValues = myDict.Values;
    }

    /*---------------------------------------------------
     * 🧠 Performans & Optimize Önerileri
     * ---------------------------------------------------
     * ✅ Dictionary başlangıçta kapasite belirle (büyüme maliyetini düşürür)
     *    var dict = new Dictionary<int, string>(initialCapacity);
     *
     * ✅ ValueType (struct) kullanırken kutulama (boxing) yapma!
     *    -> Mümkünse Dictionary<int, float> gibi primitive türlerle çalış.
     *
     * ✅ ContainsKey + Add pattern'i yerine TryAdd kullan (C# 9+)
     *    dict.TryAdd(key, value);
     *
     * ✅ Unity Update() içinde Dictionary’ye erişim pahalı olabilir.
     *    -> Cache'le veya frame dışı işle.
     *
     * ✅ Thread güvenliği gerekirse ConcurrentDictionary kullan.
     *
     * ✅ Lookup hızını artırmak için key olarak string yerine int/enum tercih et.
     *
     * ✅ JSON serileştirmede Dictionary türleri dikkat ister (özellikle string key dışındakiler).
     *    -> int key’leri string’e dönüştürerek kaydet.
     * ---------------------------------------------------*/
}
