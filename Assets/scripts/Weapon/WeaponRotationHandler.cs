using System;
using UnityEngine;

public class WeaponRotationHandler : MonoBehaviour
{
    [Serializable]
    public class WeaponStateOffset
    {
        public string stateName;
        public Vector3 rotationOffset;
    }

    [Header("Referencias")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Transform weaponTransform;

    [Header("Offsets por Estado")]
    [SerializeField]
    private WeaponStateOffset idleState = new WeaponStateOffset
    {
        stateName = "Idle",
        rotationOffset = new Vector3(15f, 0f, 0f),
    };

    [SerializeField]
    private WeaponStateOffset walkState = new WeaponStateOffset
    {
        stateName = "Walk",
        rotationOffset = new Vector3(0f, 0f, 0f),
    };

    [SerializeField]
    private WeaponStateOffset runState = new WeaponStateOffset
    {
        stateName = "Run",
        rotationOffset = new Vector3(-5f, 0f, 0f),
    };

    [Header("Parámetro del Animator")]
    [SerializeField]
    private string speedParameter = "Speed";

    [Header("Umbrales de Velocidad")]
    [SerializeField]
    private float idleThreshold = 0.1f;

    [SerializeField]
    private float walkThreshold = 0.5f;

    [SerializeField]
    private float runThreshold = 1f;

    [Header("Suavizado")]
    [SerializeField]
    private float smoothSpeed = 10f;

    private Quaternion baseLocalRotation;
    private Vector3 currentRotationOffset;

    private void Start()
    {
        if (weaponTransform == null)
        {
            Debug.LogError("WeaponRotationHandler: No se asignó weaponTransform!");
            enabled = false;
            return;
        }

        baseLocalRotation = weaponTransform.localRotation;
        currentRotationOffset = idleState.rotationOffset;
    }

    private void LateUpdate()
    {
        Vector3 targetOffset = CalculateTargetOffset();

        currentRotationOffset = Vector3.Lerp(
            currentRotationOffset,
            targetOffset,
            Time.deltaTime * smoothSpeed
        );

        Quaternion offsetRotation = Quaternion.Euler(currentRotationOffset);
        weaponTransform.localRotation = baseLocalRotation * offsetRotation;
    }

    private Vector3 CalculateTargetOffset()
    {
        float speed = animator.GetFloat(speedParameter);

        if (speed < idleThreshold)
        {
            return idleState.rotationOffset;
        }
        else if (speed < walkThreshold)
        {
            float t = Mathf.InverseLerp(idleThreshold, walkThreshold, speed);
            return Vector3.Lerp(idleState.rotationOffset, walkState.rotationOffset, t);
        }
        else if (speed < runThreshold)
        {
            float t = Mathf.InverseLerp(walkThreshold, runThreshold, speed);
            return Vector3.Lerp(walkState.rotationOffset, runState.rotationOffset, t);
        }
        else
        {
            return runState.rotationOffset;
        }
    }
}
