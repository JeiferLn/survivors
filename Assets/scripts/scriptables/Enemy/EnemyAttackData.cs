using UnityEngine;

[CreateAssetMenu(fileName = "New Attack TopDown", menuName = "Enemy System TopDown/Attack")]
public class EnemyAttackData : ScriptableObject
{
    public string attackName;
    public float damage;
    public float cooldown;
    public float range;
    public float chargeTime = 0.2f; // Tiempo antes de ejecutar el ataque
    
    [Header("Visual")]
    public GameObject effectPrefab;
    public AudioClip soundEffect;
    public bool showWarningArea = true; // Mostrar área de ataque antes de ejecutar
    
    [Header("Attack Pattern")]
    public AttackPattern pattern = AttackPattern.Single;
    public int projectileCount = 1; // Para ataques múltiples
    public float spreadAngle = 45f; // Para ataques en cono
    
    [Header("Projectile")]
    public bool isRanged;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public bool homingProjectile = false;
    public float homingStrength = 1f;
    
    public enum AttackPattern
    {
        Single,         // Un solo proyectil/ataque
        Burst,         // Múltiples proyectiles a la vez
        Circle,        // Proyectiles en todas direcciones
        Cone,          // Proyectiles en cono
        Wave           // Oleada de proyectiles
    }
}