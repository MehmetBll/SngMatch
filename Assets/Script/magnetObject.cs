using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class magnetObject : MonoBehaviour
{
private Rigidbody rb;
private Transform targetCatcher;
bool isMagnetized;
public float magnetSpeed = 5f;


         [System.Obsolete]
         void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>(); 
        
    }
         void FixedUpdate()
    {
        if (isMagnetized && targetCatcher != null)
            return;
        Vector3 dir = targetCatcher.position - transform.position;
        rb.MovePosition(transform.position + dir.normalized * magnetSpeed * Time.fixedDeltaTime);
    }
    public void magnetize(Transform center)
    {
        targetCatcher = center;
        isMagnetized = true;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void demagnetize()
    {
        isMagnetized = false;
        targetCatcher = null;
        rb.useGravity = true;
    }
}
