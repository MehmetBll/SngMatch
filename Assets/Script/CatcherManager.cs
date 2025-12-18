using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AI;

public class CatcherManager : MonoBehaviour
{
    public bool isRight = false;

    public bool useTagComparison = false;
    public bool useRootObjectFromCollaider = true;
    public bool requireNonZeroMatchId = true;
    public float returnRandomRadius = 0.5f;  
    public float returnDuration = 1.0f;
    public float processDelay = 0.05f;
    public float throwUpForce = 5f;
    public Transform returnPoint; 
    private static CatcherManager CatcherL;
    private static CatcherManager CatcherR;

    private objectId heldObject;

    private void OnEnable()
    {
        RegisterInstance();
    }

    private void OnDisable()
    {
        UnregisterInstance();
    }

    private void RegisterInstance()
    {
        if (isRight)
            CatcherR = this;
        else
            CatcherL = this;
    }

    private void UnregisterInstance()
    {
        if (isRight)
        {
            if (CatcherR == this) CatcherR = null;
        }
        else
        {
            if (CatcherL == this) CatcherL = null;
        }
    }

    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        var go = useRootObjectFromCollaider ? other.transform.root.gameObject : other.gameObject;
        if (go == null) return;

        var oid = go.GetComponent<objectId>();
        if (oid == null) return;
        if (requireNonZeroMatchId && oid.matchId == 0) return;

        if (heldObject != null) return;

        heldObject = oid;
        TryProcessPairWithOtherCatcher();
    }
    private void OnTriggerExit(Collider other)
    {
        var go = useRootObjectFromCollaider ? other.transform.root.gameObject : other.gameObject;
        if (go == null) return;

        var oid = go.GetComponent<objectId>();
        if (oid == null) return;

        if (heldObject == oid)
            heldObject = null;
    }

    [System.Obsolete]
    private void TryProcessPairWithOtherCatcher()
    {
        CatcherManager other = isRight ? CatcherL : CatcherR;
        if (other == null) return;
        if (other.heldObject == null) return;

        var obj1 = this.heldObject;
        var obj2 = other.heldObject;

        if (obj1 == null || obj2 == null)
        {
            if (obj1 == null) this.heldObject = null;
            if (obj2 == null) other.heldObject = null;
            return;
        }

        int id1 = obj1.matchId;
        int id2 = obj2.matchId;

        if (id1 == id2)
        {
            Destroy(obj1.gameObject);
            Destroy(obj2.gameObject);
        }
        else
        {
            ThrowUp(obj1);
            ThrowUp(obj2);
        }

        this.heldObject = null;
        other.heldObject = null;
    }

    [System.Obsolete]
    private void ThrowUp(objectId oid)
    {
        if (oid == null) return;
         Rigidbody rb = oid.GetComponentInChildren<Rigidbody>();
        if(rb == null)
        {
            Debug.LogError("child rb yok");
            return;
        }
            
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 dir = new Vector3(0.5f, 1f,3f).normalized;
        rb.AddForce(dir * 7f, ForceMode.Impulse);

       Debug.Log("Throwup called");
       
    }
}
    