using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class magnetObject : MonoBehaviour
{
   private Rigidbody rb;
   private Transform targetCatcher;
   // root object (prefab root) and its original parent
   private Transform rootObject;
   private Transform originalRootParent;
   private bool isMagnetized;

   void Awake()
   {
      // operate on the prefab root so children follow correctly
      rootObject = transform.root;
      originalRootParent = rootObject.parent;
      rb = rootObject.GetComponentInChildren<Rigidbody>();
   }

   void FixedUpdate()
   {
      if (!isMagnetized || targetCatcher == null) return;

      // Ensure the root stays exactly at the catcher center
      if (rootObject != null)
         rootObject.localPosition = Vector3.zero;
   }

   public void magnetize(Transform center)
   {
      if (isMagnetized) return;
      isMagnetized = true;
      targetCatcher = center;

        // Debug: konumları yaz
        if (rootObject != null)
        {
           Debug.Log($"[magnetize] root='{rootObject.name}' rootPos={rootObject.position} centerPos={center.position} parentBefore={rootObject.parent}");
           rootObject.SetParent(center);
           rootObject.localPosition = Vector3.zero;
           rootObject.localRotation = Quaternion.identity;
           Debug.Log($"[magnetize] after parent rootPos={rootObject.position} localPos={rootObject.localPosition} parentNow={rootObject.parent}");
        }

      if (rb != null)
      {
         rb.linearVelocity = Vector3.zero;
         rb.angularVelocity = Vector3.zero;
         rb.isKinematic = true;
         rb.useGravity = false;
      }
   }

   public void demagnetize()
   {
      if (!isMagnetized) return;
      isMagnetized = false;
      targetCatcher = null;

      // restore original parent (could be null)
      if (rootObject != null)
         rootObject.SetParent(originalRootParent);

      if (rb != null)
      {
         rb.isKinematic = false;
         rb.useGravity = true;
      }
   }
}
