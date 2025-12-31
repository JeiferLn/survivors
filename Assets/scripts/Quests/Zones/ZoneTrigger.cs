using System.Collections;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField]
    private ZoneId zoneId;

    private bool playerInside;

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
        StartCoroutine(Tick());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        // Resetear SOLO el progreso de misiones tipo Countdown cuando el jugador sale de la zona.
        // Otros tipos de misiones (ReachZone, CollectItem, etc.) mantienen su progreso.
        if (zoneId != null && QuestManager.Instance != null)
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
