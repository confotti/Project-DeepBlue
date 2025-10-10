using UnityEngine;

public class InteractionButton : MonoBehaviour
{
    public SubmarineController submarine;
    private bool playerInTrigger = false;

    void Update()
    {
        if (playerInTrigger && Input.GetMouseButtonDown(0)) 
        {
            submarine.ToggleSub();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            Debug.Log("Player entered button trigger");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            Debug.Log("Player left button trigger");
        }
    }
}