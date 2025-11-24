using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy TopDown", menuName = "Enemy System TopDown/Enemy Type")]
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
    public EnemyAttackData primaryAttack;
    public EnemyAttackData secondaryAttack;
    [Range(0f, 1f)]
    public float secondaryAttackChance = 0.3f;
    
    [Header("Visual")]
    public Sprite sprite;
    public Color tintColor = Color.white;
    public float scale = 1f;
    public RuntimeAnimatorController animator; // Opcional
    
    [Header("Death")]
    public GameObject deathEffectPrefab;
    public GameObject[] lootPrefabs;
    [Range(0f, 1f)]
    public float lootDropChance = 0.1f;
    
}