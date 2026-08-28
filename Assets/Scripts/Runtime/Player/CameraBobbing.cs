using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform cameraSway;

    [Header("Walking")]
    [SerializeField] private float walkBobAmount = 0.03f;
    [SerializeField] private float walkBobSpeed = 8f;

    [Header("Running")]
    [SerializeField] private float runBobAmount = 0.08f;
    [SerializeField] private float runBobSpeed = 12f;

    [Header("Swimming")]
    [SerializeField] private float swimSwayAmount = 0.02f;
    [SerializeField] private float swimSwaySpeed = 1.5f;

    [Header("Fast Swimming")]
    [SerializeField] private float fastSwimSwayAmount = 0.04f;
    [SerializeField] private float fastSwimSwaySpeed = 2.2f;

    [Header("Swimming Rotation")]
    [SerializeField] private float swimPitch = 0.5f;
    [SerializeField] private float swimRoll = 0.3f;

    [Header("Idle Underwater")]
    [SerializeField] private float idleSwayAmount = 0.008f;
    [SerializeField] private float idleSwaySpeed = 0.8f;
    [SerializeField] private float idlePitch = 0.15f;
    [SerializeField] private float idleRoll = 0.1f;

    [Header("General")]
    [SerializeField] private float smoothness = 10f;
    [SerializeField] private float swimmingBlendSpeed = 4f;

    private Vector3 startPosition;
    private Vector3 positionVelocity;
    private float bobTimer;
    private float swimBlend;

    private void Awake()
    {
        startPosition = cameraSway.localPosition;
    }

    private void LateUpdate()
    {
        ApplyBobbing();
    }

    private void ApplyBobbing()
    {
        if (playerMovement.IsSwimming)
        {
            ApplySwimmingSway();
            return;
        }

        if (playerMovement.StateMachine.CurrentState == playerMovement.StandingState)
        {
            ApplyWalkingBobbing();
            return;
        }

        ResetBobbing();
    }

    private void ApplyWalkingBobbing()
    {
        if (!playerMovement.IsMoving)
        {
            ResetBobbing();
            return;
        }

        float amount = playerMovement.IsSprinting ? runBobAmount : walkBobAmount;
        float speed = playerMovement.IsSprinting ? runBobSpeed : walkBobSpeed;

        bobTimer += Time.deltaTime * speed;

        float yOffset = Mathf.Sin(bobTimer) * amount;

        Vector3 target = startPosition;
        target.y += yOffset;

        cameraSway.localPosition = Vector3.SmoothDamp(cameraSway.localPosition, target, ref positionVelocity, 1f / smoothness);
        cameraSway.localRotation = Quaternion.Lerp(cameraSway.localRotation, Quaternion.identity, Time.deltaTime * smoothness);
    }

    private void ApplySwimmingSway()
    {
        bool moving = playerMovement.IsMoving;
        bool fastSwimming = playerMovement.IsSprinting;

        float targetBlend = moving ? 1f : 0f;
        swimBlend = Mathf.MoveTowards(swimBlend, targetBlend, Time.deltaTime * swimmingBlendSpeed);

        float speed = fastSwimming ? fastSwimSwaySpeed : swimSwaySpeed;
        float amount = fastSwimming ? fastSwimSwayAmount : swimSwayAmount;

        bobTimer += Time.deltaTime * speed;

        float swimX = Mathf.Sin(bobTimer * 0.8f) * amount;
        float swimY = Mathf.Sin(bobTimer) * amount;

        float idleX = Mathf.Sin(bobTimer * 0.7f) * idleSwayAmount;
        float idleY = Mathf.Sin(bobTimer) * idleSwayAmount;

        float x = Mathf.Lerp(idleX, swimX, swimBlend);
        float y = Mathf.Lerp(idleY, swimY, swimBlend);

        Vector3 targetPosition = startPosition;
        targetPosition.x += x;
        targetPosition.y += y;

        cameraSway.localPosition = Vector3.SmoothDamp(cameraSway.localPosition, targetPosition, ref positionVelocity, 1f / smoothness);

        float idlePitchValue = Mathf.Sin(bobTimer * 0.7f) * idlePitch;
        float idleRollValue = Mathf.Sin(bobTimer * 0.5f) * idleRoll;

        float swimPitchValue = Mathf.Sin(bobTimer * 0.7f) * swimPitch;
        float swimRollValue = Mathf.Sin(bobTimer * 0.5f) * swimRoll;

        if (fastSwimming)
        {
            swimPitchValue *= 1.5f;
            swimRollValue *= 1.5f;
        }

        float pitch = Mathf.Lerp(idlePitchValue, swimPitchValue, swimBlend);
        float roll = Mathf.Lerp(idleRollValue, swimRollValue, swimBlend);

        Quaternion targetRotation = Quaternion.Euler(pitch, 0f, roll);

        cameraSway.localRotation = Quaternion.Lerp(cameraSway.localRotation, targetRotation, Time.deltaTime * smoothness);
    }

    private void ResetBobbing()
    {
        bobTimer = 0;
        swimBlend = 0f;
        positionVelocity = Vector3.zero;

        cameraSway.localPosition = Vector3.SmoothDamp(cameraSway.localPosition, startPosition, ref positionVelocity, 1f / smoothness);
        cameraSway.localRotation = Quaternion.Lerp(cameraSway.localRotation, Quaternion.identity, Time.deltaTime * smoothness);
    }
}