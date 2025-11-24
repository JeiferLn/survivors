using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Enemy Database TopDown", menuName = "Enemy System TopDown/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public EnemyType enemyType;
        public int baseWeight = 1;
        public int minWave = 0;
        public float healthMultiplierPerWave = 0.1f;
        public float speedMultiplierPerWave = 0.05f;
    }
    
    [System.Serializable]
    public class WaveConfiguration
    {
        public string waveName = "Wave";
        public int enemyCount = 10;
        public float spawnDelay = 0.5f;
        public float waveStartDelay = 3f;
        public int bonusScore = 100;
        
        [Header("Enemy Distribution")]
        public bool useSpecificEnemies = false;
        public List<EnemySpawnInfo> specificEnemies = new List<EnemySpawnInfo>();
    }
    
    [Header("Enemy Pool")]
    public List<EnemySpawnInfo> enemyPool = new List<EnemySpawnInfo>();
    
    [Header("Wave Configurations")]
    public List<WaveConfiguration> predefinedWaves = new List<WaveConfiguration>();
    
    [Header("Infinite Mode")]
    public bool useInfiniteMode = true;
    public int baseEnemiesPerWave = 10;
    public int enemiesIncreasePerWave = 2;
    public float difficultyMultiplier = 1.15f;
    
    public EnemyType GetRandomEnemy(int currentWave)
    {
        List<EnemySpawnInfo> availableEnemies = new List<EnemySpawnInfo>();
        int totalWeight = 0;
        
        foreach (var enemy in enemyPool)
        {
            if (currentWave >= enemy.minWave)
            {
                availableEnemies.Add(enemy);
                totalWeight += enemy.baseWeight;
            }
        }
        
        if (availableEnemies.Count == 0) return null;
        
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        foreach (var enemy in availableEnemies)
        {
            currentWeight += enemy.baseWeight;
            if (randomValue < currentWeight)
            {
                return enemy.enemyType;
            }
        }
        
        return availableEnemies[0].enemyType;
    }
}