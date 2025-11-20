using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // 敌人预制体
    public Transform[] waypoints;  // 这条路线（从 Path 的子物体拖进来）

    public float spawnInterval = 2f; // 生成间隔（秒）
    public int spawnCount = 10;      // 要生成多少个敌人（可改成无穷）
    public GameObject healthBarPrefab;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        int spawned = 0;

        while (spawned < spawnCount)
        {
            SpawnEnemy();
            spawned++;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();

        if (healthBarPrefab != null && health != null)
        {
            GameObject bar = Instantiate(healthBarPrefab);
            EnemyHealthBar hb = bar.GetComponent<EnemyHealthBar>();
            hb.target = health;
        }

        enemy.GetComponent<EnemyMover>().waypoints = waypoints;
    }

}
