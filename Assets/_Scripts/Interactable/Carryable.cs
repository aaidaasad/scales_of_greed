using UnityEngine;

public class Carryable : MonoBehaviour
{
    public Vector3 carriedLocalPosition = new Vector3(0f, 0.5f, 0.7f);
    public Vector3 carriedLocalEulerAngles = Vector3.zero;

    Rigidbody rb;
    Collider[] colliders;
    Transform originalParent;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void OnPickup(Transform carrier)
    {
        originalParent = transform.parent;
        transform.SetParent(carrier);
        transform.localPosition = carriedLocalPosition;
        transform.localEulerAngles = carriedLocalEulerAngles;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
        }
    }

    public void OnDrop()
    {
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
            }
        }

        Vector3 pos = transform.position;
        Ray ray = new Ray(pos + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            transform.position = hit.point;
        }
    }
}
