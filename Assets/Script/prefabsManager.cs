using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;


public class prefabManager : MonoBehaviour
{
    private Transform _selectedObject;
    private float _distance;
    private Transform _target;
    private Vector3 _offset;
    private Rigidbody rb;
    private CWalls[] WallsController;
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
    public float objectHeight = 5f;

    [System.Obsolete]
    void Start()
    {
        cam= Camera.main;
        SpawnObjects();
        WallsController = FindObjectsOfType<CWalls>();
    }
    void Update()
    {
        HandleMouse();
        HandleTouch();
        Vector2 screenPos;
        if(Pointer.current != null)
    {
        screenPos = Pointer.current.position.ReadValue();

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
                        _target.position = new Vector3(_target.position.x, objectHeight, _target.position.z);
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
                _target.position = new Vector3(newPos.x, objectHeight, newPos.z);
            }
            if(!Pointer.current.press.isPressed)
            {
                _target = null;
            }
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
            TrySelect(mouse.position.ReadValue());
            if(_selectedObject != null)
            foreach (var wall in WallsController)
                wall.SetWallsActive(false); 
            
            
        }

        if (mouse.leftButton.isPressed && _selectedObject != null)
            Drag(mouse.position.ReadValue());

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if(_selectedObject = null)
            foreach (var wall in WallsController)
                wall.SetWallsActive(true);
        }
    }
    void HandleTouch()
    {
        if (Touchscreen.current == null) 
            return;
        var t = Touchscreen.current.primaryTouch;

        if (t.press.wasPressedThisFrame)
            TrySelect(t.position.ReadValue());
            if(_selectedObject != null)
            foreach (var wall in WallsController)
                wall.SetWallsActive(false);

        if (t.press.isPressed && _selectedObject != null)
            Drag(t.position.ReadValue());

        if (t.press.wasReleasedThisFrame)
        {
            if(_selectedObject = null)
            foreach (var wall in WallsController)
                wall.SetWallsActive(true);
        }
    }
    void TrySelect(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, draggableMask))
        {
            _selectedObject = GetDraggableRoot(hit.transform);

            if (_selectedObject != null)
            {
                Vector3 pos= _selectedObject.position;
                pos.y = objectHeight;

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
           Vector3 p = spawned.transform.position;
           p.y = objectHeight;
           spawned.transform.position = p;
        }
    
        }
    }
}
