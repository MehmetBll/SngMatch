using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class prefabManager : MonoBehaviour
{
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

