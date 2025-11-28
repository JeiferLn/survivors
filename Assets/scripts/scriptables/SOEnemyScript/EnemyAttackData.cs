using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy System/Attack")]
public class EnemyAttackData : ScriptableObject
{
    public string attackName;
    public float damageMelee;
    public float damageRange;
    public float cooldown;
   
    [Header("Visual")]
    public GameObject attackEffectPrefab;
    
    [Header("Range Settings")]
    public bool isRanged;
    [ShowIf("isRanged")]
    public GameObject projectilePrefab;
    [ShowIf("isRanged")]
    public float projectileSpeed = 10f;
}