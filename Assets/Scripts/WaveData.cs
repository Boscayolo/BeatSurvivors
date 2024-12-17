using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveData", menuName = "ScriptableObjects/WaveData", order = 1)]
public class WaveData : ScriptableObject
{
    public List<Wave> waves = new List<Wave>();
}

[System.Serializable]
public class Wave
{
    public string enemyType; // Name of the enemy type
    public GameObject enemyPrefab; // Prefab for the enemy type
    public int poolSize; // Pool size for this enemy type
    public int maxConcurrentEnemies; // Max number of concurrent active enemies
    public float spawnInterval; // Time interval between spawns
    public float minSpawnCount;
    public float maxSpawnCount;
    public float startMinute; // Start time (in minutes) of the wave
    public float endMinute; // End time (in minutes) of the wave
}
