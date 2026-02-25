using UnityEngine;
using System.Collections;

public class magnetObject : MonoBehaviour
{
   // Bu sınıf: Bir nesnenin catcher merkezine doğru yumuşakça hareket etmesini sağlar
   // - `magnetize(Transform center)` çağrıldığında nesne merkez noktasına doğru gider ve parent edilir
   // - `demagnetize()` çağrıldığında orijinal parent geri yüklenir ve fizik yeniden etkinleşir
   [Tooltip("Objenin catchere doğru hareket etme hızı (birim/saniye)")]
   public float magnetSpeed = 12f;

   [Tooltip("Merkezde sabitlenme eşiği (dünya birimleri)")]
   public float snapDistance = 0.05f;

   // Rigidbody referansı (prefab kökündeki veya çocuklarından biri)
   private Rigidbody rb;
   // Prefab kökü (root) ve orijinal parent'ı
   private Transform rootObject;
   private Transform originalParent;
   // Aktif manyetize coroutine'i
   private Coroutine magnetRoutine;
   // Nesnenin şu anda manyetize olup olmadığını tutar
   private bool isMagnetized;
   // Tutulduğunda kullanılan geçici holder kaldırıldı; doğrudan prefab kökünü center'a parent ediyoruz

   void Awake()
   {
      rootObject = transform.root;
      originalParent = rootObject.parent;
      rb = rootObject.GetComponentInChildren<Rigidbody>();
   }
   public void magnetize(Transform center)
   {
      if (center == null) return;
      if (isMagnetized) return;
      if (magnetRoutine != null) StopCoroutine(magnetRoutine);
      magnetRoutine = StartCoroutine(MagnetizeRoutine(center));
   }

   IEnumerator MagnetizeRoutine(Transform center)
   {
      isMagnetized = true;

      // Sonraki adım için mevcut parent'ı kaydet (sonra geri döndürülecek)
      originalParent = rootObject.parent;

      // Fiziksel hareketi kapat, böylece transform'u doğrudan taşıyabiliriz
      if (rb != null)
      {
         rb.linearVelocity = Vector3.zero;
         rb.angularVelocity = Vector3.zero;
         rb.isKinematic = true;
         rb.useGravity = false;
         rb.detectCollisions = false;
      }

      // Geçici olarak parent'tan ayır; dünya uzayında merkeze doğru hareket edelim
      rootObject.SetParent(null, true);

      // Hedefe yumuşakça yaklaş
      while (center != null)
      {
         float dist = Vector3.Distance(rootObject.position, center.position);
         if (dist <= snapDistance) break;
         rootObject.position = Vector3.MoveTowards(rootObject.position, center.position, magnetSpeed * Time.deltaTime);
         rootObject.rotation = Quaternion.Slerp(rootObject.rotation, center.rotation, 12f * Time.deltaTime);
         yield return null;
      }

      if (center != null)
      {
         // Doğrudan center'a parent et ve tam olarak hizala
         rootObject.SetParent(center, true);
         rootObject.localPosition = Vector3.zero;
         rootObject.localRotation = Quaternion.identity;
      }

      // Catcher tarafından tutulurken fizik kapalı kalsın
      if (rb != null)
      {
         rb.isKinematic = true;
         rb.useGravity = false;
         rb.detectCollisions = false;
      }

      magnetRoutine = null;
   }
   public void demagnetize()
   {
      // Manyetizasyonu kaldırır: parent geri, fizik açık
      if (!isMagnetized) return;
      isMagnetized = false;
      if (magnetRoutine != null)
      {
         StopCoroutine(magnetRoutine);
         magnetRoutine = null;
      }

      // Orijinal parent'ı geri yükle
      if (rootObject != null)
         rootObject.SetParent(originalParent, true);

      // artık holder cleanup gerekli değil (hiçbir holder oluşturulmuyor)

      // Fiziği yeniden etkinleştir
      if (rb != null)
      {
         rb.isKinematic = false;
         rb.useGravity = true;
         rb.detectCollisions = true;
      }
   }
}
