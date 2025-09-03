using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float mouseSensitivity = 1;
    [SerializeField] private float gravity = 20;
    [SerializeField] private float jumpPower = 20;

    private Vector2 rotation;
    private float lookYMax = 90;

    //References
    private PlayerInputHandler inputHandler;
    private Rigidbody rb;

    [SerializeField] private States currentState = States.standing;

    //TODO: Fix real statemachine
    public enum States
    {
        standing,
        swimming
    }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        inputHandler = GetComponent<PlayerInputHandler>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        inputHandler.OnJump += OnJump;

        rotation.x = transform.rotation.eulerAngles.y;
        rotation.y = transform.rotation.eulerAngles.x;
    }

    private void OnDisable()
    {
        inputHandler.OnJump -= OnJump;
    }

    private void FixedUpdate()
    {
        Movement();

        rotation.x += inputHandler.Look.x * mouseSensitivity;
        rotation.y += inputHandler.Look.y * mouseSensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -lookYMax, lookYMax);
        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
        transform.rotation = xQuat * yQuat;
    }

    private void Movement()
    {
        if (currentState == States.swimming)
        {
            rb.linearVelocity = (transform.rotation * new Vector3(inputHandler.Move.x, 0, inputHandler.Move.y)).normalized * speed;
        }
        else if (currentState == States.standing)
        {
            var move = transform.rotation * new Vector3(inputHandler.Move.x, 0, inputHandler.Move.y);
            move.y = 0;
            move = move.normalized * speed;
            move.y = rb.linearVelocity.y - gravity * Time.fixedDeltaTime;
            rb.linearVelocity = move;
        }

    }

    private void OnJump()
    {
        if (currentState != States.standing) return;

        var move = rb.linearVelocity;
        move.y = jumpPower;
        rb.linearVelocity = move;
    }

}
