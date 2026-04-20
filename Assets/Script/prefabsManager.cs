using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

/// <remarks>Prefab spawn ve surukleme islemleri (mouse/touch).</remarks>
public class prefabManager : MonoBehaviour
{
    private Transform _selectedObject;
    private Transform _target;
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
    [Tooltip("Zemin raycast icin layer mask")]
    public LayerMask floorMask;

    [Header("Referanslar")]
    [Tooltip("Kullanilacak kamera (varsayilan Camera.main)")]
    public Camera cam;

    /// <remarks>Baslangicta kamera ve prefab spawnlarini ayarlar.</remarks>
    void Start()
    {
        // raycast: cam main camerayi referans alir, spawn oyunun basinda objeleri spawn eder, cwalls duvarlari kontrol eden scripti sahnede bulur
        cam = Camera.main;
        SpawnObjects();
        wallsController = FindAnyObjectByType<CWalls>();
    }

    /// <remarks>Input okuma ve obje surukleme mantigini isler.</remarks>
    void Update()
    {
        // handle mouse ve touch input sistemi ile her frame kontrol eder
        HandleMouse();
        HandleTouch();
        if (Pointer.current == null) return;
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPos);

        // obje tutulmuyorsa veya tutuyorsa farkli islem
        if (_target == null)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, draggableMask))
                {
                    _target = hit.collider.transform;
                    _offset = _target.position - hit.point;
                    Vector3 pos = _target.position;
                    pos.y = objectHeight;
                    _target.position = pos;
                    wallsController?.SetWallsActive(false);
                }
            }

            if (Pointer.current.press.isPressed && _target != null)
            {
                if (Physics.Raycast(ray, out RaycastHit floorHit, 200f, floorMask))
                {
                    Vector3 newPos = floorHit.point + _offset;
                    newPos.y = objectHeight;
                    _target.position = newPos;
                }
            }

            if (Pointer.current.press.wasReleasedThisFrame && _target != null)
            {
                wallsController?.SetWallsActive(true);
                _target = null;
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit floorHit, 200f, floorMask))
            {
                Vector3 newPos = floorHit.point + _offset;
                _target.position = new Vector3(newPos.x, objectHeight, newPos.z);
            }
            if (!Pointer.current.press.isPressed) _target = null;
        }
    }

    /// <remarks>Mouse inputlarini isleyip secme ve suruklemeyi kontrol eder.</remarks>
    void HandleMouse()
    {
        if (Mouse.current == null) return;
        var mouse = Mouse.current;
        if (mouse.leftButton.wasPressedThisFrame)
        {
            TrySelect(mouse.position.ReadValue());
            if (_selectedObject != null) wallsController?.SetWallsActive(false);
        }
        if (mouse.leftButton.isPressed && _selectedObject != null) Drag(mouse.position.ReadValue());
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (_selectedObject != null) wallsController?.SetWallsActive(true);
        }
    }

    /// <remarks>Touch inputlarini isleyip secme ve suruklemeyi kontrol eder.</remarks>
    void HandleTouch()
    {
        if (Touchscreen.current == null) return;
        var t = Touchscreen.current.primaryTouch;
        if (t.press.wasPressedThisFrame) TrySelect(t.position.ReadValue());
        if (_selectedObject != null) wallsController?.SetWallsActive(false);
        if (t.press.isPressed && _selectedObject != null) Drag(t.position.ReadValue());
        if (t.press.wasReleasedThisFrame)
        {
            if (_selectedObject != null) wallsController?.SetWallsActive(true);
        }
    }

    /// <remarks>Raycast ile draggable objeyi secer ve offset hesaplar.</remarks>
    void TrySelect(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, draggableMask))
        {
            _selectedObject = GetDraggableRoot(hit.transform);
            if (_selectedObject != null)
            {
                Vector3 pos = _selectedObject.position;
                pos.y = objectHeight;
                _selectedObject.position = pos;
                wallsController?.SetWallsActive(false);
                _offset = _selectedObject.position - hit.point;
            }
        }
    }

    /// <remarks>Suruklenen objeyi dunyadaki plane uzerinde hareket ettirir.</remarks>
    void Drag(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, objectHeight, 0));
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPos = ray.GetPoint(enter);
            worldPos += _offset;
            worldPos.y = objectHeight;
            _selectedObject.position = worldPos;
        }
    }

    /// <remarks>Draggable layer'ina ait kok transformu dondurur.</remarks>
    Transform GetDraggableRoot(Transform t)
    {
        int layer = LayerMask.NameToLayer("Draggable");
        if (t.gameObject.layer == layer) return t;
        if (t.parent != null && t.parent.gameObject.layer == layer) return t.parent;
        return null;
    }

    /// <remarks>Inspector ayarlarına gore ornek prefablar spawn eder.</remarks>
    void SpawnObjects()
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
