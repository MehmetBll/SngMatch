using Unity.Mathematics;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject[] prefabs;
    void Awake()
    {
        prefabs = Resources.LoadAll<GameObject>("Prefabs");
    }
    void Start()
    {
        foreach(GameObject prefab in prefabs)
        {
            Instantiate(prefab,Vector3.zero,quaternion.identity);
        }
    }
}

