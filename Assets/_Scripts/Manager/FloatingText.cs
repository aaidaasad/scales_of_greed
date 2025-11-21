using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public TMP_Text text;
    public float lifetime = 1f;
    public float floatSpeed = 1f;
    public float baseFontSize = 30f;
    public float fadeInPortion = 0.2f;
    public float fadeOutPortion = 0.2f;

    float timer;
    Color baseColor;
    Camera cam;

    public void Init(string content, Color color, float size)
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>();

        if (cam == null)
            cam = Camera.main;

        if (text != null)
        {
            baseColor = color;
            baseColor.a = 1f;
            Color c = baseColor;
            c.a = 0f;
            text.color = c;
            text.text = content;
            text.fontSize = baseFontSize * size;
        }

        timer = 0f;
    }

    void Update()
    {
        if (cam == null)
            cam = Camera.main;

        timer += Time.deltaTime;
        float t = lifetime > 0f ? Mathf.Clamp01(timer / lifetime) : 1f;

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        if (cam != null)
        {
            Vector3 camForward = cam.transform.rotation * Vector3.forward;
            Vector3 camUp = cam.transform.rotation * Vector3.up;
            transform.rotation = Quaternion.LookRotation(camForward, camUp);
        }


        if (text != null)
        {
            float alpha = 1f;
            if (t < fadeInPortion)
            {
                alpha = Mathf.Clamp01(t / Mathf.Max(fadeInPortion, 0.0001f));
            }
            else if (t > 1f - fadeOutPortion)
            {
                float u = (t - (1f - fadeOutPortion)) / Mathf.Max(fadeOutPortion, 0.0001f);
                alpha = 1f - Mathf.Clamp01(u);
            }

            Color c = baseColor;
            c.a = alpha;
            text.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
