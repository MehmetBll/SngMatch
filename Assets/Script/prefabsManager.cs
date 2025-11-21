using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using UnityEditor.PackageManager;

public class prefabManager : MonoBehaviour
{
    private Transform _selectObject;
    private float _distance;
    public GameObject[] prefabs;
    //denemelik
    public int spawnCount=10;
    public float posX = 7f;
    public float posY = 10f;
    public float posZ = 12f;

    void Start()
    {
        SpawnObjects();
    }
    void Update()
    {
        Mouse mouse =Mouse.current;
        if (mouse == null)
        return;
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray  ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Transform draggable = GetDraggableRoot(hit.transform);
                if(draggable !=null)
            {
                _selectObject = draggable;
                _distance = hit.distance;
            }
            }
        }

        if(mouse.leftButton.isPressed && _selectObject !=null)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            Vector3 targetPos = ray.GetPoint(_distance);
            _selectObject.position=targetPos;
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            _selectObject=null;
        }
    }
    Transform GetDraggableRoot(Transform t)
    {
        if(t.CompareTag("Draggable"))
        return t;

        if(t.parent != null && t.parent.CompareTag("Draggable"))
        return t.parent;

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
//raycast araştırıldı pekiştirilecek sonrasında mouse ile tıklandığında (collaider lara) ve tıklama devam ettiğinde mouseyi takip edecek bir raycast fonk. yazılacak 

