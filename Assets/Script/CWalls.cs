using System.Globalization;
using UnityEngine;

public class CWalls : MonoBehaviour
{
private Collider[] wallColliders;

    private void Awake()
    {
        wallColliders = GetComponentsInChildren<Collider>();
    }
    public void SetWallsActive(bool active)
    {
        foreach (var col in wallColliders)
        {
            col.enabled = active;
        }
    }
}
