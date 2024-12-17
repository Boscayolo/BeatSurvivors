// File: EnemySpawner.cs
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform planet; // The planet's transform
    [SerializeField]private float planetRadius; // Radius of the planet, calculated dynamically

    private Dictionary<string, List<GameObject>> enemyPools = new Dictionary<string, List<GameObject>>(); // Pools for different enemy types
    private Dictionary<string, GameObject> enemyPrefabs = new Dictionary<string, GameObject>(); // Enemy prefabs by type

    private float spawnTimer = 0f;
    private int activeEnemies = 0;

    public void Start()
    {
        CalculatePlanetRadius();
    }

    private void CalculatePlanetRadius()
    {
        //if (planet.TryGetComponent(out MeshRenderer renderer))
        //{
        //    planetRadius = renderer.bounds.extents.magnitude;
        //}
        //else if (planet.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null)
        //{
        //    planetRadius = filter.sharedMesh.bounds.extents.magnitude * planet.localScale.x;
        //}
        //else
        //{
        //    Debug.LogWarning("Unable to calculate planet radius. Defaulting to 10.");
        //    planetRadius = 10f;
        //}
    }

    private void OnDrawGizmosSelected()
    {
        if (planet != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(planet.position, planetRadius);
        }
    }

    public void InitializePools(WaveData waveData)
    {
        foreach (var wave in waveData.waves)
        {
            UpdatePoolSize(wave.enemyType, wave.poolSize, wave.enemyPrefab);
        }
    }

    private void UpdatePoolSize(string enemyType, int newSize, GameObject prefab)
    {
        if (!enemyPools.ContainsKey(enemyType))
        {
            enemyPools[enemyType] = new List<GameObject>();
            enemyPrefabs[enemyType] = prefab;
        }

        var pool = enemyPools[enemyType];

        // Expand the pool if needed
        while (pool.Count < newSize)
        {
            GameObject enemy = Instantiate(prefab);
            enemy.SetActive(false);
            pool.Add(enemy);
        }

        // Shrink the pool if needed
        while (pool.Count > newSize)
        {
            GameObject enemy = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            Destroy(enemy);
        }
    }

    public void UpdateSpawner(Wave activeWave, float deltaTime)
    {
        spawnTimer += deltaTime;

        if (spawnTimer >= activeWave.spawnInterval && activeEnemies < activeWave.maxConcurrentEnemies)
        {
            SpawnEnemyFromWave(activeWave);
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemyFromWave(Wave wave)
    {
        int spawnCount = (int)Random.Range(wave.minSpawnCount, wave.maxSpawnCount);

        for (int i = 0; i < spawnCount; i++)
        {
            var enemyType = wave.enemyType;
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 spawnPosition = planet.position + randomDirection * planetRadius;

            if (SpawnEnemy(enemyType, spawnPosition, randomDirection))
            {
                activeEnemies++;
            }
        }

    }

    private bool SpawnEnemy(string enemyType, Vector3 position, Vector3 direction)
    {
        GameObject enemy = GetPooledEnemy(enemyType);
        if (enemy == null)
        {
            return false;
        }

        enemy.transform.position = position;
        enemy.transform.rotation = Quaternion.LookRotation(direction);
        enemy.SetActive(true);

        Entity entity = enemy.GetComponent<Entity>();
        if (entity != null)
        {
            entity.OnEntityDisabled += HandleEnemyDisabled;
        }

        return true;
    }

    private void HandleEnemyDisabled()
    {
        activeEnemies--;
    }

    GameObject GetPooledEnemy(string enemyType)
    {
        if (enemyPools.ContainsKey(enemyType))
        {
            foreach (var enemy in enemyPools[enemyType])
            {
                if (!enemy.activeInHierarchy)
                {
                    return enemy;
                }
            }
        }
        return null;
    }

    public void DeactivateAllEnemies()
    {
        foreach (var pool in enemyPools.Values)
        {
            foreach (var enemy in pool)
            {
                if (enemy.activeInHierarchy)
                {
                    enemy.SetActive(false);
                }
            }
        }
        activeEnemies = 0;
    }
}
