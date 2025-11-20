using Unity.Mathematics;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

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
        {
            Ray  ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                _selectObject = hit.transform;
                _distance = hit.distance;
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
            Instantiate(prefab,randomPoz,Quaternion.identity);
        }
    
        }
    }
}

