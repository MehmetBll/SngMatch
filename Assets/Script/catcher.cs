using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideCatcher3D : MonoBehaviour
{
    public bool useTagComparison = false;
    public Transform returnPoint;
    public float returnRandomRadius = 0.5f;
    public float returnDuration = 1.0f;
    public float processDelay = 0.05f;
    private List<GameObject> inside = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        var go = other.gameObject;
        if (!IsValidCandidate(go)) return;

        if (!inside.Contains(go))
            inside.Add(go);

        if (inside.Count >= 2)
            StartCoroutine(DelayedProcessPairs());
    }

    private void OnTriggerExit(Collider other)
    {
        var go = other.gameObject;
        if (inside.Contains(go))
            inside.Remove(go);
    }

    private bool IsValidCandidate(GameObject go)
    {
        if (useTagComparison) return true;
        return go.GetComponent<objectId>() != null;
    }

    private IEnumerator DelayedProcessPairs()
    {
        yield return new WaitForSeconds(processDelay);
        TryProcessPairs();
    }

    private void TryProcessPairs()
    {
        while (inside.Count >= 2)
        {
            var a = inside[0];
            var b = inside[1];

            if (a == null) { inside.RemoveAt(0); continue; }
            if (b == null) { inside.RemoveAt(1); continue; }

            bool isMatch = false;
            if (useTagComparison)
            {
                isMatch = (a.tag == b.tag);
            }
            else
            {
                var ma = a.GetComponent<objectId>();
                var mb = b.GetComponent<objectId>();
                if (ma != null && mb != null)
                    isMatch = (ma.matchId == mb.matchId);
            }

            inside.RemoveAt(1);
            inside.RemoveAt(0);

            if (isMatch)
            {
                Destroy(a);
                Destroy(b);
            }
            else
            {
                StartCoroutine(ReturnToPlayArea(a));
                StartCoroutine(ReturnToPlayArea(b));
            }
        }
    }

    private IEnumerator ReturnToPlayArea(GameObject go)
    {
        if (go == null) yield break;

        Vector3 start = go.transform.position;
        Vector3 target = ComputeReturnTarget(go);

        float elapsed = 0f;

        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            bool wasKinematic = rb.isKinematic;
            Vector3 oldVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;

            while (elapsed < returnDuration)
            {
                float t = elapsed / returnDuration;
                go.transform.position = Vector3.Lerp(start, target, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            go.transform.position = target;

            rb.isKinematic = wasKinematic;
            rb.linearVelocity = oldVelocity;
        }
        else
        {
            while (elapsed < returnDuration)
            {
                float t = elapsed / returnDuration;
                go.transform.position = Vector3.Lerp(start, target, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            go.transform.position = target;
        }
    }

    private Vector3 ComputeReturnTarget(GameObject go)
    {
        Vector3 baseTarget = Vector3.zero;

        if (returnPoint != null)
        {
            baseTarget = returnPoint.position;
        }
        else
        {
            var m = go.GetComponent<objectId>();
            if (m != null)
                baseTarget = m.originalPosition;
            else
                baseTarget = Vector3.zero;
        }

        Vector3 rand = Random.insideUnitSphere * returnRandomRadius;
        if (rand.y < 0) rand.y = -rand.y;
        return baseTarget + rand;
    }
}