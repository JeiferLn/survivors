using UnityEngine;

public class WeaponModel : MonoBehaviour
{
    [Header("Weapon ID")]
    public WeaponId weaponId;

    [Header("References")]
    public Transform leftHandGrip;
    public Transform muzzle;

    [Header("Configuration")]
    [SerializeField]
    private Vector3 fireDirection = Vector3.forward;

    [SerializeField]
    private Vector3 muzzleOffset;

    [Header("Bullet")]
    [SerializeField]
    private Vector3 bulletOffset;

    public Vector3 GetMuzzlePosition()
    {
        if (muzzle == null)
            return transform.position;

        return muzzle.position + muzzle.TransformDirection(muzzleOffset);
    }

    public Vector3 GetFireDirection()
    {
        if (muzzle == null)
            return transform.forward;

        return muzzle.TransformDirection(fireDirection).normalized;
    }

    public Vector3 GetBulletPositionCorrected()
    {
        return GetMuzzlePosition() + bulletOffset;
    }
}
