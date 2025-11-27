using UnityEngine;

public class MuzzleFlashRotation : MonoBehaviour
{
    public void Randomize()
    {
        float rot = Random.Range(0f, 360f);
        transform.localRotation = Quaternion.Euler(0, rot, 0);
    }
}
