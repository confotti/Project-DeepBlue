using UnityEngine;

public class FlashlightMovement : MonoBehaviour
{
    [SerializeField] private Transform _camera;

    [Header("Movement")]
    [SerializeField] private float _yawSmoothness = 8f;
    [SerializeField] private float _pitchSmoothness = 15f;

    [Header("Weight")]
    [SerializeField] private float _yawLagAmount = 0.5f;


    private float _currentYaw;
    private float _currentPitch;

    private float _yawVelocity;
    private float _pitchVelocity;

    private float _lastCameraYaw;


    private void Start()
    {
        Vector3 angles = transform.eulerAngles;

        _currentYaw = angles.y;
        _currentPitch = angles.x;

        _lastCameraYaw = _camera.eulerAngles.y;
    }


    private void LateUpdate()
    {
        Vector3 target = _camera.eulerAngles;


        float yawDifference = Mathf.DeltaAngle(
            _lastCameraYaw,
            target.y
        );


        // Create temporary lag while turning
        float targetYaw = target.y - yawDifference * _yawLagAmount;


        _currentYaw = Mathf.SmoothDampAngle(
            _currentYaw,
            targetYaw,
            ref _yawVelocity,
            1f / _yawSmoothness
        );


        _currentPitch = Mathf.SmoothDampAngle(
            _currentPitch,
            target.x,
            ref _pitchVelocity,
            1f / _pitchSmoothness
        );


        transform.rotation = Quaternion.Euler(
            _currentPitch,
            _currentYaw,
            0
        );


        _lastCameraYaw = target.y;
    }
} 