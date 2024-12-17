using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float gameDurationMinutes = 10f; // Total game duration in minutes
    public WaveData waveData; // Reference to WaveData ScriptableObject
    public EnemySpawner enemySpawner; // Reference to the EnemySpawner script

    private float gameTimeElapsed = 0f;
    private Wave currentWave = null;

    void Start()
    {
        // Initialize pools based on wave data
        enemySpawner.InitializePools(waveData);
    }

    void Update()
    {
        gameTimeElapsed += Time.deltaTime;

        // Determine the current active wave
        foreach (var wave in waveData.waves)
        {
            float waveStartTime = wave.startMinute * 60;
            float waveEndTime = wave.endMinute * 60;

            if (gameTimeElapsed >= waveStartTime && gameTimeElapsed <= waveEndTime)
            {
                if (currentWave != wave)
                {
                    currentWave = wave;
                    enemySpawner.InitializePools(waveData); // Ensure the pool is resized to match the new wave
                }

                enemySpawner.UpdateSpawner(currentWave, Time.deltaTime);
                return;
            }
        }

        // If no wave is active, clear the current wave
        currentWave = null;
    }
}
