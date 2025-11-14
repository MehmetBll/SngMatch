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
            
        }
    }
}
//prefablar instantiate edilecek array liste toplanacak
