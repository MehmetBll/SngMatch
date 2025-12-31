using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.AI;
using System.Collections;
using System.Xml.Serialization;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.Analytics;

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
    public Material pieceMaterial;  
    public GameObject[] cWalls;
    public GameManager gameManager;
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
        if (other.CompareTag("Catchable"))
        {
            gameManager.ObjectCaught();
            Destroy(heldObject.gameObject);
        }
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
            
            BreakPieces(obj1);
            BreakPieces(obj2);

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
    void SetCWallsActive(bool state)
    {
        if (cWalls == null)
            return;

        foreach (GameObject wall in cWalls)
        {
            if (wall != null)
                wall.SetActive(state);
        }
    }
    [System.Obsolete]
    private IEnumerator ThrowUpRoutine(objectId oid)
    {
        SetCWallsActive(false);
        Rigidbody   rb = oid.GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("child rb yok");
            SetCWallsActive(true);
            yield break;
        }
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            float sideOffset = Random.Range(-1f , 1f);

            Vector3 throwDir =
            Vector3.up * Random.Range(1.5f, 2.5f) +
            Vector3.forward * Random.Range(2f, 3f) +
            Vector3.right * sideOffset;

            throwDir.Normalize();
            rb.AddForce(throwDir * throwUpForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f , ForceMode.Impulse);

            yield return new WaitForSeconds(0.5f);
            SetCWallsActive(true);

    }

    [System.Obsolete]
    private void ThrowUp(objectId oid)
    {
        if (oid == null) return;

        StartCoroutine(ThrowUpRoutine(oid));

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

        Vector3 dir = new Vector3(0.5f, 2f,3f).normalized;
        rb.AddForce(dir * 7f, ForceMode.Impulse);

       Debug.Log("Throwup called");
       
    }
    void BreakPieces(objectId oid)
    {
         Renderer rend = oid.GetComponentInChildren<Renderer>();
           if (rend == null) 
                return;

                Vector3 center = rend.bounds.center;

        for(int i =0; i<oid.pieceCount; i++)
        {
                Vector3 spawnPos = center + Random.insideUnitSphere * 0.5f;

                GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                piece.transform.position = spawnPos;
                piece.transform.localScale = Vector3.one * 0.2f;

                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.mass = 0.1f;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                Vector3 forceDir = (spawnPos - center).normalized;
                rb.AddForce(forceDir * Random.Range(2f, 5f), ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 5f);

                Renderer r =piece.GetComponent<Renderer>();

                Material matInstance = new Material(pieceMaterial);
                matInstance.color = oid.effectColor;
                r.material = matInstance;

                Destroy(piece, 2.0f);
        }
    }
    
}
    