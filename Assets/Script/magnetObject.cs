using System.Diagnostics;
using System.Numerics;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class magnetObject : MonoBehaviour
{   
private Rigidbody rb;
private Transform targetCatcher;
private bool isMagnetized;
public float magnetSpeed = 5f;

         void Awake()
         {
            rb = GetComponentInChildren<Rigidbody>();
         }
    void Update()
    {
        if (!isMagnetized || targetCatcher == null)
            return;
        transform.position = Vector3.Lerp(transform.position, targetCatcher.position, Time.deltaTime * magnetSpeed);
    }

         public void magnetize(Transform targetCatcher)
    {
         isMagnetized = false;
         targetCatcher = null;
         rb.isKinematic = false;
         rb.useGravity = true;
         transform.position = targetCatcher.position; 

    }

    public void demagnetize()
    {
       isMagnetized = false;
       targetCatcher = null;
       rb.isKinematic = false;
       rb.useGravity = true;
    }
}
