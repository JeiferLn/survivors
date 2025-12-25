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
    
    [Header("NavMesh Settings")]
    [SerializeField] private float brakingAcceleration = 100f; // Frenado brusco
    [SerializeField] private float arrivalThreshold = 0.1f;    // Distancia para considerar "llegó"
    
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
    
    // Cache para optimización
    private float originalAcceleration;
    
    #endregion

    #region ══════════ NOMBRES DE ANIMACIONES ══════════
    
    public static class Animations
    {
        public const string Idle = "Z-Idle";
        public const string Walk = "Z-Walk";
        public const string AttackMelee = "Z-MeleeAttack";
        public const string AttackRange = "Z-RangeAttack";
        public const string Damage = "Z-GetDamage";
        public const string Die = "Z-Dead";
    }
    
    #endregion

    #region ══════════ UNITY LIFECYCLE ══════════

    private void Awake()
    {
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
        ResetEnemy();
    }

    private void Update()
    {
        // ══════════ VERIFICACIÓN DE LLEGADA ══════════
        // Esto previene el deslizamiento al verificar constantemente
        if (agent != null && agent.enabled && !agent.isStopped)
        {
            CheckArrival();
        }
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
        
        // Guardar aceleración original
        originalAcceleration = agent.acceleration;
        
        // ══════════ CONFIGURACIÓN ANTI-PATINAJE ══════════
        agent.speed = enemyType.moveSpeed;
        agent.acceleration = brakingAcceleration;      // Aceleración alta = frenado rápido
        agent.autoBraking = true;                       // Auto-frenar al llegar
        agent.stoppingDistance = IsRangeEnemy 
            ? AttackDistanceRange * 0.8f 
            : AttackMeleeRange * 0.8f;
        
        // Estos valores ayudan a un movimiento más preciso
        agent.angularSpeed = 360f;                      // Rotación rápida
    }

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
            agent.velocity = Vector3.zero; // ← IMPORTANTE: Resetear velocidad
        }
        
        if (enemyAnimator != null)
        {
            enemyAnimator.ResetAnimationTracking();
            enemyAnimator.SetAnimation(Animations.Idle);
        }
    }

    #endregion

    #region ══════════ ESTADO Y ANIMACIONES ══════════

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
                enemyAnimator.ForceAnimation(Animations.Damage, 0.05f);
                break;

            case EnemyState.Dead:
                enemyAnimator.SetAnimation(Animations.Die, 0.1f);
                break;
        }
    }

    #endregion

    #region ══════════ MOVIMIENTO ══════════

    /// <summary>
    /// Verifica si el agente llegó a su destino y lo detiene inmediatamente.
    /// </summary>
    private void CheckArrival()
    {
        if (mainTarget == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, mainTarget.position);
        float currentAttackRange = GetCurrentAttackRange();
        
        // Si está dentro del rango de ataque, detener completamente
        if (distanceToTarget <= currentAttackRange)
        {
            ForceStop();
        }
        // También verificar con el remainingDistance del agente
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + arrivalThreshold)
        {
            ForceStop();
        }
    }

    /// <summary>
    /// Fuerza la detención completa del agente sin deslizamiento.
    /// </summary>
    public void ForceStop()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        
        // ══════════ DETENCIÓN INMEDIATA ══════════
        agent.isStopped = true;
        agent.velocity = Vector3.zero;  // ← CLAVE: Elimina toda inercia
        agent.ResetPath();
        
        // Opcional: Warpar a la posición actual para evitar micro-deslizamientos
        // agent.Warp(transform.position);
    }

    public void MoveTo(Transform target)
    {
        if (agent == null || target == null || IsDead) return;
        if (!agent.isOnNavMesh) return;
        
        mainTarget = target;
        
        // Reactivar movimiento
        agent.isStopped = false;

        // Solo actualizar destino si cambió significativamente
        if (Vector3.Distance(agent.destination, target.position) > 0.5f)
        {
            agent.SetDestination(target.position);
        }
    }

    public void StopMoving()
    {
        ForceStop(); // ← Usar ForceStop en lugar del código anterior
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
    
    /// <summary>
    /// Verifica si el agente ha llegado a su destino.
    /// </summary>
    public bool HasReachedDestination()
    {
        if (agent == null || !agent.isOnNavMesh) return true;
        if (agent.pathPending) return false;
        
        return agent.remainingDistance <= agent.stoppingDistance + arrivalThreshold;
    }

    /// <summary>
    /// Verifica si está en rango de ataque del objetivo actual.
    /// </summary>
    public bool IsInAttackRange()
    {
        if (mainTarget == null) return false;
        
        float distance = Vector3.Distance(transform.position, mainTarget.position);
        return distance <= GetCurrentAttackRange();
    }

    #endregion

    #region ══════════ COMBATE ══════════

    public bool TryAttack()
    {
        if (IsDead) return false;
        
        if (Time.time < lastAttackTime + attackCooldown) 
            return false;

        // ══════════ ASEGURAR DETENCIÓN ANTES DE ATACAR ══════════
        ForceStop();
        RotateTowardsTarget();

        if (!IsRangeEnemy)
        {
            ExecuteMeleeAttack();
            lastAttackTime = Time.time;
            return true;
        }

        if (mainTarget == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, mainTarget.position);
        
        if (distanceToTarget > AttackDistanceRange)
        {
            return false;
        }

        if (!HasLineOfSight())
        {
            return false;
        }

        ExecuteRangedAttack();
        lastAttackTime = Time.time;
        return true;
    }

    private void ExecuteMeleeAttack()
    {
        Debug.Log($"[Enemy] {name}: Melee Attack! Damage: {attackMeleeDamage}");
    }

    private void ExecuteRangedAttack()
    {
        Debug.Log($"[Enemy] {name}: Ranged Attack! Damage: {attackDistanceDamage}");
    }

    private bool HasLineOfSight()
    {
        if (mainTarget == null) return false;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 toTarget = mainTarget.position - transform.position;

        float angle = Vector3.Angle(transform.forward, toTarget);
        if (angle > 70f) return false;

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
            if (CurrentState != EnemyState.Dead)
            {
                var stateBeforeDamage = CurrentState;
                CurrentState = EnemyState.TakingDamage;
            }
        }
    }

    private void Die()
    {
        CurrentState = EnemyState.Dead;
        ForceStop(); // ← Usar ForceStop
        
        Debug.Log($"[Enemy] {name}: Died!");
    }

    private void DeactivateEnemy()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region ══════════ UTILIDADES ══════════

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
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, DetectionRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, AttackMeleeRange);

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
        
        // Mostrar stopping distance
        Gizmos.color = Color.green;
        if (agent != null)
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
    }

    #endregion
}