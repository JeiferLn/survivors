using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy System/Attack")]
public class EnemyAttackData : ScriptableObject
{
    public string attackName;

    [Header("Damage")]
    public float damage;

    [Header("Cooldown")]
    public float cooldown = 1f;

    [Header("Melee or Ranged")]
    public bool usesProjectile;

    [ShowIf("usesProjectile")]
    public GameObject projectilePrefab;

    [ShowIf("usesProjectile")]
    public float projectileSpeed = 10f;

    [Header("Visual")]
    public GameObject attackEffectPrefab;
}