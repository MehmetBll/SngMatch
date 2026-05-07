using UnityEngine;

/// <summary>Çocuk collider'lardan oluşan duvarları topluca açıp kapatır.</summary>
public class CWalls : MonoBehaviour
{
    private Collider[] wallColliders;

    /// <summary>Oyun başında duvar collider'larını aktif hale getirir.</summary>
    private void Start()
    {
        SetWallsActive(true);
    }

    /// <summary>Bu objenin altındaki tüm collider referanslarını toplar.</summary>
    private void Awake()
    {
        wallColliders = GetComponentsInChildren<Collider>();
    }
    /// <summary>Duvar collider'larını aktif veya pasif yapar.</summary>
    public void SetWallsActive(bool active)
    {
        foreach (var col in wallColliders)
        {
            col.enabled = active;
        }
    }
}
