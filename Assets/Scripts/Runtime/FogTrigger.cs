using UnityEngine;

public class FogTrigger : MonoBehaviour
{
    public Collider collider; 

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player") 
        {
            Debug.Log("Player has entered"); 
        }
    }
}
