using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using System.Collections;
using System.Reflection.Emit;

/// <remarks>Catcher islemleri: trigger, eslesme, firlatma ve parcalama.</remarks>
public class CatcherManager : MonoBehaviour
{
    [Header("Catcher Ayarlari")]
    [Tooltip("Sag tarafli catcher mi")]
    public bool isRight = false;
    [Tooltip("Collider'dan root object kullanilsin mi")]
    public bool useRootObjectFromCollaider = true;
    [Tooltip("MatchId 0 olanlar eslesme icin dikkate alinsin mi")]
    public bool requireNonZeroMatchId = true;
    [Tooltip("Fırlatma gucu (impulse)")]
    public float throwUpForce = 12f;
    [Tooltip("Parcalari icin kullanılacak materyal (opsiyonel)")]
    public Material pieceMaterial;
    [Tooltip("Catcher tarafindaki duvarlari kontrol eden objeler")]
    public GameObject[] cWalls;
    [Tooltip("Referans GameManager (sahne)")]
    public GameManager gameManager;
    [Tooltip("Merkez nokta referansi (opsiyonel)")]
    public Transform centerPoint;

    private objectId heldObject;
    private static CatcherManager CatcherL;
    private static CatcherManager CatcherR;

    // sol veya sag catcherin aktif oldugunu bilmek icin
    /// <remarks>Script aktif olunca instance kayit eder.</remarks>
    private void OnEnable() { RegisterInstance(); }
    /// <remarks>Script devre disi kalinca instance kaydini siler.</remarks>
    private void OnDisable() { UnregisterInstance(); }

    /// <remarks>Bu catcher icin statik referansi ayarlar.</remarks>
    private void RegisterInstance()
    {
        if (isRight) CatcherR = this; else CatcherL = this;
    }

    /// <remarks>Bu catcher icin statik referansi temizler.</remarks>
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

    /// <remarks>Trigger girisinde objeyi isler ve eslesmeyi denetler.</remarks>
    private void OnTriggerEnter(Collider other)
    {
        // Collider'in ust hiyerarsisinden objectId alinir
        var oid = other.GetComponentInParent<objectId>();
        if (oid == null) return;
        // Eger nesne zaten baska bir catcher tarafindan tutuluyorsa yeni catcher almaz
        if (oid.isHeld) return;
        if (requireNonZeroMatchId && oid.matchId == 0) return;
        // Eger bu catcher zaten bir nesneye sahipse yeni bir nesne almaz
        if (heldObject != null) return;

        oid.isHeld = true;
        heldObject = oid;
        TryProcessPairWithOtherCatcher();
    }

    /// <remarks>Trigger cikisinda eger bizim tuttugumuz obje ise serbest birakir.</remarks>
    private void OnTriggerExit(Collider other)
    {
        // Bir collider cikarken, objeyi belirle ve yalnizca bizim tuttugumuz objeyse serbest birak
        var oid = other.GetComponentInParent<objectId>();
        if (oid == null) return;

        if (heldObject == oid)
        {
            oid.isHeld = false;
            heldObject = null;
        }
    }

    /// <remarks>Karsi catcher ile eslesme kontrolu yapar ve sonucu isler.</remarks>
    private void TryProcessPairWithOtherCatcher()
    {
        // objenin olmadigi karsi catcheri bulur
        CatcherManager other = isRight ? CatcherL : CatcherR;
        if (other == null) return;
        var obj1 = GetObjectInCenter();
        var obj2 = other.GetObjectInCenter();
        if (obj1 == null || obj2 == null) return;

        int id1 = obj1.matchId;
        int id2 = obj2.matchId;
        // id 1 ve id2 li objeler eslesiyor ise:
        if (id1 == id2)
        {
            // skor ekle
            int scoreValue = obj1.score + obj2.score;
            // combo sistemini tetikle
            ScoreManager.Instance.AddScore(scoreValue, true);
            // parcala
            BreakPieces(obj1);
            BreakPieces(obj2);
            // yok et
            // yok etmeden once isHeld flag'lerini temizle (destroy edilecek olsa bile)
            obj1.isHeld = false;
            obj2.isHeld = false;
            // clear held references on both catchers
            this.heldObject = null;
            other.heldObject = null;
            Destroy(obj1.gameObject);
            Destroy(obj2.gameObject);

            gameManager.CaughtDestroy();
        }
        else
        {
            // yanlis eslesme varsa combo'yu sifirla
            ScoreManager.Instance.ResetCombo();
            obj1.isHeld = false;
            obj2.isHeld = false;
            // clear held references
            this.heldObject = null;
            other.heldObject = null;
            // sonra firlat
            ThrowUp(obj1);
            ThrowUp(obj2);
        }
    }

