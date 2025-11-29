using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Configuration")] public EnemyState enemyCurrentState;
    public EnemyType enemyType = null;
    public EnemyAttackData enemyAttackData = null;
    public bool isRangeEnemy = false;

    [ShowIf("isRangeEnemy")]

    // Enemy Stats
    private float health;

    // Target range
    private Transform mainTarget;

    // Detection & Combat
    public float DetectionRange { get; private set; }
    public float AttackMeleeRange { get; private set; }
    public float AttackDistanceRange { get; private set; }

    private NavMeshAgent agent;

    // Control de Ataque
    private float lastAttackTime;
    private float attackCooldown;
    private float attackMeleeDamage;
    private float attackDistanceDamage;


    private void Start()
    {
        enemyCurrentState = EnemyState.Idle;
        agent = GetComponent<NavMeshAgent>();

        if (agent == null || enemyType == null) return;

        agent.speed = enemyType.moveSpeed;
        SetEnemyStats();
        SetEnemyDetection();
    }

    private void SetEnemyStats()
    {
        health = enemyType.maxHealth;
        isRangeEnemy = enemyAttackData.isRanged;
        attackCooldown = enemyAttackData.cooldown;
        attackMeleeDamage = enemyAttackData.damageMelee;
        attackDistanceDamage = enemyAttackData.damageRange;
    }

    private void SetEnemyDetection()
    {
        DetectionRange = enemyType.detectionRange;
        AttackMeleeRange = enemyType.attackMeleeRange;
        AttackDistanceRange = enemyType.attackDistanceRange;

        agent.stoppingDistance = isRangeEnemy ? AttackDistanceRange * 0.8f : AttackMeleeRange * 0.8f;
    }

    public void MoveTo(Transform target)
    {
        if (agent is null || target is null) return;

        mainTarget = target;

        agent.isStopped = false;
        // Solo setear destino si está lejos del actual para no saturar el NavMesh
        if (Vector3.Distance(agent.destination, mainTarget.position) > 0.5f)
        {
            agent.SetDestination(mainTarget.position);
        }
    }
    
    private void RotateTowardsTarget()
    {
        if (mainTarget is null) return;

        Vector3 direction = (mainTarget.position - transform.position).normalized;
        direction.y = 0; // evitar inclinaciones

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    public void StopMoving()
    {
        if (agent is null) return;
        agent.isStopped = true;
        // Resetear el path para asegurar que se detenga
        agent.ResetPath();
    }

    public void TryAttack()
    {
        // 1) Respetar cooldown
        if (Time.time < lastAttackTime + attackCooldown) return;

        // ---- MELEE ----
        if (!isRangeEnemy)
        {
            RotateTowardsTarget();
            MakeDamage();
            lastAttackTime = Time.time;
            return;
        }

        // ---- RANGED ----
        if (mainTarget is null) return;

        float distanceToTarget = Vector3.Distance(transform.position, mainTarget.position);

        // 2) Si NO está en rango → NO raycast
        if (distanceToTarget > AttackDistanceRange)
        {
            MoveTo(mainTarget);
            return;
        }
        
        // Rotamos hacia el objetivo
        RotateTowardsTarget();

        // 3) Está en rango → ahora sí hacemos el ÚNICO raycast
        if (!HasLineOfSightOptimized())
        {
            // En rango pero bloqueado → mover para encontrar ángulo
            MoveTo(mainTarget);
            return;
        }

        // 4) En rango + sin obstáculos → disparar
        MakeDamage();
        lastAttackTime = Time.time;
    }


    private bool HasLineOfSightOptimized()
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 toTarget = (mainTarget.position - transform.position);

        // Opción: evitar raycast si el ángulo es muy malo
        float angle = Vector3.Angle(transform.forward, toTarget);
        if (angle > 70f)
            return false;

        Vector3 direction = toTarget.normalized;

        // Raycast ÚNICO
        if (Physics.Raycast(origin, direction, out RaycastHit hit, AttackDistanceRange))
        {
            if (hit.transform == mainTarget || hit.transform.IsChildOf(mainTarget))
            {
                return true;
            }
        }

        return false;
    }


    private void TryRangeAttack()
    {
        if (mainTarget is null) return;

        Vector3 direction = (mainTarget.position - transform.position).normalized;
        float distance = AttackDistanceRange;

        // DEBUG ray (solo en editor)
        Debug.DrawRay(transform.position + Vector3.up, direction * distance, Color.yellow);

        // Lanzamos raycast
        if (Physics.Raycast(transform.position + Vector3.up, direction, out RaycastHit hit, distance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                Debug.Log("Ranged Hit → MakeDamage()");
                MakeDamage(); // daño real
            }
            else
            {
                Debug.Log("Ranged attack blocked by: " + hit.transform.name);
            }
        }
        else
        {
            Debug.Log("Raycast no tocó nada. No dispara.");
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
        // Esferas existentes
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, DetectionRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, AttackMeleeRange);

        if (isRangeEnemy)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.3f);
            Gizmos.DrawSphere(transform.position, AttackDistanceRange);

            // Dibujar rayo hacia rangeTarget
            if (mainTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(
                    transform.position + Vector3.up,
                    mainTarget.position + Vector3.up
                );
            }
        }
    }
}