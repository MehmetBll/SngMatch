using UnityEngine;

/// <remarks>Obje metadata: match id, skor, parca sayisi ve runtime durum.</remarks>
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

    // Bu nesnenin su anda bir catcher tarafindan tutulup tutulmadigini belirtir
    [Tooltip("Runtime: nesne catcher tarafindan tutuluyor mu")]
    public bool isHeld = false;

    public int scoreValue { get; internal set; }

    /// <remarks>Baslangicta orijinal pozisyon ve prefab adini kaydeder.</remarks>
    private void Awake()
    {
        originalPosition = transform.position;
        prefabName = gameObject.name;
    }
    /// <remarks>Iki objectId'nin eslesip eslesmedigini kontrol eder.</remarks>
    public bool IsMatch(objectId other, bool requireNonZero = true)
    {
        if (other == null) return false;
        if (requireNonZero)
        {
            if (this.matchId == 0 || other.matchId == 0)
                return false;
        }
        return this.matchId == other.matchId;
    }
}