    // Centere cekme/parent islemleri kaldirildi -- nesnelerin fiziklerine dokunulmuyor.
    // ReleaseObject removed -- physics not altered by catcher anymore.
    // GetRootTransform removed -- not needed when not reparenting objects.

    // CWalls objelerini acar kapatir
    /// <remarks>Ilgili duvar objelerinin aktifligini ayarlar.</remarks>
    void SetCWallsActive(bool state)
    {
        if (cWalls == null) return;
        foreach (GameObject wall in cWalls) if (wall != null) wall.SetActive(state);
    }

    // Return the object currently tracked by this catcher (no centering search).
    /// <remarks>Bu catcher'in tuttugu objectId referansini dondurur.</remarks>
    private objectId GetObjectInCenter() { return heldObject; }

    /// <remarks>Firlatma coroutine'i: fizik ve duvar kontrolu yapar.</remarks>
    private IEnumerator ThrowUpRoutine(objectId oid)
    {
        // duvarlari obje firlatilir veya mouse ile tutulursa kapatir
        SetCWallsActive(false);
        // donuk ve bir anda objeler firlatilmasin diye fiziksel bir hava katar
        Rigidbody rb = oid.GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("child rb yok");
            SetCWallsActive(true);
            yield break;
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float sideOffset = Random.Range(-1f, 1f);
        // firlatma yonu
        Vector3 throwDir = Vector3.up * Random.Range(1.5f, 2.5f) + Vector3.forward * Random.Range(2f, 3f) + Vector3.right * sideOffset;
        throwDir.Normalize();
        // objeleri rasgele bir yere firlatir, yukarida da ne kadar bir mesafeye atilacagi var
        rb.AddForce(throwDir * throwUpForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        // 0.5 saniye sonra duvarlari geri acar
        yield return new WaitForSeconds(0.5f);
        SetCWallsActive(true);
    }

    /// <remarks>Objeyi fiziksel olarak firlatir ve duvar coroutine baslatir.</remarks>
    private void ThrowUp(objectId oid)
    {
        if (oid == null) return;
        // Duvarlari hemen ac (acilursa, kapatilabilir)
        SetCWallsActive(false);
        // fizik katar
        Rigidbody rb = oid.GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("child rb yok");
            SetCWallsActive(true); // Hata durumunda duvarlari ac
            return;
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Firlatma yonu
        float sideOffset = Random.Range(-1f, 1f);
        Vector3 throwDir = Vector3.up * Random.Range(1.5f, 2.5f) + Vector3.forward * Random.Range(2f, 3f) + Vector3.right * sideOffset;
        throwDir.Normalize();
        rb.AddForce(throwDir * throwUpForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        // Coroutine ile duvarlari belirli sure acik tut sonra kapa
        StartCoroutine(ResetWallsAfterDelay(1.2f));
        Debug.Log("Atis yapildi");
    }

    /// <remarks>Belirtilen gecikme sonra duvarlari tekrar acar.</remarks>
    private IEnumerator ResetWallsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetCWallsActive(true);
    }

    /// <remarks>Objeyi parcalara ayirarak gecici parcaciklar olusturur.</remarks>
    void BreakPieces(objectId oid)
    {
        Renderer rend = oid.GetComponentInChildren<Renderer>();
        if (rend == null) return;
        Vector3 center = rend.bounds.center;
        // kucuk kureler olusturur
        for (int i = 0; i < oid.pieceCount; i++)
        {
            Vector3 spawnPos = center + Random.insideUnitSphere * 0.5f;
            // kucuk kureler olusturur
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piece.transform.position = spawnPos;
            piece.transform.localScale = Vector3.one * 0.2f;
            Rigidbody rb = piece.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            // daha gercekci fizik icin
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Vector3 forceDir = (spawnPos - center).normalized;
            // patlatma hissi verir
            rb.AddForce(forceDir * Random.Range(2f, 5f), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f);
            Renderer r = piece.GetComponent<Renderer>();
            // her objeye gore renk olustur
            Material matInstance = new Material(pieceMaterial);
            matInstance.color = oid.effectColor;
            r.material = matInstance;
            // 2 saniye sonra parcaclar yok olur
            Destroy(piece, 2.0f);
        }
    }
}
