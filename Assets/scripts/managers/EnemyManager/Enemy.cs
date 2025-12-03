using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    #region ══════════ SERIALIZED FIELDS ══════════
    
    [Header("Components")] 
    private EnemyAnimator enemyAnimator;
    private NavMeshAgent agent;
    
    [Header("Configuration")]
    [SerializeField] private EnemyType enemyType;
    
    [Header("State (Debug)")]
    [SerializeField] private EnemyState _currentState = EnemyState.None;
    [SerializeField] private EnemyState _previousState = EnemyState.None;
    
    #endregion

    #region ══════════ PROPIEDADES PÚBLICAS ══════════
    
    // Estado
    public EnemyState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;
            _previousState = _currentState;
            _currentState = value;
            OnStateChanged(_previousState, _currentState);
        }
    }
    public EnemyState PreviousState => _previousState;
    
    // Tipo de enemigo
    public bool IsRangeEnemy { get; private set; }
    public EnemyType Type => enemyType;
    
    // Detección y Combate
    public float DetectionRange { get; private set; }
    public float AttackMeleeRange { get; private set; }
    public float AttackDistanceRange { get; private set; }
    
    // Stats
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    
    // Componentes
    public EnemyAnimator Animator => enemyAnimator;
    
    #endregion

    #region ══════════ CAMPOS PRIVADOS ══════════
    
    private Transform mainTarget;
    private float lastAttackTime;
    private float attackCooldown;
    private float attackMeleeDamage;
    private float attackDistanceDamage;
    
    #endregion

    #region ══════════ NOMBRES DE ANIMACIONES ══════════
    
    // Centralizamos los nombres para evitar typos
    public static class Animations
    {
        public const string Idle = "root|Zombie_Idle";
        public const string Walk = "root|Zombie_Walk";
        public const string AttackMelee = "root|Zombie_Attack";
        public const string AttackRange = "root|Zombie_RangeAttack";
        public const string Damage = "root|Zombie_Damage";
        public const string Die = "root|Zombie_dead";
    }
    
    #endregion

    #region ══════════ UNITY LIFECYCLE ══════════

    private void Awake()
    {
        // Obtener componentes si no están asignados
        if (agent == null) 
            agent = GetComponent<NavMeshAgent>();
        
        if (enemyAnimator == null) 
            enemyAnimator = GetComponent<EnemyAnimator>();
    }

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        // Reiniciar cuando se reactiva desde el pool
        ResetEnemy();
    }

    #endregion

    #region ══════════ INICIALIZACIÓN ══════════

    private void Initialize()
    {
        if (enemyType == null)
        {
            Debug.LogError($"[Enemy] {name}: EnemyType no asignado!", this);
            return;
        }

        SetupStats();
        SetupDetection();
        SetupNavMesh();
    }

    private void SetupStats()
    {
        CurrentHealth = enemyType.maxHealth;

        switch (enemyType.attackMode)
        {
            case EnemyAttackMode.Melee:
                IsRangeEnemy = false;
                attackCooldown = enemyType.meleeAttack.cooldown;
                attackMeleeDamage = enemyType.meleeAttack.damage;
                break;

            case EnemyAttackMode.Ranged:
                IsRangeEnemy = true;
                attackCooldown = enemyType.rangeAttack.cooldown;
                attackDistanceDamage = enemyType.rangeAttack.damage;
                break;

            case EnemyAttackMode.Hybrid:
                IsRangeEnemy = true;
                attackMeleeDamage = enemyType.meleeAttack.damage;
                attackDistanceDamage = enemyType.rangeAttack.damage;
                attackCooldown = Mathf.Min(
                    enemyType.meleeAttack.cooldown, 
                    enemyType.rangeAttack.cooldown
                );
                break;
        }
    }

    private void SetupDetection()
    {
        DetectionRange = enemyType.detectionRange;
        AttackMeleeRange = enemyType.attackMeleeRange;
        AttackDistanceRange = enemyType.attackDistanceRange;
    }

    private void SetupNavMesh()
    {
        if (agent == null) return;
        
        agent.speed = enemyType.moveSpeed;
        agent.stoppingDistance = IsRangeEnemy 
            ? AttackDistanceRange * 0.8f 
            : AttackMeleeRange * 0.8f;
    }

    /// <summary>
    /// Reinicia el enemigo para reutilización en pool.
    /// </summary>
    public void ResetEnemy()
    {
        _currentState = EnemyState.Idle;
        _previousState = EnemyState.None;
        lastAttackTime = 0f;
        
        if (enemyType != null)
            CurrentHealth = enemyType.maxHealth;
        
        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
        
        if (enemyAnimator != null)
        {
            enemyAnimator.ResetAnimationTracking();
            enemyAnimator.SetAnimation(Animations.Idle);
        }
    }

    #endregion

    #region ══════════ ESTADO Y ANIMACIONES ══════════

    /// <summary>
    /// Callback cuando el estado cambia. Actualiza animaciones.
    /// </summary>
    private void OnStateChanged(EnemyState from, EnemyState to)
    {
        if (enemyAnimator == null) return;

        switch (to)
        {
            case EnemyState.Idle:
                enemyAnimator.SetAnimation(Animations.Idle);
                break;

            case EnemyState.Moving:
                enemyAnimator.SetAnimation(Animations.Walk);
                break;

            case EnemyState.AttackingMelee:
                enemyAnimator.SetAnimation(Animations.AttackMelee, 0.1f);
                break;

            case EnemyState.AttackingDistance:
                enemyAnimator.SetAnimation(Animations.AttackRange, 0.1f);
                break;

            case EnemyState.TakingDamage:
                // Forzamos para que se reproduzca aunque estemos en damage
                enemyAnimator.ForceAnimation(Animations.Damage, 0.05f);
                break;

            case EnemyState.Dead:
                enemyAnimator.SetAnimation(Animations.Die, 0.1f);
                break;
        }
    }

    #endregion

    #region ══════════ MOVIMIENTO ══════════

    public void MoveTo(Transform target)
    {
        if (agent == null || target == null || IsDead) return;
        
        mainTarget = target;
        agent.isStopped = false;

        // Solo actualizar destino si cambió significativamente
        if (Vector3.Distance(agent.destination, target.position) > 0.5f)
        {
            agent.SetDestination(target.position);
        }
    }

    public void StopMoving()
    {
        if (agent == null) return;
        
        agent.isStopped = true;
        agent.ResetPath();
    }

    public void RotateTowardsTarget()
    {
        if (mainTarget == null) return;

        Vector3 direction = (mainTarget.position - transform.position).normalized;
        direction.y = 0;

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    #endregion

    #region ══════════ COMBATE ══════════

    /// <summary>
    /// Intenta atacar. Retorna true si el ataque se ejecutó.
    /// </summary>
    public bool TryAttack()
    {
        if (IsDead) return false;
        
        // Respetar cooldown
        if (Time.time < lastAttackTime + attackCooldown) 
            return false;

        RotateTowardsTarget();

        // Ataque melee
        if (!IsRangeEnemy)
        {
            ExecuteMeleeAttack();
            lastAttackTime = Time.time;
            return true;
        }

        // Ataque a distancia - verificar línea de visión
        if (mainTarget == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, mainTarget.position);
        
        if (distanceToTarget > AttackDistanceRange)
        {
            // Fuera de rango, el manager debería moverlo
            return false;
        }

        if (!HasLineOfSight())
        {
            // Bloqueado, el manager debería moverlo
            return false;
        }

        ExecuteRangedAttack();
        lastAttackTime = Time.time;
        return true;
    }

    private void ExecuteMeleeAttack()
    {
        // La animación ya se setea en OnStateChanged
        Debug.Log($"[Enemy] {name}: Melee Attack! Damage: {attackMeleeDamage}");
        
        // TODO: Aplicar daño real al jugador
        // mainTarget.GetComponent<PlayerHealth>()?.TakeDamage(attackMeleeDamage);
    }

    private void ExecuteRangedAttack()
    {
        Debug.Log($"[Enemy] {name}: Ranged Attack! Damage: {attackDistanceDamage}");
        
        // TODO: Instanciar proyectil o aplicar daño directo
    }

    private bool HasLineOfSight()
    {
        if (mainTarget == null) return false;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 toTarget = mainTarget.position - transform.position;

        // Verificar ángulo
        float angle = Vector3.Angle(transform.forward, toTarget);
        if (angle > 70f) return false;

        // Raycast
        if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, AttackDistanceRange))
        {
            return hit.transform == mainTarget || hit.transform.IsChildOf(mainTarget);
        }

        return false;
    }

    #endregion

    #region ══════════ DAÑO Y MUERTE ══════════

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth -= amount;
        
        Debug.Log($"[Enemy] {name}: Took {amount} damage. Health: {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Solo cambiar a TakingDamage si no está ya muriendo
            if (CurrentState != EnemyState.Dead)
            {
                // Guardar estado para volver después
                var stateBeforeDamage = CurrentState;
                CurrentState = EnemyState.TakingDamage;
                
                // TODO: Opcionalmente, volver al estado anterior después de la animación
                // StartCoroutine(ReturnToStateAfterDelay(stateBeforeDamage, 0.5f));
            }
        }
    }

    private void Die()
    {
        CurrentState = EnemyState.Dead;
        StopMoving();
        
        Debug.Log($"[Enemy] {name}: Died!");
        
        // Desactivar después de un tiempo para que se vea la animación
        // Invoke(nameof(DeactivateEnemy), 3f);
    }

    private void DeactivateEnemy()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region ══════════ UTILIDADES ══════════

    /// <summary>
    /// Obtiene el rango de ataque actual según el tipo de enemigo.
    /// </summary>
    public float GetCurrentAttackRange()
    {
        return IsRangeEnemy ? AttackDistanceRange : AttackMeleeRange;
    }
    
    public void SetInZoneState()
    {
        if (_currentState == EnemyState.Moving) return;

        int initialState = Random.Range(0, 2);
        _currentState = initialState == 0 ? EnemyState.Idle : EnemyState.Moving;
    }

    /// <summary>
    /// Verifica si puede atacar (cooldown listo).
    /// </summary>
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    #endregion

    #region ══════════ DEBUG ══════════

    [ContextMenu("Test Take Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10f);
    }

    [ContextMenu("Test Die")]
    private void TestDie()
    {
        Die();
    }

    [ContextMenu("Test Reset")]
    private void TestReset()
    {
        ResetEnemy();
    }

    private void OnDrawGizmosSelected()
    {
        // Detección
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, DetectionRange);

        // Melee
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, AttackMeleeRange);

        // Rango
        if (IsRangeEnemy)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, AttackDistanceRange);

            if (mainTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position + Vector3.up, mainTarget.position + Vector3.up);
            }
        }
    }

    #endregion
}