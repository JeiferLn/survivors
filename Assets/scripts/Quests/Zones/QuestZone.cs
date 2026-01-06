using System.Collections;
using UnityEngine;

public class QuestZone : MonoBehaviour
{
    [Header("Zone Configuration")]
    [SerializeField]
    private ZoneId zoneId;

    [Header("Quest Event Types")]
    [SerializeField]
    private bool triggerReachZone = false;

    [SerializeField]
    private bool triggerCountdown = false;

    private bool playerInside;
    private bool reachZoneTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (zoneId == null)
        {
            Debug.LogError(
                $"[ZoneTrigger] ZoneId no está asignado en el GameObject '{gameObject.name}'. Por favor, asigna un ZoneId en el Inspector."
            );
            return;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogError(
                "[ZoneTrigger] QuestManager.Instance es null. Asegúrate de que QuestManager esté inicializado."
            );
            return;
        }

        playerInside = true;

        if (triggerReachZone && !reachZoneTriggered)
        {
            QuestManager.Instance.DispatchEvent(QuestType.ReachZone, zoneId);
            reachZoneTriggered = true;
        }

        if (triggerCountdown)
        {
            StartCoroutine(Tick());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        reachZoneTriggered = false;

        if (triggerCountdown && zoneId != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetCountdownProgressForZone(zoneId);
        }
    }

    private IEnumerator Tick()
    {
        while (playerInside)
        {
            if (zoneId == null || QuestManager.Instance == null)
            {
                yield break;
            }

            var eventData = new CountdownEventData(zoneId, Time.deltaTime);
            QuestManager.Instance.DispatchEvent(QuestType.Countdown, eventData);
            yield return null;
        }
    }
}
