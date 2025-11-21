using UnityEngine;
using static CartoonFX.CFXR_Effect;

public class LightningPotion : Potion
{
    [Header("Lightning Strike")]
    public float lightningDamage = 10f;
    public GameObject lightningHitVfxPrefab;
    public float vfxHeightOffset = 1.5f;
    public float vfxLifetime = 2f;

    [Header("Paralyze")]
    public float paralyzeDuration = 0.6f;
    public GameObject paralyzeVfxPrefab;
    public float paralyzeVfxHeightOffset = 1.2f;

    [Header("Screen Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;

    [Header("Lightning Speed Curve")]
    public AnimationCurve speedCurve;
    public float speedCurveDuration = 1.2f;

    public void TriggerLightning()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
        }

        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyHealth enemy = enemies[i];
            if (enemy == null) continue;

            Transform t = enemy.transform;

            if (lightningHitVfxPrefab != null)
            {
                Vector3 pos = t.position + Vector3.up * vfxHeightOffset;
                GameObject vfx = Instantiate(lightningHitVfxPrefab, pos, Quaternion.identity);
                if (vfxLifetime > 0f)
                    Destroy(vfx, vfxLifetime);
            }

            enemy.TakeDamage(lightningDamage);

            EnemyMover mover = enemy.GetComponent<EnemyMover>();
            if (mover != null)
            {
                mover.ApplyParalyze(paralyzeDuration);

                if (paralyzeVfxPrefab != null)
                {
                    Vector3 ppos = t.position + Vector3.up * paralyzeVfxHeightOffset;
                    GameObject pvfx = Instantiate(paralyzeVfxPrefab, ppos, Quaternion.identity, t);
                    Destroy(pvfx, paralyzeDuration);
                }
            }
        }
    }
}
