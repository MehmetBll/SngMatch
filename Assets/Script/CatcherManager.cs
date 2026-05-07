using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>Catcher bölgelerinde objeyi tutar, karşı catcher ile eşleşmeyi kontrol eder.</summary>
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

    /// <summary>Script aktifleşince sol veya sağ catcher referansını kaydeder.</summary>
    private void OnEnable() { RegisterInstance(); }

    /// <summary>Script kapanınca kayıtlı catcher referansını temizler.</summary>
    private void OnDisable() { UnregisterInstance(); }

    /// <summary>Tutulan objeyi fizik adımlarında catcher merkezinde sabit tutar.</summary>
    private void FixedUpdate()
    {
        if (currentState != CatcherState.Magnet || heldObject == null) return;
        LockHeldObjectToCenter();
    }

    /// <summary>Bu catcher'ı sol veya sağ taraf olarak statik kayda alır.</summary>
    private void RegisterInstance()
    {
        if (!HasEnabledTriggerCollider()) return;

        if (isRight) CatcherR = this; else CatcherL = this;
    }

    /// <summary>Bu catcher'ın statik kaydını güvenli şekilde siler.</summary>
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

    /// <summary>Trigger'a giren geçerli objeyi yakalar ve eşleşme kontrolünü başlatır.</summary>
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

    /// <summary>İki catcher'daki objeleri karşılaştırır; doğruysa yok eder, yanlışsa fırlatır.</summary>
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

    /// <summary>Objeyi catcher'a kilitler, fiziğini durdurur ve tutuluyor olarak işaretler.</summary>
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

    /// <summary>Tutulan obje referanslarını ve tutuluyor bayrağını temizler.</summary>
    private void ClearHeldObject()
    {
        if (heldObject != null)
            heldObject.isHeld = false;

        heldObject = null;
        heldRigidbody = null;
        currentState = CatcherState.Idle;
    }

    /// <summary>Objeyi fırlatma için serbest bırakır ve geçici olarak tekrar yakalanmasını engeller.</summary>
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

    /// <summary>Tutulan objenin görünen merkezini catcher merkezine hizalar.</summary>
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

    /// <summary>Bu objede aktif trigger collider olup olmadığını kontrol eder.</summary>
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

    /// <summary>Catcher ile ilişkili duvar objelerini açar veya kapatır.</summary>
    private void SetCWallsActive(bool state)
    {
        if (cWalls == null) return;
        foreach (GameObject wall in cWalls) if (wall != null) wall.SetActive(state);
    }

    /// <summary>Bu catcher'ın şu anda tuttuğu objeyi döndürür.</summary>
    private objectId GetObjectInCenter() { return heldObject; }

    /// <summary>Yanlış eşleşen objeyi fizik kuvvetiyle yukarı fırlatır.</summary>
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

    /// <summary>Fırlatma coroutine'ini güvenli şekilde başlatır.</summary>
    private void ThrowUp(objectId oid)
    {
        if (oid == null) return;
        StartCoroutine(ThrowUpRoutine(oid));
    }

    /// <summary>Doğru eşleşen obje için kısa süreli parçalanma efekti üretir.</summary>
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
