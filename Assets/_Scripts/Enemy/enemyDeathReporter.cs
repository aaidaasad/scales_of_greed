using UnityEngine;

public class EnemyDeathReporter : MonoBehaviour
{
    EnemyWaveSpawner spawner;

    public void Init(EnemyWaveSpawner s)
    {
        spawner = s;
    }

    void OnDestroy()
    {
        // 只要这只敌人被销毁（被打死 / 撞基地 / 手动 Destroy），就通知 Spawner
        if (spawner != null)
        {
            spawner.NotifyEnemyDestroyed();
        }
    }
}
