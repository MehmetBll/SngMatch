using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using System.Collections;
using System.Reflection.Emit;


public class CatcherManager : MonoBehaviour
{
    public bool isRight = false;
    public bool useRootObjectFromCollaider = true;
    public bool requireNonZeroMatchId = true;
    public float throwUpForce = 12f;
    public Material pieceMaterial;
    public GameObject[] cWalls;
    public GameManager gameManager;
    public Transform centerPoint;
    private objectId heldObject;
    private static CatcherManager CatcherL;
    private static CatcherManager CatcherR;


    //sol veya sağ catcherin aktif olduğunu bilmek için
    private void OnEnable()
    {
        RegisterInstance();
    }

    private void OnDisable()
    {
        UnregisterInstance();
    }
    //
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

    //obje catchere girerse bu kod satırı çaılışır
    private void OnTriggerEnter(Collider other)
    {
        // Collider'ın üst hiyerarşisinden objectId alınır
        var oid = other.GetComponentInParent<objectId>();
        if (oid == null) return;
        // Eğer nesne zaten başka bir catcher tarafından tutuluyorsa yeni catcher almaz
        if (oid.isHeld) return;
        if (requireNonZeroMatchId && oid.matchId == 0) return;
        // Eğer bu catcher zaten bir nesneye sahipse yeni bir nesne almaz
        if (heldObject != null) return;

        oid.isHeld = true;
        heldObject = oid;
        TryProcessPairWithOtherCatcher();
    }

    private void OnTriggerExit(Collider other)
    {
        // Bir collider çıkarken, objeyi belirle ve yalnızca bizim tuttuğumuz objeyse serbest bırak
        var oid = other.GetComponentInParent<objectId>();
        if (oid == null) return;

        if (heldObject == oid)
        {
            oid.isHeld = false;
            heldObject = null;
        }
    }

    private void TryProcessPairWithOtherCatcher()
    {
        //objenin olmadığı karşı catcheri bulşur
        CatcherManager other = isRight ? CatcherL : CatcherR;
        if (other == null) return;
        var obj1 = GetObjectInCenter();
        var obj2 = other.GetObjectInCenter();
        if (obj1 == null || obj2 == null) return;

        int id1 = obj1.matchId;
        int id2 = obj2.matchId;
        //id 1 ve id2 li objeler eşleşiyor ise:
        if (id1 == id2)
        {
            //skor ekle
            int scoreValue = obj1.score + obj2.score;
            //combo sistemini tetikle
            ScoreManager.Instance.AddScore(scoreValue, true);
            //parçala
            BreakPieces(obj1);
            BreakPieces(obj2);
            //yok et
            // yok etmeden önce isHeld flag'lerini temizle (destroy edilecek olsa bile)
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
            //yanlış eşleşme varsa combo'yu sıfırla
            ScoreManager.Instance.ResetCombo();
            obj1.isHeld = false;
            obj2.isHeld = false;
            // clear held references
            this.heldObject = null;
            other.heldObject = null;
            // sonra fırlat
            ThrowUp(obj1);
            ThrowUp(obj2);
        }
    }

    // Centere çekme/parent işlemleri kaldırıldı — nesnelerin fiziklerine dokunulmuyor.

    // ReleaseObject removed — physics not altered by catcher anymore.

    // GetRootTransform removed — not needed when not reparenting objects.

    //CWalls objelerini açar kapatır
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

    // Return the object currently tracked by this catcher (no centering search).
    private objectId GetObjectInCenter()
    {
        return heldObject;
    }

    [System.Obsolete]
    private IEnumerator ThrowUpRoutine(objectId oid)
    {
        //duvarları obje fırlatılır veya mouse ile tutularsa kapatır
        SetCWallsActive(false);
        //donuk ve bir anda ebjeler fırlatılmasın diye fiziksel bir hava katar 
        Rigidbody rb = oid.GetComponentInChildren<Rigidbody>();
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

        float sideOffset = Random.Range(-1f, 1f);
        //fırlatma yönü
        Vector3 throwDir =
        Vector3.up * Random.Range(1.5f, 2.5f) +
        Vector3.forward * Random.Range(2f, 3f) +
        Vector3.right * sideOffset;

        throwDir.Normalize();
        //objeleri rasgele bir yere fırlatır yukarıdada nekadar bir mesafeye atılacağı var
        rb.AddForce(throwDir * throwUpForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        //0.5 saniye sonra duvarları geri açar
        yield return new WaitForSeconds(0.5f);
        SetCWallsActive(true);

    }

    private void ThrowUp(objectId oid)
    {
        if (oid == null) return;
        // Duvarları hemen aç (açılırsa, kapatılabilir)
        SetCWallsActive(false);

        //fizik katar
        Rigidbody rb = oid.GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("child rb yok");
            SetCWallsActive(true); // Hata durumunda duvarları aç
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Fırlatma yönü
        float sideOffset = Random.Range(-1f, 1f);
        Vector3 throwDir =
            Vector3.up * Random.Range(1.5f, 2.5f) +
            Vector3.forward * Random.Range(2f, 3f) +
            Vector3.right * sideOffset;

        throwDir.Normalize();
        rb.AddForce(throwDir * throwUpForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        // Coroutine ile duvarları belirli süre açık tut sonra kapa
        StartCoroutine(ResetWallsAfterDelay(1.2f));
        Debug.Log("Throwup called");
    }

    private IEnumerator ResetWallsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetCWallsActive(true);
    }
    void BreakPieces(objectId oid)
    {

        Renderer rend = oid.GetComponentInChildren<Renderer>();
        if (rend == null)
            return;

        Vector3 center = rend.bounds.center;
        //küçük küreler oluşturur
        for (int i = 0; i < oid.pieceCount; i++)
        {
            Vector3 spawnPos = center + Random.insideUnitSphere * 0.5f;
            //küçük küreler oluşturur
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piece.transform.position = spawnPos;
            piece.transform.localScale = Vector3.one * 0.2f;

            Rigidbody rb = piece.AddComponent<Rigidbody>();
            rb.mass = 0.1f;
            //daha gerçekçi fizik için
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Vector3 forceDir = (spawnPos - center).normalized;
            //patlatma hissi verir 
            rb.AddForce(forceDir * Random.Range(2f, 5f), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f);

            Renderer r = piece.GetComponent<Renderer>();

            //her objeye göre renk olutur
            Material matInstance = new Material(pieceMaterial);
            matInstance.color = oid.effectColor;
            r.material = matInstance;

            //2 saniye sonra parçalar yok olur
            Destroy(piece, 2.0f);
        }
    }

}
