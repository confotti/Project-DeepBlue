using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private float mouseSensitivity = 1;
    [SerializeField] private float gravity = 20;
    [SerializeField] private float jumpPower = 20;

    [SerializeField] private float rideHeight = 3;
    [SerializeField] private float rideSpringStrength = 500;
    [SerializeField] private float rideSpringDamper = 40;

    private Vector2 rotation;
    private float lookYMax = 90;

    //References
    private PlayerInputHandler inputHandler;
    private Rigidbody rb;
    private Collider collider;

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
        collider = GetComponent<Collider>();
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

    private void Update()
    {
        CameraMovement();
    }

    private void FixedUpdate()
    {
        Movement();
        
        
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

            RaycastHit hit;
            var a = collider.bounds.center;
            a.y = collider.bounds.min.y;
            if (Physics.Raycast(a, Vector3.down, out hit, rideHeight*2, ~0))
                springThing(hit);
        }

    }

    private void CameraMovement()
    {
        rotation.x += inputHandler.Look.x * mouseSensitivity;
        rotation.y += inputHandler.Look.y * mouseSensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -lookYMax, lookYMax);
        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
        transform.rotation = xQuat * yQuat;
    }

    private void OnJump()
    {
        if (currentState != States.standing) return;

        var move = rb.linearVelocity;
        move.y = jumpPower;
        rb.linearVelocity = move;
    }

    private void springThing(RaycastHit hit)
    {
        Vector3 vel = rb.linearVelocity;
        Vector3 rayDir = Vector3.down;

        Vector3 otherVel = Vector3.zero;
        Rigidbody hitBody = hit.rigidbody;
        if (hitBody != null)
        {
            otherVel = hitBody.linearVelocity;
        }

        float rayDirVel = Vector3.Dot(rayDir, vel);
        float otherDirVel = Vector3.Dot(rayDir, otherVel);

        float relVel = rayDirVel - otherDirVel;

        float x = hit.distance - rideHeight;

        float springForce = (x * rideSpringStrength) - (relVel * rideSpringDamper);

        rb.AddForce(rayDir * springForce);

        /*
        if(hitBody != null)
        {
            hitBody.AddForceAtPosition(rayDir * -springForce, hit.point);
        }
        */
    }

    private void OnDrawGizmosSelected()
    {
        collider = GetComponent<Collider>();
        var a = collider.bounds.center;
        a.y = collider.bounds.min.y;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(a, a + Vector3.down * rideHeight * 2);
    }
}
