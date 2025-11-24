using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy System/Attack")]
public class EnemyAttackData : ScriptableObject
{
    public string attackName;
    public float damage;
    public float cooldown;
    public float range;
   
    [Header("Visual")]
    public GameObject effectPrefab;
    public AudioClip soundEffect;
    
    [Header("Range Settings")]
    public bool isRanged;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
}