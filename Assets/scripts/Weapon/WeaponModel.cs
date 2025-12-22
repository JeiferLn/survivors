using UnityEngine;

public class WeaponModel : MonoBehaviour
{
    public Transform leftHandGrip;
    public Transform muzzle;

    [Tooltip("Dirección local del disparo")]
    public Vector3 fireDirection = Vector3.forward;

    public Vector3 GetFireDirection()
    {
        return muzzle.TransformDirection(fireDirection).normalized;
    }
}
