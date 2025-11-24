using System.Collections;
using UnityEngine;
using System;

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

    public int WavesCleared { get; private set; }  // 已经真正“清完”的波数
    public Action<int> OnWavesClearedChanged;

    [Header("Loop Settings")]
    public bool loopLastWave = true;   // 无限循环最后一波

    // 🔴 新增：当前场上活着的敌人数
    public int AliveEnemies { get; private set; }
    public Action<int> OnAliveEnemiesChanged;

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

    // 🔴 由 EnemyDeathReporter 调用
    public void NotifyEnemyDestroyed()
    {
        if (AliveEnemies > 0)
        {
            AliveEnemies--;
            OnAliveEnemiesChanged?.Invoke(AliveEnemies);
        }
    }

    IEnumerator SpawnAllWaves()
    {
        IsSpawning = true;
        AliveEnemies = 0;
        OnAliveEnemiesChanged?.Invoke(AliveEnemies);

        if (firstWaveDelay > 0f)
            yield return new WaitForSeconds(firstWaveDelay);

        int i = 0;

        while (true)
        {
            EnemyWave wave;

            if (i < waves.Length)   // 普通波次
            {
                CurrentWaveIndex = i;
                wave = waves[i];
            }
            else                    // 无限循环
            {
                CurrentWaveIndex = waves.Length - 1;

                if (loopLastWave)
                {
                    wave = waves[waves.Length - 1];
                }
                else
                {
                    break;
                }
            }

            if (wave != null)
            {
                // 保险：上一波如果还有怪没死，等它们先清完
                if (AliveEnemies > 0)
                    yield return new WaitUntil(() => AliveEnemies == 0);

                // 生成这一波所有敌人
                yield return StartCoroutine(SpawnWave(wave));

                // ✅ 关键：等这一波所有敌人都“清掉”再算这一波完成
                if (AliveEnemies > 0)
                    yield return new WaitUntil(() => AliveEnemies == 0);

                WavesCleared++;
                OnWavesClearedChanged?.Invoke(WavesCleared);

                // 这一波清完后，再等 timeBeforeNextWave 进入下一波
                if (wave.timeBeforeNextWave > 0f)
                    yield return new WaitForSeconds(wave.timeBeforeNextWave);
            }

            i++;
        }

        IsSpawning = false;
    }

    IEnumerator SpawnWave(EnemyWave wave)
    {
        if (wave == null || wave.groups == null || wave.groups.Length == 0)
            yield break;

        foreach (var group in wave.groups)
        {
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

        // 🔴 新增：计数 + 挂上 Reporter
        AliveEnemies++;
        OnAliveEnemiesChanged?.Invoke(AliveEnemies);

        EnemyDeathReporter reporter = enemy.AddComponent<EnemyDeathReporter>();
        reporter.Init(this);

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
