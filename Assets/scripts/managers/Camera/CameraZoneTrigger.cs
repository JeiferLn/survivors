using Unity.Cinemachine;
using UnityEngine;

public class CameraZoneTrigger : MonoBehaviour
{
    // ------------- REFERENCES -------------
    [Header("Cinemachine Camera")]
    [SerializeField]
    private CinemachineCamera zoneCamera;

    // ------------- TARGET GROUP -------------
    [Header("Target Group")]
    [SerializeField]
    private CinemachineTargetGroup targetGroup;

    [SerializeField]
    private Transform[] targets;

    // ------------- PRIORITIES -------------
    [Header("Priorities")]
    [SerializeField]
    private int activePriority = 3;

    [SerializeField]
    private int defaultPriority = 1;

    // ------------- TIMING -------------
    [Header("Timing")]
    [SerializeField]
    private float revertDelay = 3f;

    // ------------- PLAYER -------------
    [Header("Player")]
    [SerializeField]
    private PlayerController player;

    // ------------- ON TRIGGER ENTER -------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];

            foreach (Transform t in targets)
            {
                targetGroup.AddMember(t, 1f, 0f);
            }

            zoneCamera.Priority = activePriority;

            player.canMove = false;

            StopAllCoroutines();
            StartCoroutine(RevertPriority());
        }
    }

    private System.Collections.IEnumerator RevertPriority()
    {
        yield return new WaitForSeconds(revertDelay);

        zoneCamera.Priority = defaultPriority;

        yield return new WaitForSeconds(1.25f);

        player.canMove = true;

        Destroy(gameObject);
    }
}
