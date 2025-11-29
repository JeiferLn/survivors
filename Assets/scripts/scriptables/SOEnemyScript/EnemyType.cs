using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy System/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [Header("Basic Stats")]
    public string enemyName;
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public int expAmount = 10;

    [Header("Detection")]
    public float detectionRange = 5f;

    [Header("Attack Mode")]
    public EnemyAttackMode attackMode;

    // -----------------------------
    // MELEE ATTACK
    // -----------------------------
    [ShowIf("ShowMelee")]
    [FoldoutGroup("Melee Attack")]
    public float attackMeleeRange = 2f;

    [ShowIf("ShowMelee")]
    [FoldoutGroup("Melee Attack")]
    public EnemyAttackData meleeAttack;

    // -----------------------------
    // RANGED ATTACK
    // -----------------------------
    [ShowIf("ShowRanged")]
    [FoldoutGroup("Ranged Attack")]
    public float attackDistanceRange = 3f;

    [ShowIf("ShowRanged")]
    [FoldoutGroup("Ranged Attack")]
    public EnemyAttackData rangeAttack;

    [Header("Visual")]
    public GameObject enemy3DModel;

    [Header("Death")]
    public GameObject deathEffectPrefab;
    public GameObject[] lootPrefabs;
    [Range(0f, 1f)]
    public float lootDropChance = 0.1f;

    // -----------------------------
    // CONDITIONALS FOR ODIN
    // -----------------------------
    private bool ShowMelee => attackMode == EnemyAttackMode.Melee || attackMode == EnemyAttackMode.Hybrid;
    private bool ShowRanged => attackMode == EnemyAttackMode.Ranged || attackMode == EnemyAttackMode.Hybrid;
}