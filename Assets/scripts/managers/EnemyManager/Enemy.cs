
using UnityEngine;
using UnityEngine.AI;


public class Enemy : MonoBehaviour
{
    [Header("Configuration")]
    public EnemyState enemyCurrentState;
    public EnemyType enemyType;
    public EnemyAttackData enemyAttackData;
    public bool isRangeEnemy = false;

    // Enemy Stats
    private float health;
    
    // Detection & Combat
    public float DetectionRange { get; private set; }
    public float AttackMeleeRange { get; private set; }
    public float AttackDistanceRange { get; private set; }

    private NavMeshAgent agent;
    
    // Control de Ataque
    private float lastAttackTime;
    private float attackCooldown = 1.5f; // Tiempo entre ataques (puedes ponerlo en el ScriptableObject)
    private float attackMeleeDamage;
    private float attackDistanceDamage;
    
    
    private void Start()
    {
        enemyCurrentState = EnemyState.Idle;
        
        agent = GetComponent<NavMeshAgent>();
        if(agent != null && enemyType != null)
        {
            agent.speed = enemyType.moveSpeed;
            SetEnemyStats();
            SetEnemyDetection();
        }
    }

    private void SetEnemyStats()
    {
        health = enemyType.maxHealth;
        attackCooldown = enemyAttackData.cooldown;
        attackMeleeDamage = enemyAttackData.damageMelee;
        attackDistanceDamage = enemyAttackData.damageRange; 
    }

    private void SetEnemyDetection()
    {
        DetectionRange = enemyType.detectionRange;
        AttackMeleeRange = enemyType.attackMeleeRange;
        AttackDistanceRange = enemyType.attackDistanceRange;
        
        // Ajustar el Stopping Distance del agente para que no empuje al jugador
        agent.stoppingDistance = isRangeEnemy ? AttackDistanceRange * 0.8f : AttackMeleeRange * 0.8f;
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (agent == null) return;
        
        agent.isStopped = false;
        // Pequeña optimización: Solo setear destino si está lejos del actual para no saturar el NavMesh
        if (Vector3.Distance(agent.destination, targetPosition) > 0.5f)
        {
            agent.SetDestination(targetPosition);
        }
    }

    public void StopMoving()
    {
        if (agent == null) return;
        agent.isStopped = true;
        // Resetear el path para asegurar que se detenga
        agent.ResetPath(); 
    }

    // Este método es llamado constantemente por el Manager cuando estamos en estado de ataque
    public void TryAttack()
    {
        // Rotar hacia el objetivo (opcional pero recomendado)
        // transform.LookAt... (lógica de rotación)

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // Ejecutar ataque
            if (isRangeEnemy)
            {
                // Lógica de disparo
                Debug.Log("Enemy Shoots Projectile!"); 
            }
            else
            {
                MakeDamage(); // Melee
            }

            lastAttackTime = Time.time;
        }
    }
    
    public void SetInZoneState()
    {
        if (enemyCurrentState == EnemyState.Moving) return;

        int initialState = Random.Range(0, 2);
        enemyCurrentState = initialState == 0 ? EnemyState.Idle : EnemyState.Moving;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void MakeDamage()
    {
        // Aquí iría la lógica de animación y daño real al jugador
        Debug.Log("Zombie Bite Animation & Damage!");
    }

    private void Die()
    {
        gameObject.SetActive(false);
        // Notificar al manager o pool si es necesario
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);;
        Gizmos.DrawSphere(transform.position, DetectionRange);
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);;
        Gizmos.DrawSphere(transform.position, AttackMeleeRange);

        if (!isRangeEnemy) return;
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.3f);
        Gizmos.DrawSphere(transform.position, AttackDistanceRange);
    }
}