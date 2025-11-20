using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    public Transform[] waypoints; 
    public float moveSpeed = 3f;   
    public float reachThreshold = 0.1f; 

    private int currentIndex = 0;  
    public float damageToBase = 1f;  
    BaseHealth baseHealth;


    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;


        Transform target = waypoints[currentIndex];


        Vector3 dir = (target.position - transform.position);
        Vector3 moveDir = dir.normalized;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDir);
        }


        if (dir.magnitude <= reachThreshold)
        {
            currentIndex++;


            if (currentIndex >= waypoints.Length)
            {
                ReachDestination();
            }
        }
    }
    void Start()
    {
        baseHealth = FindObjectOfType<BaseHealth>();  
    }

    private void ReachDestination()
    {

        if (baseHealth != null)
        {
            baseHealth.TakeDamage(damageToBase);
        }


        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
