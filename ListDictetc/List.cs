/*
===========================================================
  C# List Referans Dosyası (Unity için Optimize Rehberi)
  Hazırlayan: ChatGPT
  Amaç: List<T> yapısını hızlı, doğru ve optimize şekilde kullanmak
===========================================================
*/

using System.Collections.Generic;
using UnityEngine;

public class ListReference : MonoBehaviour
{
    // 🔹 List<T> Tanımı
    private List<string> myList = new List<string>();

    void Start()
    {
        /*---------------------------------------------------
         * 1️⃣ Add(Value)
         * - Listeye yeni bir öğe ekler.
         * - Sonuna ekler, dinamik olarak büyür.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) Ortalama, ancak kapasite dolarsa O(n) (yeniden boyutlandırma).
         * **Uzay Karmaşıklığı**: O(n) - Öğeler depolanır, ancak zamanla dinamik olarak büyür.
         * ---------------------------------------------------*/
        myList.Add("Player");
        myList.Add("Enemy");

        /*---------------------------------------------------
         * 2️⃣ Insert(index, value)
         * - Belirli bir indekse öğe ekler.
         * - Diğer öğeleri sağa kaydırır.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Listeyi kaydırmak gerekebilir.
         * **Uzay Karmaşıklığı**: O(n) - Öğeler kaydırılacağı için ekstra bellek kullanılmaz.
         * ---------------------------------------------------*/
        myList.Insert(1, "NPC");

        /*---------------------------------------------------
         * 3️⃣ Contains(Value)
         * - Listenin içinde değerin olup olmadığını kontrol eder.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Tüm öğeleri sırayla kontrol eder.
         * **Uzay Karmaşıklığı**: O(1) - Sadece kontrol eder, ekstra bellek kullanmaz.
         * ---------------------------------------------------*/
        if (myList.Contains("Player"))
        {
            Debug.Log("Player var!");
        }

        /*---------------------------------------------------
         * 4️⃣ Remove(Value)
         * - Listeye eklenmiş ilk karşılaşılan öğeyi siler.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Öğeyi bulmak ve ardından öğeleri kaydırmak gerekir.
         * **Uzay Karmaşıklığı**: O(1) - Ekstra bellek harcamaz.
         * ---------------------------------------------------*/
        myList.Remove("Enemy");

        /*---------------------------------------------------
         * 5️⃣ RemoveAt(index)
         * - Belirli bir indeksteki öğeyi siler.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Öğeyi bulup silmek ve kalan öğeleri kaydırmak gerekir.
         * **Uzay Karmaşıklığı**: O(1) - Ekstra bellek kullanımı yoktur.
         * ---------------------------------------------------*/
        myList.RemoveAt(0);

        /*---------------------------------------------------
         * 6️⃣ Clear()
         * - Listede tüm öğeleri temizler.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Tüm öğeleri siler.
         * **Uzay Karmaşıklığı**: O(1) - Listeyi sıfırlar, ancak bellek serbest bırakılmaz.
         * ---------------------------------------------------*/
        myList.Clear();

        /*---------------------------------------------------
         * 7️⃣ Count
         * - Listede kaç öğe olduğunu döner.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Öğeler sayılırken ekstra işlem yapılmaz.
         * **Uzay Karmaşıklığı**: O(1) - Sadece sayıyı döner.
         * ---------------------------------------------------*/
        Debug.Log("Toplam Eleman Sayısı: " + myList.Count);

        /*---------------------------------------------------
         * 8️⃣ Foreach ile Tüm Elemanları Gezinme
         * - Listeyi sırasıyla gezip işlem yapar.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Tüm öğeleri dolaşmak gerekir.
         * **Uzay Karmaşıklığı**: O(1) - İlave bellek harcamaz.
         * ---------------------------------------------------*/
        foreach (string item in myList)
        {
            Debug.Log("Eleman: " + item);
        }

        /*---------------------------------------------------
         * 9️⃣ Indexer [index]
         * - Belirli bir indeksle öğeye erişir.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(1) - Direkt indeksle erişim.
         * **Uzay Karmaşıklığı**: O(1) - Değer doğrudan döner.
         * ---------------------------------------------------*/
        string player = myList[0];

        /*---------------------------------------------------
         * 🔟 ToArray()
         * - Listeyi diziye dönüştürür.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Listeyi yeni bir diziyi kopyalar.
         * **Uzay Karmaşıklığı**: O(n) - Yeni bir dizi oluşturur.
         * ---------------------------------------------------*/
        string[] playerArray = myList.ToArray();

        /*---------------------------------------------------
         * 1️⃣1️⃣ Sort()
         * - Listeyi sıralar (varsayılan sıralama ile).
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n log n) - Sıralama algoritması (Merge Sort veya Quick Sort).
         * **Uzay Karmaşıklığı**: O(n) - Sıralama işlemi sırasında geçici bellek kullanılır.
         * ---------------------------------------------------*/
        myList.Sort();

        /*---------------------------------------------------
         * 1️⃣2️⃣ Reverse()
         * - Listeyi tersine çevirir.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Listeyi tersine çevirirken tüm öğeleri tarar.
         * **Uzay Karmaşıklığı**: O(1) - Listeyi yerinde ters çevirir, ekstra bellek kullanmaz.
         * ---------------------------------------------------*/
        myList.Reverse();

        /*---------------------------------------------------
         * 1️⃣3️⃣ Exists(Predicate)
         * - Koşula uyan öğe var mı kontrol eder.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Koşula uyan öğe bulunur.
         * **Uzay Karmaşıklığı**: O(1) - Boolean değer döner, ekstra bellek harcamaz.
         * ---------------------------------------------------*/
        bool exists = myList.Exists(x => x == "Player");

        /*---------------------------------------------------
         * 1️⃣4️⃣ Find(Predicate)
         * - Koşula uyan ilk öğeyi döner.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Koşula uyan ilk öğe bulunur.
         * **Uzay Karmaşıklığı**: O(1) - İlk öğeyi döner.
         * ---------------------------------------------------*/
        string foundItem = myList.Find(x => x == "Player");

        /*---------------------------------------------------
         * 1️⃣5️⃣ FindIndex(Predicate)
         * - Koşula uyan ilk öğenin indeksini döner.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Koşula uyan öğe bulunur.
         * **Uzay Karmaşıklığı**: O(1) - İndeks değerini döner.
         * ---------------------------------------------------*/
        int foundIndex = myList.FindIndex(x => x == "Player");

        /*---------------------------------------------------
         * 1️⃣6️⃣ AddRange(IEnumerable)
         * - Bir koleksiyonu listeye ekler.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(k) - k, eklenen öğe sayısı.
         * **Uzay Karmaşıklığı**: O(k) - Eklenen öğeler kadar bellek harcanır.
         * ---------------------------------------------------*/
        myList.AddRange(new List<string> { "Boss", "Ally" });

        /*---------------------------------------------------
         * 1️⃣7️⃣ RemoveAll(Predicate)
         * - Koşula uyan tüm öğeleri siler.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Öğeleri bulup siler.
         * **Uzay Karmaşıklığı**: O(1) - Ekstra bellek harcamaz.
         * ---------------------------------------------------*/
        myList.RemoveAll(x => x == "Ally");

        /*---------------------------------------------------
         * 1️⃣8️⃣ RemoveRange(int index, int count)
         * - Belirtilen indeks aralığındaki öğeleri siler.
         * ---------------------------------------------------
         * **Zaman Karmaşıklığı**: O(n) - Listeyi kaydırarak öğeleri siler.
         * **Uzay Karmaşıklığı**: O(1) - Silme işlemi bellek harcamaz.
         * ---------------------------------------------------*/
        myList.RemoveRange(0, 2);
    }
}
