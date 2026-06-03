using UnityEngine;

/// <summary>Tek bir prefab objesine, merkezi telefon sallama komutunu uygular.</summary>
[DisallowMultipleComponent]
public class PrefabShake : MonoBehaviour
{
    private Rigidbody[] rigidbodies;
    private objectId objectData;

    private void Awake()
    {
        CacheReferences();
    }

    /// <summary>Child objelerdeki Rigidbody'leri de dahil ederek referanslari hazirlar.</summary>
    public void CacheReferences()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        objectData = GetComponentInParent<objectId>();
    }

    /// <summary>Telefon sallama siddetine gore bu prefab objesini hareket ettirir.</summary>
    public void ApplyPhoneShake(Vector3 impulse, Vector3 torque, bool affectHeldObjects)
    {
        if (!affectHeldObjects && objectData != null && objectData.isHeld)
            return;

        if (rigidbodies == null || rigidbodies.Length == 0)
            CacheReferences();

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb == null || rb.isKinematic)
                continue;

            rb.WakeUp();
            rb.AddForce(impulse, ForceMode.Impulse);
            rb.AddTorque(torque, ForceMode.Impulse);
        }
    }
}
