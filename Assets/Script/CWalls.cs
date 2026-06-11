using UnityEngine;

/// <summary>Cocuk collider'lardan olusan duvarlari topluca acip kapatir.</summary>
public class CWalls : MonoBehaviour
{
    private Collider[] wallColliders;

    /// <summary>Oyun basinda duvar collider'larini aktif hale getirir.</summary>
    private void Start()
    {
        SetWallsActive(true);
    }

    /// <summary>Bu objenin altindaki tum collider referanslarini toplar.</summary>
    private void Awake()
    {
        CacheColliders();
    }

    /// <summary>Duvar collider'larini aktif veya pasif yapar.</summary>
    public void SetWallsActive(bool active)
    {
        if (wallColliders == null || wallColliders.Length == 0)
            CacheColliders();

        foreach (Collider col in wallColliders)
        {
            if (col != null)
                col.enabled = active;
        }
    }

    private void CacheColliders()
    {
        wallColliders = GetComponentsInChildren<Collider>();
    }
}
