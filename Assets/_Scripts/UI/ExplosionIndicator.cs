using UnityEngine;

public class ExplosionIndicator : MonoBehaviour
{
    public void SetRadius(float radius)
    {
        transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
