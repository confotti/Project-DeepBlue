using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10;

    //References
    private PlayerInputHandler inputHandler;
    private Rigidbody rb;

    private States currentState = States.swimming;

    //TODO: Fix real statemachine
    public enum States
    {
        standing,
        swimming
    }

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void FixedUpdate()
    {
        var move = inputHandler.Move;
        Debug.Log(move);
        rb.linearVelocity = new Vector3(move.x, 0, move.y) * speed;
    }

}
