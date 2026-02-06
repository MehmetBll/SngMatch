using UnityEngine;

public class objectId : MonoBehaviour
{
public int matchId = 0;
public int id;
public int score = 10;
public Vector3 originalPosition;
public string prefabName;
public int pieceCount = 12;
public Color effectColor = Color.white;

         public int scoreValue { get; internal set; }

         private void Awake()
    {
        originalPosition = transform.position;
        prefabName = gameObject.name;
    }
    public bool IsMatch(objectId other, bool requireNonZero = true)
    {
        if (other == null) return false;
        if(requireNonZero)
        {
            if(this.matchId == 0 || other.matchId == 0)
            return false;
        }
        return this.matchId == other.matchId;
    }
}
