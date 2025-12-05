using UnityEngine;

public class objectId : MonoBehaviour
{
public int matchId = 0;
public Vector3 originalPosition;
public string prefabName;
    private void Awake()
    {
        originalPosition = transform.position;
        prefabName = gameObject.name;
    }
    private void OValidate()
    {
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
