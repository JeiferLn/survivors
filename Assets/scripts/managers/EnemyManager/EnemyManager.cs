using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla la lógica de todos los enemigos activos en escena.
/// Actúa como el "cerebro" de la máquina de estados.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    #region ══════════ SERIALIZED FIELDS ══════════

    [Header("Pool & Containers")]
    [SerializeField]
    private Transform poolEnemyContainer;

    [Header("Activation Zone")]
    [SerializeField]
    private EnemyActivationZone activationZone;

    [Header("Target")]
    [SerializeField]
    private Transform mainTarget;

    [Header("Settings")]
    [SerializeField]
    private float exitAttackRangeOffset = 0.5f;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugLogs = false;

    #endregion

    #region ══════════ CAMPOS PRIVADOS ══════════

    private readonly List<Enemy> activeEnemies = new();
    private readonly List<Enemy> enemiesToRemove = new();

    #endregion

    #region ══════════ PROPIEDADES ══════════

    public int ActiveEnemyCount => activeEnemies.Count;
    public IReadOnlyList<Enemy> ActiveEnemies => activeEnemies;

    #endregion

    #region ══════════ UNITY LIFECYCLE ══════════

    private void OnEnable()
    {
        if (activationZone != null)
            activationZone.OnEnemyActivated += RefreshActiveEnemies;
    }

    private void OnDisable()
    {
        if (activationZone != null)
            activationZone.OnEnemyActivated -= RefreshActiveEnemies;
    }

    private void Start()
    {
        RefreshActiveEnemies();
    }

    private void Update()
    {
        if (activeEnemies.Count == 0 || mainTarget == null) return;

        ProcessEnemies();
        CleanupDeadEnemies();
    }

    #endregion

    #region ══════════ GESTIÓN DE ENEMIGOS ══════════

    /// <summary>
    /// Refresca la lista de enemigos activos desde el pool.
    /// </summary>
    public void RefreshActiveEnemies()
    {
        activeEnemies.Clear();

        if (poolEnemyContainer == null) return;

        foreach (Transform child in poolEnemyContainer)
        {
            if (child.gameObject.activeInHierarchy &&
                child.TryGetComponent(out Enemy enemy) &&
                !enemy.IsDead)
            {
                activeEnemies.Add(enemy);
            }
        }

        LogDebug($"Enemigos activos: {activeEnemies.Count}");
    }

    /// <summary>
    /// Procesa la lógica de cada enemigo activo.
    /// </summary>
    private void ProcessEnemies()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if (!IsEnemyValid(enemy)) continue;

            float distanceToTarget = Vector3.Distance(
                enemy.transform.position,
                mainTarget.position
            );

            ProcessEnemyState(enemy, distanceToTarget);
        }
    }

    /// <summary>
    /// Verifica si un enemigo es válido para procesar.
    /// </summary>
    private bool IsEnemyValid(Enemy enemy)
    {
        if (enemy == null) return false;
        if (!enemy.gameObject.activeInHierarchy) return false;
        if (enemy.IsDead) return false;

        return true;
    }

    /// <summary>
    /// Limpia enemigos muertos o desactivados de la lista.
    /// </summary>
    private void CleanupDeadEnemies()
    {
        enemiesToRemove.Clear();

        foreach (Enemy enemy in activeEnemies)
        {
            if (!IsEnemyValid(enemy))
            {
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (Enemy enemy in enemiesToRemove)
        {
            activeEnemies.Remove(enemy);
        }
    }

    #endregion

    #region ══════════ MÁQUINA DE ESTADOS ══════════

    /// <summary>
    /// Procesa el estado actual de un enemigo y decide transiciones.
    /// </summary>
    private void ProcessEnemyState(Enemy enemy, float distanceToTarget)
    {
        switch (enemy.CurrentState)
        {
            case EnemyState.Idle:
                ProcessIdleState(enemy, distanceToTarget);
                break;

            case EnemyState.Moving:
                ProcessMovingState(enemy, distanceToTarget);
                break;

            case EnemyState.AttackingMelee:
            case EnemyState.AttackingDistance:
                ProcessAttackingState(enemy, distanceToTarget);
                break;

            case EnemyState.TakingDamage:
                ProcessDamageState(enemy);
                break;

            case EnemyState.Dead:
                // No hacer nada, ya está muerto
                break;

            case EnemyState.None:
                // Estado inválido, poner en Idle
                enemy.CurrentState = EnemyState.Idle;
                break;
        }
    }

    private void ProcessIdleState(Enemy enemy, float distance)
    {
        // Si el jugador entra en rango de detección → Moverse
        if (distance <= enemy.DetectionRange)
        {
            enemy.CurrentState = EnemyState.Moving;
            LogDebug($"{enemy.name}: Jugador detectado, persiguiendo...");
        }
    }

    private void ProcessMovingState(Enemy enemy, float distance)
    {
        float attackRange = enemy.GetCurrentAttackRange();

        if (distance <= attackRange)
        {
            // En rango de ataque
            enemy.StopMoving();
            enemy.CurrentState = enemy.IsRangeEnemy
                ? EnemyState.AttackingDistance
                : EnemyState.AttackingMelee;

            LogDebug($"{enemy.name}: En rango, atacando...");
        }
        else if (distance <= enemy.DetectionRange)
        {
            // Perseguir al jugador

            enemy.MoveTo(mainTarget);
        }
        else
        {
            // Jugador fuera de rango de detección
            enemy.StopMoving();
            enemy.CurrentState = EnemyState.Idle;
            LogDebug($"{enemy.name}: Objetivo perdido, volviendo a Idle");
        }
    }

    private void ProcessAttackingState(Enemy enemy, float distance)
    {
        if (enemy.isAttacking) return;

        float attackRange = enemy.GetCurrentAttackRange();
        float exitRange = attackRange + exitAttackRangeOffset;

        if (distance > exitRange)
        {
            // Jugador se alejó, volver a perseguir
            enemy.CurrentState = EnemyState.Moving;
            LogDebug($"{enemy.name}: Objetivo fuera de rango, persiguiendo...");
        }
        else
        {
            // Ejecutar ataque
            if (!enemy.isAttacking)
            {
                enemy.TryAttack();
            }
        }
    }

    private void ProcessDamageState(Enemy enemy)
    {
        // Verificar si la animación de daño terminó
        if (enemy.Animator != null && enemy.Animator.IsCurrentAnimationFinished(0.9f))
        {
            // Volver a estado de combate o idle
            float distance = Vector3.Distance(enemy.transform.position, mainTarget.position);

            if (distance <= enemy.GetCurrentAttackRange())
            {
                enemy.CurrentState = enemy.IsRangeEnemy
                    ? EnemyState.AttackingDistance
                    : EnemyState.AttackingMelee;
            }
            else if (distance <= enemy.DetectionRange)
            {
                enemy.CurrentState = EnemyState.Moving;
            }
            else
            {
                enemy.CurrentState = EnemyState.Idle;
            }
        }
    }

    #endregion

    #region ══════════ API PÚBLICA ══════════

    /// <summary>
    /// Cambia el objetivo principal de todos los enemigos.
    /// </summary>
    public void SetMainTarget(Transform newTarget)
    {
        mainTarget = newTarget;
    }

    /// <summary>
    /// Aplica daño a todos los enemigos en un área.
    /// </summary>
    public void DamageEnemiesInArea(Vector3 center, float radius, float damage)
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if (!IsEnemyValid(enemy)) continue;

            float distance = Vector3.Distance(enemy.transform.position, center);
            if (distance <= radius)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    /// <summary>
    /// Obtiene el enemigo más cercano al punto especificado.
    /// </summary>
    public Enemy GetClosestEnemy(Vector3 position)
    {
        Enemy closest = null;
        float closestDistance = float.MaxValue;

        foreach (Enemy enemy in activeEnemies)
        {
            if (!IsEnemyValid(enemy)) continue;

            float distance = Vector3.Distance(enemy.transform.position, position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    #endregion

    #region ══════════ DEBUG ══════════

    private void LogDebug(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[EnemyManager] {message}");
    }

    [ContextMenu("Refresh Enemies")]
    private void DebugRefreshEnemies()
    {
        RefreshActiveEnemies();
    }

    [ContextMenu("Log Active Enemies")]
    private void DebugLogEnemies()
    {
        Debug.Log($"=== ENEMIGOS ACTIVOS ({activeEnemies.Count}) ===");
        foreach (Enemy enemy in activeEnemies)
        {
            Debug.Log($"  - {enemy.name}: {enemy.CurrentState} | HP: {enemy.CurrentHealth}");
        }
    }

    #endregion
}