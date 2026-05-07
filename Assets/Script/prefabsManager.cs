using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

/// <summary>Prefabları sahneye üretir ve oyuncunun objeleri sürüklemesini yönetir.</summary>
public class prefabManager : MonoBehaviour
{
    private Transform _selectedObject;
    private Vector3 _offset;
    private CWalls wallsController;

    [Header("Prefab ve Spawn")]
    [Tooltip("Spawn edilecek prefab'lar")]
    public GameObject[] prefabs;
    [Tooltip("Spawn adedi (her prefab icin)")]
    public int spawnCount = 10;
    [Tooltip("Spawn X araligi (pozitif deger)")]
    public float posX = 7f;
    [Tooltip("Spawn Y pozisyonu (yukseklik)")]
    public float posY = 10f;
    [Tooltip("Spawn Z araligi (pozitif deger)")]
    public float posZ = 12f;
    [Tooltip("Objelerin tutulacagi yukseklik")]
    public float objectHeight = 5f;

    [Header("Girdiler ve Katmanlar")]
    [Tooltip("Draggable layer mask")]
    public LayerMask draggableMask;

    [Header("Referanslar")]
    [Tooltip("Kullanilacak kamera (varsayilan Camera.main)")]
    public Camera cam;

    /// <summary>Kamera referansını hazırlar, objeleri üretir ve duvar kontrolünü bulur.</summary>
    private void Start()
    {
        if (cam == null) cam = Camera.main;
        SpawnObjects();
        wallsController = FindAnyObjectByType<CWalls>();
    }

    /// <summary>Mouse veya dokunma girdisini okuyarak seçme, sürükleme ve bırakmayı yönetir.</summary>
    private void Update()
    {
        if (Pointer.current == null) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();

        if (Pointer.current.press.wasPressedThisFrame) TrySelect(screenPos);
        if (Pointer.current.press.isPressed && _selectedObject != null) Drag(screenPos);
        if (Pointer.current.press.wasReleasedThisFrame) ReleaseSelection();
    }

    /// <summary>Ekran pozisyonundan raycast atar ve sürüklenebilir objeyi seçer.</summary>
    private void TrySelect(Vector2 screenPos)
    {
        ReleaseSelection();

        Camera activeCamera = cam != null ? cam : Camera.main;
        if (activeCamera == null) return;

        Ray ray = activeCamera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit, 500f, draggableMask)) return;

        Transform draggableRoot = GetDraggableRoot(hit.transform);
        if (draggableRoot == null || IsHeldByCatcher(draggableRoot)) return;

        _selectedObject = draggableRoot;

        Vector3 pos = _selectedObject.position;
        pos.y = objectHeight;
        _selectedObject.position = pos;
        _offset = _selectedObject.position - hit.point;
        wallsController?.SetWallsActive(false);
    }

    /// <summary>Seçili objeyi bırakır ve kapatılan duvarları tekrar açar.</summary>
    private void ReleaseSelection()
    {
        if (_selectedObject != null)
        {
            wallsController?.SetWallsActive(true);
            _selectedObject = null;
        }
    }

    /// <summary>Seçili objeyi sabit yükseklikte kamera ray'i ile hareket ettirir.</summary>
    private void Drag(Vector2 screenPos)
    {
        if (_selectedObject == null) return;

        if (IsHeldByCatcher(_selectedObject))
        {
            ReleaseSelection();
            return;
        }

        Camera activeCamera = cam != null ? cam : Camera.main;
        if (activeCamera == null) return;

        Ray ray = activeCamera.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, objectHeight, 0));
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPos = ray.GetPoint(enter);
            worldPos += _offset;
            worldPos.y = objectHeight;
            _selectedObject.position = worldPos;
        }
    }

    /// <summary>Raycast'in vurduğu objeden Draggable layer'ındaki kök objeyi bulur.</summary>
    private Transform GetDraggableRoot(Transform t)
    {
        int layer = LayerMask.NameToLayer("Draggable");
        if (t.gameObject.layer == layer) return t;
        if (t.parent != null && t.parent.gameObject.layer == layer) return t.parent;
        return null;
    }

    /// <summary>Objenin catcher tarafından tutulup tutulmadığını kontrol eder.</summary>
    private bool IsHeldByCatcher(Transform t)
    {
        if (t == null) return false;
        objectId oid = t.GetComponentInParent<objectId>();
        return oid != null && oid.isHeld;
    }

    /// <summary>Inspector'daki prefabları rastgele pozisyonlarda sahneye üretir.</summary>
    private void SpawnObjects()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPoz = new Vector3(
                Random.Range(-posX, posX),
                Random.Range(posY, posY),
                Random.Range(-posZ, posZ)
            );
            foreach (GameObject prefab in prefabs)
            {
                GameObject spawned = Instantiate(prefab, randomPoz, Quaternion.identity);
                Vector3 p = spawned.transform.position;
                p.y = objectHeight;
                spawned.transform.position = p;
            }
        }
    }
}
