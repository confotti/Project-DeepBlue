using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //TODO: Rework the whole access to player thing later. 
    public static PlayerMovement Instance;

    public StateMachine<PlayerMovement> StateMachine = new();
    [SerializeField] public PlayerStandingState StandingState = new();
    [SerializeField] public PlayerSwimmingState SwimmingState = new();

    [SerializeField] private float _mouseSensitivity = 0.2f;

    [SerializeField] private bool StartStanding = false;

    private Vector2 _rotation;
    private float _lookYMax = 90;

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

        neckComparedToHead = transform.InverseTransformDirection(neckBone.transform.position) - transform.InverseTransformDirection(CameraHead.transform.position);

        InputHandler = GetComponent<PlayerInputHandler>();
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<CapsuleCollider>();

        StandingState.Init(this, StateMachine);
        SwimmingState.Init(this, StateMachine);
        StateMachine.Initialize(StartStanding ? StandingState : SwimmingState);
    }

    private void OnEnable()
    {
        _rotation.x = CameraHead.transform.rotation.eulerAngles.y;
        _rotation.y = CameraHead.transform.rotation.eulerAngles.x;
    }

    void OnDestroy()
    {
        StateMachine.CurrentState.Exit();
    }

    private void Update()
    {
        Shader.SetGlobalVector("_Player", transform.position);
        CameraMovement();
        StateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();

        _animator.SetFloat("MoveX", Mathf.Lerp(_animator.GetFloat("MoveX"), InputHandler.Move.x, Time.deltaTime));
        _animator.SetFloat("MoveY", Mathf.Lerp(_animator.GetFloat("MoveY"), InputHandler.Move.y, Time.deltaTime));

        playerModel.transform.position = playerModel.transform.position - (neckBone.transform.position - (CameraHead.transform.position + transform.rotation * neckComparedToHead));
    }

    private void CameraMovement()
    {
        _rotation.x += InputHandler.Look.x * _mouseSensitivity;
        _rotation.y += InputHandler.Look.y * _mouseSensitivity;
        _rotation.y = Mathf.Clamp(_rotation.y, -_lookYMax, _lookYMax);
        var xQuat = Quaternion.AngleAxis(_rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(_rotation.y, Vector3.left);
        CameraHead.transform.rotation = xQuat * yQuat;

        transform.rotation = xQuat;
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
