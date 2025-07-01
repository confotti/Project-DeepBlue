using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float mouseSensitivity = 1;

    private Vector2 eulerLook;
    private float lookYMin = -90;
    private float lookYMax = 90;

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
        eulerLook.x = transform.rotation.eulerAngles.x;
        eulerLook.y = transform.rotation.eulerAngles.y;
    }

    private void OnDisable()
    {
        
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(inputHandler.Move.x, 0, inputHandler.Move.y) * speed;

        eulerLook.x -= inputHandler.Look.y * mouseSensitivity;
        eulerLook.y += inputHandler.Look.x * mouseSensitivity;
        eulerLook.x %= 360;
        eulerLook.x = Mathf.Clamp(eulerLook.x, lookYMin, lookYMax);
        transform.rotation = Quaternion.Euler(eulerLook);
    }

}
