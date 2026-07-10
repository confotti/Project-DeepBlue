using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraHead;

    [Header("Walking")]
    [SerializeField] private float walkBobAmount = 0.03f;
    [SerializeField] private float walkBobSpeed = 8f;

    [Header("Running")]
    [SerializeField] private float runBobAmount = 0.08f;
    [SerializeField] private float runBobSpeed = 12f;

    [SerializeField] private float smoothness = 10f;


    private Vector3 startPosition;
    private float bobTimer;


    private void Awake()
    {
        startPosition = cameraHead.localPosition;
    }


    private void LateUpdate()
    {
        ApplyBobbing();
    }


    private void ApplyBobbing()
    {
        Vector2 movement = playerMovement.InputHandler.Move;

        bool moving = movement.magnitude > 0.1f;

        if (playerMovement.StateMachine.CurrentState != playerMovement.StandingState)
        {
            bobTimer = 0;

            cameraHead.localPosition = Vector3.Lerp(
                cameraHead.localPosition,
                startPosition,
                Time.deltaTime * smoothness
            );

            return;
        }

        if (!moving)
        {
            bobTimer = 0;

            cameraHead.localPosition = Vector3.Lerp(
                cameraHead.localPosition,
                startPosition,
                Time.deltaTime * smoothness
            );

            return; 
        }


        bool running = Input.GetKey(KeyCode.LeftShift);


        float amount = running ? runBobAmount : walkBobAmount;
        float speed = running ? runBobSpeed : walkBobSpeed;


        bobTimer += Time.deltaTime * speed;


        float yOffset = Mathf.Sin(bobTimer) * amount;


        Vector3 target = startPosition;
        target.y += yOffset;


        cameraHead.localPosition = Vector3.Lerp(
            cameraHead.localPosition,
            target,
            Time.deltaTime * smoothness
        );
    }
} 