using UnityEngine;

public class shooter : MonoBehaviour
{ 
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player"){
            Destroy(gameObject);
        }
    }
}
