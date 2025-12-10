using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class SideCatcher3D : MonoBehaviour
{
    public bool useTagComparison = false;
    public Transform returnPoint;
    public float returnRandomRadius = 0.5f;
    public float returnDuration = 1.0f;
    public float processDelay = 0.05f;
    public bool useRootObjectFromCollaider = true;
    public bool requireNonZeroMatchId = true;
    public float throwUpForce = 5f;
    private List<objectId> inside = new List<objectId>();

    private void OnTriggerEnter(Collider other)
    {
        var go = other.transform.root.gameObject;
        if(go == null)
            return;

        objectId objectId = go.GetComponent<objectId>();
        if(objectId == null)
            return;
        if (requireNonZeroMatchId && objectId.matchId == 0)
            return;

        if (!inside.Contains(objectId))
            inside.Add(objectId);

        if (inside.Count >= 2)
        {
            var obj1 = inside[0];
            var obj2 = inside[1];
                
            if(obj1 == null || obj2 == null)
            {
                inside.RemoveAll(x => x == null);
                    return;
            }
            int objId1 = obj1.matchId;
            int objId2 = obj2.matchId;

            if(objId1 == objId2 && (!requireNonZeroMatchId || objId1 != 0))
            {
                inside.Remove(obj1);
                inside.Remove(obj2);

                if(obj1.gameObject != null)
                Destroy(obj1.gameObject);
                if(obj2.gameObject != null)
                    Destroy(obj2.gameObject);
            }
            else
            {
                // obj1 ve obj2 yi yukarı fırlat
                if (obj1 != null)
                    throwUp(obj1);
                if (obj2 != null)
                    throwUp(obj2);
                    inside.Remove(obj1);
                    inside.Remove(obj2);

            }
        }
        //    StartCoroutine(DelayedProcessPairs());
    }
    private void throwUp(objectId oid)
    {
        if (oid == null)
            return;
            GameObject go = oid.gameObject;
        if (go == null)
            return;
            var rb = go.GetComponent<Rigidbody>();
            if ( rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.up * throwUpForce, ForceMode.Impulse);
        }
        else
        {
            go.transform.position += Vector3.up * 0.5f;
        }
    }



/*    private void OnTriggerExit(Collider other)
    {
        var go = useRootObjectFromCollaider ? other.transform.root.gameObject : other.gameObject;
        if (inside.Contains(go))
            inside.Remove(go);
            Debug.Log("catcherden çikti");
    }

    private bool IsValidCandidate(GameObject go)
    {
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

                bool IsMatch = false;
                if (useTagComparison)
                {
                    IsMatch = a.tag == b.tag;
                }
                else
                {
                    var ma = a.GetComponent<objectId>();
                    var mb = b.GetComponent<objectId>();
                    if (ma != null && mb != null)
                        IsMatch = ma.matchId == mb.matchId;
                }

                if (IsMatch)
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
            baseTarget = returnPoint.position;
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
    }*/
}