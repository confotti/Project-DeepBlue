using UnityEngine;

public class StalkerBehaviour : MonoBehaviour
{

    [SerializeField] private float speed = 2;

    private Rigidbody rb;
    void Start()
    {
        
    }

    void Update()
    {
        var a = transform.position;
        a += transform.forward * speed * Time.deltaTime;
        transform.position = a;
    }
}
