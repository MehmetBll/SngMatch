using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>Catcher bolgelerinde objeyi tutar, karsi catcher ile eslesmeyi kontrol eder.</summary>
public class CatcherManager : MonoBehaviour
{
    private enum CatcherState
    {
        Idle,
        Magnet,
        ThrowUp
    }

    [Header("Catcher Ayarlari")]
    [Tooltip("Sag tarafli catcher mi")]
    public bool isRight = false;
    [Tooltip("MatchId 0 olanlar eslesme icin dikkate alinsin mi")]
    public bool requireNonZeroMatchId = true;
    [Tooltip("Firlatma gucu (impulse)")]
    public float throwUpForce = 10f;
    [Tooltip("Firlatmadan sonra magnet tekrar devreye girmeden once beklenecek sure")]
    public float throwUpStateDuration = 1.2f;
    [Tooltip("Parcalar icin kullanilacak materyal (opsiyonel)")]
    public Material pieceMaterial;
    [Tooltip("Catcher tarafindaki duvarlari kontrol eden objeler")]
    public GameObject[] cWalls;
    [Tooltip("Referans GameManager (sahne)")]
    public GameManager gameManager;
    [Tooltip("Merkez nokta referansi (opsiyonel)")]
    public Transform centerPoint;

    private objectId heldObject;
    private Rigidbody heldRigidbody;
    private CatcherState currentState = CatcherState.Idle;
    private static readonly HashSet<objectId> ThrowingObjects = new HashSet<objectId>();
    private static CatcherManager CatcherL;
    private static CatcherManager CatcherR;

    /// <summary>Script aktiflesince sol veya sag catcher referansini kaydeder.</summary>
    private void OnEnable() { RegisterInstance(); }

    /// <summary>Script kapaninca kayitli catcher referansini temizler.</summary>
    private void OnDisable() { UnregisterInstance(); }

    /// <summary>Tutulan objeyi fizik adimlarinda catcher merkezinde sabit tutar.</summary>
    private void FixedUpdate()
    {
        if (currentState != CatcherState.Magnet || heldObject == null) return;
        LockHeldObjectToCenter();
    }

    /// <summary>Bu catcher'i sol veya sag taraf olarak statik kayda alir.</summary>
    private void RegisterInstance()
    {
        if (!HasEnabledTriggerCollider()) return;

        if (isRight) CatcherR = this; else CatcherL = this;
    }

    /// <summary>Bu catcher'in statik kaydini guvenli sekilde siler.</summary>
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

    /// <summary>Trigger'a giren gecerli objeyi yakalar ve eslesme kontrolunu baslatir.</summary>
    private void OnTriggerEnter(Collider other)
    {
        var oid = other.GetComponentInParent<objectId>();
        if (oid == null) return;
        if (currentState != CatcherState.Idle) return;
        if (oid.isHeld || ThrowingObjects.Contains(oid)) return;
        if (requireNonZeroMatchId && oid.matchId == 0) return;
        if (heldObject != null) return;

        HoldObject(oid);
        TryProcessPairWithOtherCatcher();
    }

    /// <summary>Iki catcher'daki objeleri karsilastirir; dogruysa yok eder, yanlissa firlatir.</summary>
    private void TryProcessPairWithOtherCatcher()
    {
        CatcherManager other = isRight ? CatcherL : CatcherR;
        if (other == null) return;
        var obj1 = GetObjectInCenter();
        var obj2 = other.GetObjectInCenter();
        if (obj1 == null || obj2 == null) return;

        int id1 = obj1.matchId;
        int id2 = obj2.matchId;

        if (id1 == id2)
        {
            int scoreValue = obj1.score + obj2.score;
            ScoreManager.Instance.AddScore(scoreValue, true);
            BreakPieces(obj1);
            BreakPieces(obj2);
            ClearHeldObject();
            other.ClearHeldObject();
            Destroy(obj1.gameObject);
            Destroy(obj2.gameObject);

            gameManager.CaughtDestroy();
        }
        else
        {
            ScoreManager.Instance.ResetCombo();
            ClearHeldObject();
            other.ClearHeldObject();
            ThrowUp(obj1);
            ThrowUp(obj2);
        }
    }

    /// <summary>Objeyi catcher'a kilitler, fizigini durdurur ve tutuluyor olarak isaretler.</summary>
    private void HoldObject(objectId oid)
    {
        currentState = CatcherState.Magnet;
        oid.isHeld = true;
        heldObject = oid;
        heldRigidbody = oid.GetComponentInChildren<Rigidbody>();

        if (heldRigidbody != null)
        {
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
            heldRigidbody.useGravity = false;
            heldRigidbody.isKinematic = true;
        }

        LockHeldObjectToCenter();
    }

