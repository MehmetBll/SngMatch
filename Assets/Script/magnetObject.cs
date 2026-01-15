using System.Diagnostics;
using UnityEngine;

public class magnetObject : MonoBehaviour
{
private Rigidbody rb;
private Transform targetCatcher;
private float pullSpeed;
private bool isPulled = false;

    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
    }
    //objeyi çekmeye başlar
    [System.Obsolete]
    public void StartMagnet(Transform catcherTransform, float speed)
    {
        targetCatcher = catcherTransform;
        pullSpeed = speed;
        isPulled = true;
        rb.useGravity = false;
        rb.drag = 5f;
        isPulled = true;
    }
    //objeyi eski haline getirir
    [System.Obsolete]
    public void StopMagnet()
    {
        targetCatcher = null;
        rb.useGravity = true;
        rb.drag = 0f;
        isPulled = false;
    }
    //objeyi çekme işlemi
    void FixedUpdate()
    {
        if (!isPulled || targetCatcher == null)
            return;

        Vector3 dir = targetCatcher.position - rb.position;
        rb.linearVelocity = dir * pullSpeed;
    }
}
