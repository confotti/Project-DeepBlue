using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float range = 2f;
    [SerializeField] private LayerMask interactLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, range, interactLayer))
        {
            
        }

        
    }
}
