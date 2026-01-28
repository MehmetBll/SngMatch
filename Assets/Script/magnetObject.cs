using System.ComponentModel.Design.Serialization;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class magnetObject : MonoBehaviour
{   
private Rigidbody rb;
private Transform targetCatcher;
[SerializeField]private Transform childObject;
private bool isMagnetized;
public float magnetSpeed = 5f;


         void Awake()
         {
            rb = GetComponentInChildren<Rigidbody>();
            
         }
         void FixedUpdate()
         {
            //targerc null değilse objeyi ortaya çek
            if(!isMagnetized|| targetCatcher == null) return;

          childObject.localPosition = Vector3.Lerp(childObject.localPosition, Vector3.zero, Time.fixedDeltaTime * magnetSpeed);
         }

         public void magnetize(Transform center)
    {
         isMagnetized = true;
         targetCatcher = center;

         rb.isKinematic = true;
         rb.useGravity = false;
    }

    public void demagnetize()
    {
       isMagnetized = false;
       targetCatcher = null;

       rb.isKinematic = false;
       rb.useGravity = true;
    }
}
