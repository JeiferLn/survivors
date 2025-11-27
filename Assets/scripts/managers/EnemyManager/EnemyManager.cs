using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Pool Enemy Reference")]
    [SerializeField] private Transform poolEnemyContainer;

    [Header("ActivationZone Reference")] 
    [SerializeField] private EnemyActivationZone activationZone;

    [Header("Targets")] 
    [SerializeField] private Transform mainTarget;
    // [SerializeField] private Transform secondaryTarget; 

    private List<Enemy> activeEnemies = new List<Enemy>();

    private void OnEnable() => activationZone.OnEnemyActivated += GetActiveEnemies;
    private void OnDisable() => activationZone.OnEnemyActivated -= GetActiveEnemies;

    private void Start()
    {
        GetActiveEnemies();
    }

    private void GetActiveEnemies()
    {
        activeEnemies.Clear(); // Limpiamos la lista existente antes de buscar
        foreach (Transform child in poolEnemyContainer)
        {
            if (child.TryGetComponent(out Enemy enemy))
            {
                // Solo agregamos si está activo, o reiniciamos su lógica
                if(child.gameObject.activeSelf) activeEnemies.Add(enemy);
            }
        }
    }

    private void Update()
    {
        if (activeEnemies.Count == 0 || mainTarget == null) return;

        foreach (Enemy enemy in activeEnemies)
        {
            if (!enemy.gameObject.activeInHierarchy) continue; // Si murió, saltar

            // 1. Calcular Distancia REAL en 3D
            float distanceToTarget = Vector3.Distance(enemy.transform.position, mainTarget.position);

            // MAQUINA DE ESTADOS
            switch (enemy.enemyCurrentState)
            {
                case EnemyState.Idle:
                    // Si el jugador entra en rango de detección -> Moverse
                    if (distanceToTarget < enemy.DetectionRange)
                    {
                        enemy.enemyCurrentState = EnemyState.Moving;
                    }
                    break;

                case EnemyState.Moving:
                    // Chequear rangos de ataque
                    float attackRange = enemy.isRangeEnemy ? enemy.AttackDistanceRange : enemy.AttackMeleeRange;
                    
                    if (distanceToTarget <= attackRange)
                    {
                        // Cambiar a estado de ataque
                        enemy.enemyCurrentState = enemy.isRangeEnemy ? EnemyState.AttackingDistance : EnemyState.AttackingMelee;
                        enemy.StopMoving(); // Detener el NavMesh
                    }
                    else
                    {
                        // Seguir moviéndose
                        EnemyMove(enemy);
                    }
                    break;

                case EnemyState.AttackingMelee:
                case EnemyState.AttackingDistance:
                    // Lógica: Si el jugador se aleja, volver a perseguir
                    float exitAttackRange = enemy.isRangeEnemy ? enemy.AttackDistanceRange : enemy.AttackMeleeRange;
                    
                    // Le damos un pequeño margen (offset) para que no parpadee entre atacar y moverse
                    if (distanceToTarget > exitAttackRange + 0.5f) 
                    {
                        enemy.enemyCurrentState = EnemyState.Moving;
                    }
                    else
                    {
                        // Aquí ordenamos al enemigo que ejecute su lógica de ataque
                        // El Manager decide QUE atacar, el Enemy decide COMO atacar
                        enemy.TryAttack();
                    }
                    break;
            }
        }
    }

    private void EnemyMove(Enemy enemy)
    {
        // Pasamos el destino. El script del enemigo decidirá si necesita actualizar el NavMesh
        enemy.MoveTo(mainTarget.position);
    }
}