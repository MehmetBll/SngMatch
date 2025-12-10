using Unity.Mathematics;
using UnityEngine;

public class CatcherManager : MonoBehaviour
{
   public bool isRight = false;
   public bool useTagComparison = false;
   public bool useRootObjectFromCollaider = true;
   public bool requireNonZeroMatchId = true;
   public float retrunRandomRadius = 0.5f;
   public float returnDuration = 1.0f;
   public float processDelay = 0.05f;
   public float throwUpForce = 5f;
   public Transform retrunPoint;
   private static SideCatcher3D CatcherL;
   private static SideCatcher3D CatcherR;
   private objectId heldObject;

    private void OnEnable()
    {
        registerInstance();
    }
    private void OnDisable()
    {
        unregisterInstance();
    }
    private void registerInstance()
    {
        if(isRight)
        {
            CatcherR = this;
        }
        else
        {
            CatcherL = this;
        }
    }
    private void unregisterInstance()
    {
        if(isRight)
        {
            if(CatcherR == this) CatcherR = null;
        }
        else
        {
            if(CatcherL == this) CatcherL = null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        var go = useRootObjectFromCollaider ? other.transform.root.gameObject : other.gameObject;
            if(go == null) 
                return;
        var oid =go.GetComponent<objectId>();
        if(oid == null)
            return;
            if(requireNonZeroMatchId && oid.matchId == 0)
            return;
            heldObject = oid;
            TryProcessPairWithOtherCatcher();
    }
}