    /// <summary>Tutulan obje referanslarini ve tutuluyor bayragini temizler.</summary>
    private void ClearHeldObject()
    {
        if (heldObject != null)
            heldObject.isHeld = false;

        heldObject = null;
        heldRigidbody = null;
        currentState = CatcherState.Idle;
    }

    /// <summary>Objeyi firlatma icin serbest birakir ve gecici olarak tekrar yakalanmasini engeller.</summary>
    private void ReleaseHeldObjectForThrow(objectId oid)
    {
        if (oid != null)
        {
            oid.isHeld = false;
            ThrowingObjects.Add(oid);
        }

        if (heldObject == oid)
        {
            heldObject = null;
            heldRigidbody = null;
        }

        currentState = CatcherState.ThrowUp;
    }

    /// <summary>Tutulan objenin gorunen merkezini catcher merkezine hizalar.</summary>
    private void LockHeldObjectToCenter()
    {
        if (heldObject == null) return;

        Vector3 targetPosition = centerPoint != null ? centerPoint.position : transform.position;
        Renderer renderer = heldObject.GetComponentInChildren<Renderer>();
        Vector3 currentCenter = renderer != null
            ? renderer.bounds.center
            : heldRigidbody != null ? heldRigidbody.position : heldObject.transform.position;
        Vector3 delta = targetPosition - currentCenter;

        if (delta.sqrMagnitude < 0.000001f)
            return;

        heldObject.transform.position += delta;

        if (heldRigidbody != null)
        {
            if (!heldRigidbody.isKinematic)
            {
                heldRigidbody.linearVelocity = Vector3.zero;
                heldRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    /// <summary>Bu objede aktif trigger collider olup olmadigini kontrol eder.</summary>
    private bool HasEnabledTriggerCollider()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != null && col.enabled && col.isTrigger)
                return true;
        }

        return false;
    }

    /// <summary>Catcher ile iliskili duvar objelerini acar veya kapatir.</summary>
    private void SetCWallsActive(bool state)
    {
        if (cWalls == null) return;
        foreach (GameObject wall in cWalls) if (wall != null) wall.SetActive(state);
    }

    /// <summary>Bu catcher'in su anda tuttugu objeyi dondurur.</summary>
    private objectId GetObjectInCenter() { return heldObject; }

    /// <summary>Yanlis eslesen objeyi fizik kuvvetiyle yukari firlatir.</summary>
    private IEnumerator ThrowUpRoutine(objectId oid)
    {
        ReleaseHeldObjectForThrow(oid);
        SetCWallsActive(false);

        Rigidbody rb = oid.GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("child rb yok");
            ThrowingObjects.Remove(oid);
            currentState = CatcherState.Idle;
            SetCWallsActive(true);
            yield break;
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float sideOffset = Random.Range(-1f, 1f);
        Vector3 throwDir = Vector3.up * Random.Range(1.5f, 2.5f) + Vector3.forward * Random.Range(2f, 3f) + Vector3.right * sideOffset;
        throwDir.Normalize();
        rb.AddForce(throwDir * throwUpForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        Debug.Log("Atis yapildi");
        yield return new WaitForSeconds(throwUpStateDuration);
        ThrowingObjects.Remove(oid);
        currentState = CatcherState.Idle;
        SetCWallsActive(true);
    }

    /// <summary>Firlatma coroutine'ini guvenli sekilde baslatir.</summary>
    private void ThrowUp(objectId oid)
    {
        if (oid == null) return;
        StartCoroutine(ThrowUpRoutine(oid));
    }

    /// <summary>Dogru eslesen obje icin kisa sureli parcalanma efekti uretir.</summary>
    private void BreakPieces(objectId oid)
    {
        Renderer rend = oid.GetComponentInChildren<Renderer>();
        if (rend == null) return;
        Vector3 center = rend.bounds.center;

        for (int i = 0; i < oid.pieceCount; i++)
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
            Renderer r = piece.GetComponent<Renderer>();
            Material matInstance = pieceMaterial != null ? new Material(pieceMaterial) : new Material(r.sharedMaterial);
            matInstance.color = oid.effectColor;
            r.material = matInstance;
            Destroy(piece, 2.0f);
        }
    }
}
