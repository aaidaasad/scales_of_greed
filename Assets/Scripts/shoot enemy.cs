using UnityEngine;
using UnityEngine.AI;

public class shootenemy : MonoBehaviour
{
    [SerializeField] private float timer = 5f;
    private float bulletTime;

    public GameObject enemyBullet;
    public Transform spawnPoint;
    public float enemySpeed;

    public NavMeshAgent enemy;
    public Transform player;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        enemy.SetDestination(player.position);

        ShootAtPlayer();   // <-- Don't forget to call this!
    }

    void ShootAtPlayer()
    {
        bulletTime -= Time.deltaTime;

        if (bulletTime > 0)
            return;

        bulletTime = timer;

        GameObject bulletObj = Instantiate(enemyBullet, 
                                           spawnPoint.transform.position, 
                                           spawnPoint.transform.rotation) as GameObject;

        Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();

        bulletRig.AddForce(bulletRig.transform.forward * enemySpeed);

        void OnTriggerEnter(Collider other){
            Destroy(bulletObj);
        }
    }
}