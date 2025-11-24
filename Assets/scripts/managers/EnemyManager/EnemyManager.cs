using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Pool Enemy Reference")] [SerializeField]
    private Transform poolEnemyContainer;

    [Header("ActivationZone Reference")] [SerializeField]
    private EnemyActivationZone activationZone;

    [Header("Targets")] [SerializeField] private Transform mainTarget;
    [SerializeField] private Transform secondaryTarget;

    // list of active enemies
    private List<Enemy> activeEnemies = new List<Enemy>();


    private void OnEnable()
    {
        activationZone.OnEnemyActivated += GetActiveEnemies;
    }


    private void OnDisable()
    {
        activationZone.OnEnemyActivated -= GetActiveEnemies;
    }


    private void Start()
    {
        GetActiveEnemies();
    }


    private void GetActiveEnemies()
    {
        activeEnemies = new List<Enemy>();
        foreach (Transform child in poolEnemyContainer)
        {
            if (child.TryGetComponent(out Enemy enemy))
            {
                activeEnemies.Add(enemy);
            }
        }
    }


    private void Update()
    {
        if (activeEnemies.Count > 0)
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy.enemyCurrentState == EnemyState.Moving)
                {
                    if (Vector2.Distance(enemy.transform.position, mainTarget.position) < enemy.AttackMeleeRange 
                        && !enemy.isRangeEnemy)
                    {
                        enemy.enemyCurrentState = EnemyState.AttackingMelee;
                    }
                    else if (Vector2.Distance(enemy.transform.position, mainTarget.position) < enemy.AttackDistanceRange
                        && enemy.isRangeEnemy)
                    {
                        enemy.enemyCurrentState = EnemyState.AttackingDistance;
                    }
                    else
                    {
                        EnemyMove(enemy);
                    }
                }
                else if (enemy.enemyCurrentState == EnemyState.Idle)
                {
                    if (Vector2.Distance(enemy.transform.position, mainTarget.position) < enemy.DetectionRange)
                    {
                        enemy.enemyCurrentState = EnemyState.Moving;
                    }
                }else if (enemy.enemyCurrentState is > EnemyState.AttackingMelee or > EnemyState.AttackingDistance)
                {
                    enemy.enemyCurrentState = EnemyState.Moving;
                }
            }
        }
    }

    private void EnemyMove(Enemy enemy)
    {
        enemy.transform.position =
            Vector2.MoveTowards(enemy.transform.position, mainTarget.position, 5f * Time.deltaTime);
    }
}