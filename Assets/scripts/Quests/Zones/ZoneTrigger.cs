using System.Collections;
using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    private bool playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        playerInside = true;
        StartCoroutine(Tick());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        playerInside = false;
    }

    private IEnumerator Tick()
    {
        while (playerInside)
        {
            QuestManager.Instance.DispatchEvent(QuestType.Countdown, Time.deltaTime);

            yield return null;
        }
    }
}
