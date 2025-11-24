using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    // Enemy states behavior
    public EnemyState enemyCurrentState;

    // Enemy Template
    public EnemyType enemyType;

    // Enemy Stats
    private float health = 0;
    private float moveSpeed = 0;
    private float expAmount = 0;


    // is a Range Enemy
    public bool isRangeEnemy = false;

    // Enemy Detection
    public float DetectionRange { get; private set; }
    public float AttackMeleeRange { get; private set; }
    public float AttackDistanceRange { get; private set; }

    private bool isAttacking = false;
    private Coroutine attackCoroutine;


    private void Start()
    {
        // always start in idle state
        enemyCurrentState = EnemyState.Idle;
        SetEnemyStats();
        SetEnemyDetection();
    }

    private void SetEnemyStats()
    {
        health = enemyType.maxHealth;
        moveSpeed = enemyType.moveSpeed;
        expAmount = enemyType.expAmount;
    }

    private void SetEnemyDetection()
    {
        DetectionRange = enemyType.detectionRange;
        AttackMeleeRange = enemyType.attackMeleeRange;
        AttackDistanceRange = enemyType.attackDistanceRange;
    }

    public void SetInZoneState()
    {
        if (enemyCurrentState == EnemyState.Moving) return;

        int initialState = Random.Range(0, 2);
        enemyCurrentState = initialState == 0 ? EnemyState.Idle : EnemyState.Moving;
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"aww shiet! {amount} Damage");
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public void MakeDamage()
    {
        Debug.Log("Zoombie Bite!");
    }


    private void Die()
    {
        gameObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackMeleeRange);
        Gizmos.color = Color.yellow;
        if (isRangeEnemy) Gizmos.DrawWireSphere(transform.position, AttackDistanceRange);
    }
}