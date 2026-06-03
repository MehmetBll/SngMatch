using UnityEngine;

/// <summary>Objenin eslesme kimligini, skor degerini ve efekt bilgilerini tutar.</summary>
public class objectId : MonoBehaviour
{
    [Header("Obje Verileri")]
    [Tooltip("Eslesme id'si (0 ise eslesme yok)")]
    public int matchId = 0;
    [Tooltip("Skor degeri (bir eslesmede eklenecek)")]
    public int score = 10;
    [Tooltip("Obje orijinal pozisyonu (runtime atanir)")]
    public Vector3 originalPosition;
    [Tooltip("Prefab adi veya referans icin isim")]
    public string prefabName;
    [Tooltip("Parca sayisi (parcalama efektleri icin)")]
    public int pieceCount = 12;
    [Tooltip("Parca/efekt rengi")]
    public Color effectColor = Color.white;
    [Tooltip("Runtime: nesne catcher tarafindan tutuluyor mu")]
    public bool isHeld = false;

    /// <summary>Obje olusunca baslangic pozisyonunu ve prefab adini kaydeder.</summary>
    private void Awake()
    {
        originalPosition = transform.position;
        prefabName = gameObject.name;
    }

    /// <summary>Baska bir objectId ile eslesip eslesmedigini kontrol eder.</summary>
    public bool IsMatch(objectId other, bool requireNonZero = true)
    {
        if (other == null) return false;
        if (requireNonZero)
        {
            if (matchId == 0 || other.matchId == 0)
                return false;
        }
        return matchId == other.matchId;
    }
}
