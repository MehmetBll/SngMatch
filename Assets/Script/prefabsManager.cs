using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using UnityEditor.PackageManager;
using UnityEditor;

public class prefabManager : MonoBehaviour
{
    private Transform _selectedObject;
    private float _distance;
    private Transform _target;
    private Vector3 _offset;
    public GameObject[] prefabs;
    public LayerMask draggableMask;
    public Camera cam; 
    public LayerMask floorMask;
    public string draggableTag = "Draggable";
    //denemelik
    public int spawnCount=10;
    public float posX = 7f;
    public float posY = 10f;
    public float posZ = 12f;
    public float objectHeight = 0.5f;

    void Start()
    {
        cam= Camera.main;
        SpawnObjects();
        HandleMouse();
        HandleTouch();
    }
    void Update()
    {
        
        Vector2 screenPos = Pointer.current.position.ReadValue();

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (_target == null)
        {
            if(Pointer.current.press.isPressed)
            {
                if(Physics.Raycast(ray,out RaycastHit hit, 100f))
                {
                    if(hit.collider.CompareTag(draggableTag))
                    {
                        _target =hit.collider.transform;
                        _offset = _target.position - hit.point;
                    }
                }
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit floorHit, 200f, floorMask))
            {
                Vector3 newPos = floorHit.point + _offset;
                _target.position = new Vector3(newPos.x, _target.position.y, newPos.z);
            }
            if(!Pointer.current.press.isPressed)
            {
                _target = null;
            }
        }
    }
     void HandleMouse()
    {
        if (Mouse.current == null) 
            return;
        var mouse = Mouse.current;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Debug.Log("girdi");TrySelect(mouse.position.ReadValue());
        }
            

        if (mouse.leftButton.isPressed && _selectedObject != null)
            Drag(mouse.position.ReadValue());

        if (mouse.leftButton.wasReleasedThisFrame)
            _selectedObject = null;
    }
    void HandleTouch()
    {
        if (Touchscreen.current == null) 
            return;
        var t = Touchscreen.current.primaryTouch;

        if (t.press.wasPressedThisFrame)
            TrySelect(t.position.ReadValue());

        if (t.press.isPressed && _selectedObject != null)
            Drag(t.position.ReadValue());

        if (t.press.wasReleasedThisFrame)
            _selectedObject = null;
    }
    void TrySelect(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, draggableMask))
        {
            _selectedObject = GetDraggableRoot(hit.transform);

            if (_selectedObject != null)
            {
                _offset = _selectedObject.position - hit.point;
            }
        }
    }
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
    Transform GetDraggableRoot(Transform t)
    {
        if (t.CompareTag("Draggable")) 
            return t;
        if (t.parent != null && t.parent.CompareTag("Draggable")) return t.parent;

        return null;
    }


    void SpawnObjects()
    {
        
            for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPoz = new Vector3(
                Random.Range(-posX,posX),
                Random.Range(posY, posY),
                Random.Range(-posZ, posZ)
            );
        foreach(GameObject prefab in prefabs)
        {
           GameObject spawned = Instantiate(prefab,randomPoz,Quaternion.identity);
        }
    
        }
    }
}
