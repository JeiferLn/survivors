using UnityEngine;

public class MuzzleFlipZ : MonoBehaviour
{
    [SerializeField]
    private Transform weapon;

    [SerializeField]
    private Camera cam;

    void LateUpdate()
    {
        if (weapon == null || cam == null)
            return;

        Vector3 toCam = cam.transform.position - transform.position;

        float dot = Vector3.Dot(transform.forward, toCam);

        Vector3 localScale = transform.localScale;

        if (dot < 0f)
            localScale.z = Mathf.Abs(localScale.z);
        else
            localScale.z = -Mathf.Abs(localScale.z);

        transform.localScale = localScale;
    }
}
