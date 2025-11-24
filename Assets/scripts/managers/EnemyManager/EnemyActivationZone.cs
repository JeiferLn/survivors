using System;
using UnityEngine;

public class EnemyActivationZone : MonoBehaviour
{
    
    public Action OnEnemyActivated = delegate { };
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent(out EnemyActivation enemy))
        {
            OnEnemyActivated?.Invoke();
            enemy.SetVisible(true);
        }
    }

    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent(out EnemyActivation enemy))
        {
            enemy.SetVisible(false);
        }
    }
}