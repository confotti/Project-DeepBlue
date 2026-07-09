using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //TODO: Rework the whole access to player thing later. 
    public static PlayerMovement Instance;

    public StateMachine<PlayerMovement> StateMachine = new();
    [SerializeField] public PlayerStandingState StandingState = new();
    [SerializeField] public PlayerSwimmingState SwimmingState = new();
    [SerializeField] private GameObject underwaterParticles; 

    [SerializeField] private float _mouseSensitivity = 0.2f;

    [SerializeField] private bool StartStanding = false;

    private Vector2 _targetRotation;
    private Vector2 _currentRotation;

    private const float _lookYMax = 80;

    public bool IsSwimming => StateMachine.CurrentState == SwimmingState;

    //References
    public PlayerInputHandler InputHandler { get; private set; }
    public Rigidbody Rb { get; private set; }
    public CapsuleCollider Col { get; private set; }

    [SerializeField] public GameObject CameraHead;
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject neckBone;

    private Vector3 neckComparedToHead;

    private void Awake()
    {
        Instance = this;

        neckComparedToHead = transform.InverseTransformDirection(neckBone.transform.position)
                           - transform.InverseTransformDirection(CameraHead.transform.position);

        InputHandler = GetComponent<PlayerInputHandler>();
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<CapsuleCollider>();

        StandingState.Init(this, StateMachine);
        SwimmingState.Init(this, StateMachine);

        StateMachine.Initialize(StartStanding ? StandingState : SwimmingState);
    }

    private void OnEnable()
    {
        _targetRotation.x = transform.eulerAngles.y;
        _targetRotation.y = CameraHead.transform.localEulerAngles.x;

        if (_targetRotation.y > 180f)
            _targetRotation.y -= 360f;

        _currentRotation = _targetRotation;
    }

    private void OnDestroy()
    {
        StateMachine.CurrentState.Exit();
    }

    private void Update()
    {
        Shader.SetGlobalVector("_Player", transform.position);

        StateMachine.CurrentState.LogicUpdate();
    }

    private void LateUpdate()
    {
        CameraMovement();

        playerModel.transform.position = playerModel.transform.position -
            (neckBone.transform.position -
            (CameraHead.transform.position + transform.rotation * neckComparedToHead));
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();

        Quaternion bodyRotation = Quaternion.AngleAxis(_currentRotation.x, Vector3.up);
        Rb.MoveRotation(bodyRotation);

        _animator.SetFloat("MoveX",
            Mathf.Lerp(_animator.GetFloat("MoveX"), InputHandler.Move.x, Time.deltaTime));

        _animator.SetFloat("MoveY",
            Mathf.Lerp(_animator.GetFloat("MoveY"), InputHandler.Move.y, Time.deltaTime));
    }

    private void CameraMovement()
    {
        _targetRotation.x += InputHandler.Look.x * _mouseSensitivity;
        _targetRotation.y += InputHandler.Look.y * _mouseSensitivity;

        _targetRotation.y = Mathf.Clamp(
            _targetRotation.y,
            -_lookYMax,
            _lookYMax);

        _currentRotation = _targetRotation;

        Quaternion bodyRotation = Quaternion.AngleAxis(
            _currentRotation.x,
            Vector3.up);

        Quaternion headRotation = Quaternion.AngleAxis(
            _currentRotation.y,
            Vector3.left);

        CameraHead.transform.rotation = bodyRotation * headRotation;
    }

    public void SetUnderwaterParticlesActive(bool active)
    {
        if (underwaterParticles == null)
            return;

        underwaterParticles.SetActive(active);
    } 

    /*
                [System.Diagnostics.Conditional("UNITY_EDITOR")]
                private void OnDrawGizmosSelected()
                {
                    Col = GetComponent<CapsuleCollider>();
                    var a = Col.bounds.center;
                    a.y = Col.bounds.min.y;

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(a, a + Vector3.down * rideHeight);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(a + Vector3.down * rideHeight, a + Vector3.down * rideHeight * 1.5f);

                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(_hitPosition, 0.5f);
                }

                */
} 
