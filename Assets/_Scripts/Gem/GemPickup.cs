using UnityEngine;
using Random = UnityEngine.Random;

public class GemPickup : MonoBehaviour
{
    public int amount = 1;
    public float rotateSpeed = 90f;
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 2f;
    public float moveToPlayerSpeed = 10f;
    public float collectHeightOffset = 1f;
    public float collectAcceleration = 3f;

    Transform target;
    Vector3 startPosition;
    bool isCollecting;
    float timeOffset;
    float collectProgress;

    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.value * 10f;
    }

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);

        if (!isCollecting)
        {
            float y = startPosition.y + Mathf.Sin((Time.time + timeOffset) * bobFrequency) * bobAmplitude;
            Vector3 p = transform.position;
            p.y = y;
            transform.position = p;
        }
        else
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            collectProgress += Time.deltaTime * collectAcceleration;
            float speedFactor = Mathf.Clamp01(collectProgress);
            float stepSpeed = moveToPlayerSpeed * (0.5f + speedFactor);

            Vector3 targetPos = target.position + Vector3.up * collectHeightOffset;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, stepSpeed * Time.deltaTime);

            float distance = Vector3.Distance(transform.position, targetPos);
            if (distance < 0.1f)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddGems(amount);
                }

                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollecting) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            target = player.transform;
            isCollecting = true;
            collectProgress = 0f;

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
}
