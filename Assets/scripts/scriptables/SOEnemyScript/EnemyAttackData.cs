using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy System/Attack")]
public class EnemyAttackData : ScriptableObject
{
    public string attackName;
    public bool isRanged;
    public float cooldown;
    public float damageMelee;
    [ShowIf("isRanged")]
    public float damageRange;
   
    [Header("Visual")]
    public GameObject attackEffectPrefab;
    
    [Header("Range Settings")]
    [ShowIf("isRanged")]
    public GameObject projectilePrefab;
    [ShowIf("isRanged")]
    public float projectileSpeed = 10f;
}