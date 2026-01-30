using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using System.Collections;
using System.Reflection.Emit;


public class CatcherManager : MonoBehaviour
{
    public bool isRight = false;
    public bool useRootObjectFromCollaider = true;
    public bool requireNonZeroMatchId = true;
    public float throwUpForce = 5f;
    public Material pieceMaterial;  
    public GameObject[] cWalls;
    public GameManager gameManager;
    public Transform centerPoint;
    private static CatcherManager CatcherL;
    private static CatcherManager CatcherR;
    public float magnetSpeed = 10f;
    private objectId heldObject;


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

    [System.Obsolete]
    //obje catchere girerse bu kod satırı çaılışır
    private void OnTriggerEnter(Collider other)
    {
        //obje child mi yoksa root mu ona bakıyor ona göre doğru objeyi alıyor
        var go = useRootObjectFromCollaider ? other.transform.root.gameObject : other.gameObject;
        if (go == null) return;
        //objenin eşleşen id si varmı diye bakar
        var oid = go.GetComponent<objectId>();
        if (oid == null) return;
        if (requireNonZeroMatchId && oid.matchId == 0) return;
        //catcher doluysa yeni objeyi almaz
        if (heldObject != null) return;

        if(!other.CompareTag("Catchable")) return;
        Rigidbody rb = oid.GetComponentInChildren<Rigidbody>();
        if(rb == null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.LogError("child rb yok");
            return;
        }

        Vector3 dir = (centerPoint.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * magnetSpeed * Time.fixedDeltaTime);
        
        heldObject = oid;
        //diğer catchere giren objeyi kontrol eder
        TryProcessPairWithOtherCatcher();
    }
    
    private void OnTriggerExit(Collider other)
    {
        //obje catcherden çıkarsa bu kod çalışır
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
        //objenin olmadığı karşı catcheri bulşur
        CatcherManager other = isRight ? CatcherL : CatcherR;
        if (other == null) return;
        if (other.heldObject == null) return;

        var obj1 = this.heldObject;
        var obj2 = other.heldObject;
        //iki tarafta dolu değilse işlem yapmaması için tek catcher 
        if (obj1 == null || obj2 == null)
        {
            if (obj1 == null) this.heldObject = null;
            if (obj2 == null) other.heldObject = null;
            return;
        }

        int id1 = obj1.matchId;
        int id2 = obj2.matchId;
        //id 1 ve id2 li objeler eşleşiyor ise:
        if (id1 == id2)
        {
            //parçala
            BreakPieces(obj1);
            BreakPieces(obj2);
            //yok et
            Destroy(obj1.gameObject);
            Destroy(obj2.gameObject);

            
            gameManager.CaughtDestroy();
        }
        else
        {
            //yalnış eşleşme varsa fırlatır
            ThrowUp(obj1);
            ThrowUp(obj2);
        }

        this.heldObject = null;
        other.heldObject = null;
    }
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

    [System.Obsolete]
    private IEnumerator ThrowUpRoutine(objectId oid)
    {
        //duvarları obje fırlatılır veya mouse ile tutularsa kapatır
        SetCWallsActive(false);
        //donuk ve bir anda ebjeler fırlatılmasın diye fiziksel bir hava katar 
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
            //fırlatma yönü
            Vector3 throwDir =
            Vector3.up * Random.Range(1.5f, 2.5f) +
            Vector3.forward * Random.Range(2f, 3f) +
            Vector3.right * sideOffset;

            throwDir.Normalize();
            //objeleri rasgele bir yere fırlatır yukarıdada nekadar bir mesafeye atılacağı var
            rb.AddForce(throwDir * throwUpForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 5f , ForceMode.Impulse);
            //0.5 saniye sonra duvarları geri açar
            yield return new WaitForSeconds(0.5f);
            SetCWallsActive(true);

    }

    [System.Obsolete]
    private void ThrowUp(objectId oid)
    {
        
        if (oid == null) return;
        //asıl fırlatma kısmı bura
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

       Debug.Log("Throwup called");
       
    }
    void BreakPieces(objectId oid)
    {
           
         Renderer rend = oid.GetComponentInChildren<Renderer>();
           if (rend == null) 
                return;

                Vector3 center = rend.bounds.center;
        //küçük küreler oluşturur
        for(int i =0; i<oid.pieceCount; i++)
        {
                Vector3 spawnPos = center + Random.insideUnitSphere * 0.5f;

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

                Renderer r =piece.GetComponent<Renderer>();

                //her objeye göre renk olutur
                Material matInstance = new Material(pieceMaterial);
                matInstance.color = oid.effectColor;
                r.material = matInstance;

                //2 saniye sonra parçalar yok olur
                Destroy(piece, 2.0f);
        }
    }
    
}
    