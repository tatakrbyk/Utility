/*
===========================================================
  Unity Hashtable - Optimize Rehberi
  Hazırlayan: ChatGPT
  Amaç: Unity'de Hashtable kullanarak verimli işlemler yapmak
===========================================================
*/

using System.Collections;
using UnityEngine;

public class HashtableReference : MonoBehaviour
{
    // 🔹 Hashtable örneği
    private Hashtable playerData = new Hashtable()
    {
        { "PlayerName", "Hero" },
        { "PlayerScore", 5000 },
        { "PlayerLevel", 10 }
    };

    void Start()
    {
        /*---------------------------------------------------
         * 1️⃣ Add() - Yeni bir anahtar-değer çifti ekler
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Anahtar ekleme işlemi hızlıdır.
         * **Uzay Karmaşıklığı**: O(1) - Yeni bir anahtar-değer çifti için bellek tahsis eder.
         * ---------------------------------------------------*/
        playerData.Add("PlayerHealth", 100);

        /*---------------------------------------------------
         * 2️⃣ Contains() - Anahtarın mevcut olup olmadığını kontrol eder
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Anahtarın olup olmadığını hızlıca kontrol eder.
         * **Uzay Karmaşıklığı**: O(1) - Boolean değer döner, ek bellek harcamaz.
         * ---------------------------------------------------*/
        bool hasKey = playerData.Contains("PlayerName");
        Debug.Log("Contains PlayerName: " + hasKey);

        /*---------------------------------------------------
         * 3️⃣ Remove() - Belirli bir anahtarı siler
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Anahtar hızlıca silinir.
         * **Uzay Karmaşıklığı**: O(1) - Hafızadan silme işlemi hızlıdır.
         * ---------------------------------------------------*/
        playerData.Remove("PlayerHealth");

        /*---------------------------------------------------
         * 4️⃣ Clear() - Tüm anahtarları ve değerleri siler
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Tüm öğeleri siler.
         * **Uzay Karmaşıklığı**: O(1) - Hafızayı temizler.
         * ---------------------------------------------------*/
        playerData.Clear();

        /*---------------------------------------------------
         * 5️⃣ ContainsKey() - Anahtarın var olup olmadığını kontrol eder
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Anahtarın olup olmadığını kontrol eder.
         * **Uzay Karmaşıklığı**: O(1) - Boolean değer döner, ek bellek harcamaz.
         * ---------------------------------------------------*/
        bool containsKey = playerData.ContainsKey("PlayerName");
        Debug.Log("ContainsKey PlayerName: " + containsKey);

        /*---------------------------------------------------
         * 6️⃣ ContainsValue() - Değerin mevcut olup olmadığını kontrol eder
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Değerleri tek tek kontrol eder.
         * **Uzay Karmaşıklığı**: O(1) - Boolean değer döner, ek bellek harcamaz.
         * ---------------------------------------------------*/
        bool containsValue = playerData.ContainsValue(5000);
        Debug.Log("Contains Value 5000: " + containsValue);

        /*---------------------------------------------------
         * 7️⃣ Get() - Anahtara karşılık gelen değeri alır
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Anahtara hızlıca erişir.
         * **Uzay Karmaşıklığı**: O(1) - Yalnızca bir değeri döner.
         * ---------------------------------------------------*/
        object playerName = playerData["PlayerName"];
        Debug.Log("Player Name: " + playerName);

        /*---------------------------------------------------
         * 8️⃣ Keys - Anahtarları döner
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Tüm anahtarları döner.
         * **Uzay Karmaşıklığı**: O(n) - Yeni bir koleksiyon oluşturur.
         * ---------------------------------------------------*/
        ICollection keys = playerData.Keys;
        foreach (var key in keys)
        {
            Debug.Log("Key: " + key);
        }

        /*---------------------------------------------------
         * 9️⃣ Values - Değerleri döner
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Tüm değerleri döner.
         * **Uzay Karmaşıklığı**: O(n) - Yeni bir koleksiyon oluşturur.
         * ---------------------------------------------------*/
        ICollection values = playerData.Values;
        foreach (var value in values)
        {
            Debug.Log("Value: " + value);
        }

        /*---------------------------------------------------
         * 🔟 Count - Hashtable'ın öğe sayısını döner
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Öğeler sayısı hızlıca döner.
         * **Uzay Karmaşıklığı**: O(1) - Ekstra bellek harcamaz.
         * ---------------------------------------------------*/
        Debug.Log("Hashtable öğe sayısı: " + playerData.Count);

        /*---------------------------------------------------
         * 1️⃣1️⃣ Item[] - Anahtar ile değeri doğrudan erişme
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Direkt anahtar ile erişim sağlar.
         * **Uzay Karmaşıklığı**: O(1) - Yalnızca bir öğe döner.
         * ---------------------------------------------------*/
        playerData["PlayerScore"] = 6000;
        Debug.Log("Updated PlayerScore: " + playerData["PlayerScore"]);
    }
}
