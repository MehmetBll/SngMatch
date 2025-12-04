using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

public class SideCatcher3D : MonoBehaviour
{
    public bool useTagComparison = false;
    public Transform returnPoint;
    public float returnRandomRadius = 0.5f;
    public float returnDuration = 1.0f;
    public float processDelay = 0.05f;
    public bool useRootObjectFromCollaider = true;
    private bool isProcessing = false;
    private List<GameObject> inside = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        var go = other.gameObject;
        if (!IsValidCandidate(go)) 
            return;

        if (!inside.Contains(go))
            inside.Add(go);

        if (inside.Count >= 2 && !isProcessing)
            StartCoroutine(DelayedProcessPairs());
    }

    private void OnTriggerExit(Collider other)
    {
        var go = useRootObjectFromCollaider ? other.transform.root.gameObject : other.gameObject;
        if (inside.Contains(go))
            inside.Remove(go);
    }

    private bool IsValidCandidate(GameObject go)
    {
        if (useTagComparison) 
            return true;
        return go.GetComponent<objectId>() != null;
    }

    private IEnumerator DelayedProcessPairs()
    {
        isProcessing = true;
        yield return new WaitForSeconds(processDelay);
        TryProcessPairs();
        isProcessing = false;
    }

    private void TryProcessPairs()
    {
        inside.RemoveAll(x=> x == null);
        //
        var tDestroy = new List<GameObject>();
        var tReturn = new List<GameObject>();

        while (inside.Count >= 2)
        {
            inside.RemoveAll(x => x == null);
            if (inside.Count < 2)
                break;
                int i = Random.Range(0, inside.Count);
            GameObject a = inside[i];
            if (a == null)
            {
                inside[i] = null;
                continue;
            }

            int matchIndex = -1;
            for (int j = 0; j < inside.Count; j++)
            {
                if (j == i) continue;
                GameObject b = inside[j];
                if (b == null) continue;

                bool isMatch = false;
                if (useTagComparison)
                {
                    isMatch = a.tag == b.tag;
                }
                else
                {
                    var ma = a.GetComponent<objectId>();
                    var mb = b.GetComponent<objectId>();
                    if (ma != null && mb != null)
                        isMatch = ma.matchId == mb.matchId;
                }

                if (isMatch)
                {
                    matchIndex = j;
                    break;
                }
            }

            if (matchIndex != -1)
            {
                GameObject bObj = inside[matchIndex];
                tDestroy.Add(a);
                tDestroy.Add(bObj);

                inside[i] = null;
                inside[matchIndex] = null;
            }
            else
            {
                tReturn.Add(a);
                inside[i] = null;
            }

            inside.RemoveAll(x => x == null);
        }

        if (inside.Count > 0)
        {
            foreach (var go in inside)
            {
                if (go != null)
                    tReturn.Add(go);
            }
            inside.Clear();
        }

        foreach (var g in tDestroy)
        {
            if (g != null)
                Destroy(g);
        }

        foreach (var g in tReturn)
        {
            if (g != null)
                StartCoroutine(ReturnToPlayArea(g));
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
                baseTarget = transform.position;
        }

        Vector3 randomOffset = Random.insideUnitSphere * returnRandomRadius;
        return baseTarget + randomOffset;
    }
}