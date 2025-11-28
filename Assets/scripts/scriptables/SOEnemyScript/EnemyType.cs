using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy System/Enemy Type")]
public class EnemyType: ScriptableObject
{
    [Header("Basic Stats")]
    public string enemyName;
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public int expAmount = 10;
    
    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackMeleeRange = 2f; 
    public float attackDistanceRange = 3f;
    
    [Header("Attacks")]
    public EnemyAttackData meleeAttack;
    public EnemyAttackData rangeAttack;
    [Range(0f, 1f)]
    public float attackDelay = 0.3f;
    
    [Header("Visual")]
    public GameObject enemy3DModel;
    
    [Header("Death")]
    public GameObject deathEffectPrefab;
    public GameObject[] lootPrefabs;
    [Range(0f, 1f)]
    public float lootDropChance = 0.1f;
    
}