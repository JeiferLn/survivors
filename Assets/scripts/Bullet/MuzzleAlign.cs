using UnityEngine;

public class MuzzleFlipZ : MonoBehaviour
{
    private Transform weapon;
    private Camera cam;

    public void Init(Transform weaponTransform, Camera camera)
    {
        weapon = weaponTransform;
        cam = camera;
    }

    void LateUpdate()
    {
        if (weapon == null || cam == null)
            return;

        Vector3 toCam = cam.transform.position - transform.position;
        float dot = Vector3.Dot(transform.forward, toCam);

        Vector3 localScale = transform.localScale;
        localScale.z = dot < 0f ? Mathf.Abs(localScale.z) : -Mathf.Abs(localScale.z);
        transform.localScale = localScale;
    }
}
