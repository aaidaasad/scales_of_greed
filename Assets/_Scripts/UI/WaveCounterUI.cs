using UnityEngine;
using TMPro;

public class WaveCounterUI : MonoBehaviour
{
    public EnemyWaveSpawner spawner;
    public TextMeshProUGUI waveText;
    public string prefix = "Finished Waves：";

    void Start()
    {
        if (spawner == null)
            spawner = FindObjectOfType<EnemyWaveSpawner>();

        UpdateText(spawner != null ? spawner.WavesCleared : 0);

        if (spawner != null)
            spawner.OnWavesClearedChanged += UpdateText;
    }

    void OnDestroy()
    {
        if (spawner != null)
            spawner.OnWavesClearedChanged -= UpdateText;
    }

    void UpdateText(int count)
    {
        if (waveText == null) return;
        waveText.text = prefix + count.ToString();
    }
}
