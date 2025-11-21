using System.Collections;
using UnityEngine;

public class EnemyWaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyGroup
    {
        public string groupName = "Group";
        public GameObject enemyPrefab;
        public Transform spawnPoint;
        public Transform[] waypoints;
        public int count = 5;
        public float spawnInterval = 1f;
        public float startDelay = 0f;
    }

    [System.Serializable]
    public class EnemyWave
    {
        public string waveName = "Wave";
        public EnemyGroup[] groups;
        public float timeBeforeNextWave = 3f;
        public float speedMultiplier = 1f;
        public float healthMultiplier = 1f;
    }

    public EnemyWave[] waves;
    public GameObject healthBarPrefab;

    public bool autoStart = true;
    public float firstWaveDelay = 2f;

    public int CurrentWaveIndex { get; private set; } = -1;
    public bool IsSpawning { get; private set; }

    void Start()
    {
        if (autoStart && waves != null && waves.Length > 0)
        {
            StartWaves();
        }
    }

    public void StartWaves()
    {
        if (!IsSpawning && waves != null && waves.Length > 0)
        {
            StartCoroutine(SpawnAllWaves());
        }
    }

    IEnumerator SpawnAllWaves()
    {
        IsSpawning = true;

        if (firstWaveDelay > 0f)
            yield return new WaitForSeconds(firstWaveDelay);

        for (int i = 0; i < waves.Length; i++)
        {
            CurrentWaveIndex = i;
            EnemyWave wave = waves[i];

            if (wave != null)
                yield return StartCoroutine(SpawnWave(wave));

            if (wave != null && wave.timeBeforeNextWave > 0f && i < waves.Length - 1)
            {
                yield return new WaitForSeconds(wave.timeBeforeNextWave);
            }
        }

        IsSpawning = false;
    }

    IEnumerator SpawnWave(EnemyWave wave)
    {
        if (wave == null || wave.groups == null || wave.groups.Length == 0)
            yield break;

        for (int i = 0; i < wave.groups.Length; i++)
        {
            EnemyGroup group = wave.groups[i];
            if (group == null || group.enemyPrefab == null || group.count <= 0)
                continue;

            if (group.startDelay > 0f)
                yield return new WaitForSeconds(group.startDelay);

            for (int c = 0; c < group.count; c++)
            {
                SpawnEnemy(group, wave);

                if (group.spawnInterval > 0f && c < group.count - 1)
                    yield return new WaitForSeconds(group.spawnInterval);
            }
        }
    }

    void SpawnEnemy(EnemyGroup group, EnemyWave wave)
    {
        Transform spawn = group.spawnPoint != null ? group.spawnPoint : transform;
        GameObject enemy = Instantiate(group.enemyPrefab, spawn.position, spawn.rotation);

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        EnemyMover mover = enemy.GetComponent<EnemyMover>();

        if (healthBarPrefab != null && health != null)
        {
            GameObject bar = Instantiate(healthBarPrefab);
            EnemyHealthBar hb = bar.GetComponent<EnemyHealthBar>();
            if (hb != null)
            {
                hb.target = health;
            }
        }

        if (mover != null)
        {
            if (group.waypoints != null && group.waypoints.Length > 0)
                mover.waypoints = group.waypoints;

            if (wave != null && wave.speedMultiplier != 1f)
                mover.moveSpeed *= wave.speedMultiplier;
        }

        if (health != null)
        {
            float multiplier = wave != null ? wave.healthMultiplier : 1f;

            if (!Mathf.Approximately(multiplier, 1f))
            {
                multiplier = Mathf.Max(multiplier, 0.01f);
                float newMax = health.maxHealth * multiplier;
                if (newMax < 1f) newMax = 1f;

                health.ResetHealth(newMax);
            }
        }
    }
}
