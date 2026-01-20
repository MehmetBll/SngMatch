using System.Diagnostics;
using UnityEngine;

public class magnetObject : MonoBehaviour
{
private Rigidbody rb;
private Transform targetCatcher;
private bool isMagnetized = false;
public float magnetSpeed = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
    }

         void Update()
         {
            if(!isMagnetized) 
                return;
                
                transform.position = Vector3.Lerp(
               transform.position,
               targetCatcher.position,
               Time.deltaTime * magnetSpeed);
         }
         public void magnetize(Transform center)
    {
        targetCatcher = center;
        isMagnetized = true;
        rb.isKinematic = true; 
        rb.angularVelocity = Vector3.zero;
        //fiziksel olan hareketleri kapatır
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;
        
    }

    
}
