using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Telefon sallanmasini tek yerden okuyup prefab objelerine orantili hareket uygular.</summary>
[DisallowMultipleComponent]
public class PhoneShakeObjectJiggler : MonoBehaviour
{
    [Header("Sallanacak Objeler")]
    [Tooltip("Opsiyonel: Sahnede hazir duran objeleri buraya ekleyebilirsin. Spawn edilen prefablar prefabsManager tarafindan otomatik eklenir.")]
    public GameObject[] objectsToShake;
    [Tooltip("Aciksa sahnedeki objectId tasiyan prefab objeleri baslangicta otomatik bulunur.")]
    public bool autoRegisterScenePrefabs = true;

    [Header("Sallama Algilama")]
    [Tooltip("Bu deger buyudukce telefonu daha sert sallamak gerekir.")]
    public float shakeThreshold = 0.8f;
    [Tooltip("Bu siddet ve uzeri maksimum sallama kuvveti sayilir.")]
    public float maxShakeStrength = 2.2f;
    [Tooltip("Sallama algilandiktan sonra yeni sallama icin beklenecek sure.")]
    public float shakeCooldown = 0.25f;
    [Tooltip("Yercekimi etkisini ayirmak icin ivme verisini yumusatma orani.")]
    [Range(0.01f, 0.5f)]
    public float accelerationSmoothing = 0.12f;

    [Header("Obje Tepkisi")]
    [Tooltip("Yatay sallanma kuvveti.")]
    public float sideImpulse = 4f;
    [Tooltip("Objelerin hafif ziplamasi icin yukari kuvvet.")]
    public float upwardImpulse = 1.2f;
    [Tooltip("Objelerin kendi etrafinda donme kuvveti.")]
    public float torqueImpulse = 5f;
    [Tooltip("Tek sallamada uygulanabilecek en yuksek yatay kuvvet.")]
    public float maxSideImpulse = 7f;
    [Tooltip("Catcher tarafindan tutulmus objeler de etkilensin mi?")]
    public bool affectHeldObjects = false;

    [Header("Editor Test")]
    [Tooltip("Unity Editor Play Mode'da S tusuna basinca sallama efekti denensin.")]
    public bool testWithSKey = true;

    private readonly List<PrefabShake> targets = new List<PrefabShake>();
    private Vector3 smoothedAcceleration;
    private Vector3 previousDynamicAcceleration;
    private bool hasAccelerationSample;
    private float nextAllowedShakeTime;

    private void OnEnable()
    {
        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);

        if (autoRegisterScenePrefabs)
            RegisterScenePrefabs();

        RegisterObjects(objectsToShake);
        ResetAccelerationSamples();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (testWithSKey && Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
        {
            ApplyShake(Vector3.right, maxShakeStrength);
            nextAllowedShakeTime = Time.unscaledTime + shakeCooldown;
            return;
        }

        if (!TryReadAcceleration(out Vector3 acceleration))
            return;

        if (!hasAccelerationSample)
        {
            smoothedAcceleration = acceleration;
            previousDynamicAcceleration = Vector3.zero;
            hasAccelerationSample = true;
            return;
        }

        smoothedAcceleration = Vector3.Lerp(smoothedAcceleration, acceleration, accelerationSmoothing);

        Vector3 dynamicAcceleration = acceleration - smoothedAcceleration;
        float shakeStrength = (dynamicAcceleration - previousDynamicAcceleration).magnitude;
        previousDynamicAcceleration = dynamicAcceleration;

        if (shakeStrength < shakeThreshold || Time.unscaledTime < nextAllowedShakeTime)
            return;

        ApplyShake(dynamicAcceleration, shakeStrength);
        nextAllowedShakeTime = Time.unscaledTime + shakeCooldown;
    }

    /// <summary>Spawn edilen veya sahnede duran objeyi sallama sistemine ekler.</summary>
    public void RegisterObject(GameObject root)
    {
        if (root == null)
            return;

        PrefabShake prefabShake = root.GetComponent<PrefabShake>();
        if (prefabShake == null)
            prefabShake = root.AddComponent<PrefabShake>();

        prefabShake.CacheReferences();

        if (!targets.Contains(prefabShake))
            targets.Add(prefabShake);
    }

    /// <summary>Sahnedeki objectId tasiyan prefab objelerini otomatik bulur.</summary>
    public void RegisterScenePrefabs()
    {
        objectId[] sceneObjects = FindObjectsByType<objectId>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (objectId sceneObject in sceneObjects)
        {
            if (sceneObject != null)
                RegisterObject(sceneObject.gameObject);
        }
    }

    /// <summary>Spawn edilen birden fazla objeyi sallama sistemine ekler.</summary>
    public void RegisterObjects(GameObject[] roots)
    {
        if (roots == null)
            return;

        foreach (GameObject root in roots)
        {
            RegisterObject(root);
        }
    }

    private bool TryReadAcceleration(out Vector3 acceleration)
    {
        acceleration = Vector3.zero;

        if (Accelerometer.current == null)
            return false;

        acceleration = Accelerometer.current.acceleration.ReadValue();
        return true;
    }

    private void ApplyShake(Vector3 shakeDirection, float shakeStrength)
    {
        RemoveMissingTargets();

        Vector3 planarDirection = new Vector3(shakeDirection.x, 0f, shakeDirection.y);
        if (planarDirection.sqrMagnitude < 0.001f)
            planarDirection = Vector3.right;

        planarDirection.y = 0f;
        planarDirection.Normalize();

        float normalizedStrength = Mathf.InverseLerp(shakeThreshold, maxShakeStrength, shakeStrength);
        float sideForce = Mathf.Min(sideImpulse * normalizedStrength, maxSideImpulse);
        Vector3 impulse = planarDirection * sideForce + Vector3.up * (upwardImpulse * normalizedStrength);

        Vector3 torqueAxis = Vector3.Cross(Vector3.up, planarDirection).normalized;
        Vector3 torque = torqueAxis * torqueImpulse * normalizedStrength;

        foreach (PrefabShake target in targets)
        {
            if (target == null)
                continue;

            target.ApplyPhoneShake(impulse, torque, affectHeldObjects);
        }
    }

    private void RemoveMissingTargets()
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null)
                targets.RemoveAt(i);
        }
    }

    private void ResetAccelerationSamples()
    {
        smoothedAcceleration = Vector3.zero;
        previousDynamicAcceleration = Vector3.zero;
        hasAccelerationSample = false;
        nextAllowedShakeTime = 0f;
    }
}
