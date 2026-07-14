using UnityEngine;

public class StalkerBehaviour : MonoBehaviour
{
    [SerializeField] public Transform LookAtPoint;
    [SerializeField] private Renderer _renderer;

    public Rigidbody Rb { get; private set; }

    [SerializeField] private LayerMask _lineOfSightMask;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField, Range(45, 360)] private float _fieldOfViewInspector;

    private float _fovThreshold;

    [SerializeField] private float _fearDistance = 400f;
    [SerializeField] private float _attackDistance = 150f;

    public float DistanceToPlayer => Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position);
    public bool PlayerIsTooFarToCare => DistanceToPlayer > _fearDistance;
    public bool PlayerCanScareCreature => DistanceToPlayer <= _fearDistance && DistanceToPlayer > _attackDistance;
    public bool PlayerIsAggressiveRange => DistanceToPlayer <= _attackDistance;
    public bool PlayerInPursuitRange => DistanceToPlayer < PursuitState.PursuitDetectionRange; 

    [SerializeField] private float _rotationSpeed = 5f;

    private Vector3 _lastPosition;
    private Vector3 _currentAvoidance;

    private float _stuckTimer;

    public StateMachine<StalkerBehaviour> StateMachine = new ();

    public StalkerWanderState WanderState = new ();
    public StalkerPursuitState PursuitState = new ();
    public StalkerStalkState StalkState = new ();
    public StalkerScaredState ScaredState = new ();

#if UNITY_EDITOR
    [Header("Green Gizmos"), SerializeField] private bool _showWanderGizmos = true;
    [Header("Red Gizmos"), SerializeField] private bool _showPursuitGizmos = true;
#endif

    [SerializeField] private bool _debugState = true;

    private string _lastState;

    public float TimeSinceLastAttack = 100f; 

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();

        WanderState.Init(this, StateMachine);
        PursuitState.Init(this, StateMachine);
        StalkState.Init(this, StateMachine);
        ScaredState.Init(this, StateMachine);

        StateMachine.Initialize(StalkState);

        UpdateFOV();
    }
    
    private void OnValidate() => UpdateFOV();

    private void Update()
    {
        TimeSinceLastAttack += Time.deltaTime;

        StateMachine.CurrentState.LogicUpdate();

        if (_debugState)
            DebugState();
    }

    private void FixedUpdate() => StateMachine.CurrentState.PhysicsUpdate();

    private void UpdateFOV() => _fovThreshold = Mathf.Cos(_fieldOfViewInspector * Mathf.Deg2Rad * 0.5f);

    public bool PlayerInLineOfSight()
    {
        if (!PlayerInFOV())
            return false;

        Vector3 direction = PlayerMovement.Instance.transform.position - transform.position;

        return Physics.Raycast(transform.position, direction, out RaycastHit hit, _lineOfSightMask) && hit.collider.CompareTag("Player");
    }

    public bool PlayerInFOV()
    {
        return Vector3.Dot(transform.forward, (PlayerMovement.Instance.transform.position - transform.position).normalized) >= _fovThreshold;
    }

    public bool IsStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, _lastPosition);

        _stuckTimer = distanceMoved < 0.5f ? _stuckTimer + Time.deltaTime : 0;

        _lastPosition = transform.position;

        return _stuckTimer > 2f;
    }

    public Vector3 GetSteeredDirection(Vector3 targetDirection, float avoidanceStrength = 3f)
    {
        Vector3 avoidance = Vector3.zero;

        Vector3[] rayDirections =
        {
            transform.forward,
            Quaternion.Euler(0, 60, 0) * transform.forward,
            Quaternion.Euler(0, -60, 0) * transform.forward,
            Quaternion.Euler(45, 0, 0) * transform.forward,
            Quaternion.Euler(-45, 0, 0) * transform.forward,
            Quaternion.Euler(0, 120, 0) * transform.forward,
            Quaternion.Euler(0, -120, 0) * transform.forward
        };

        foreach (Vector3 direction in rayDirections)
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 35f, _obstacleMask))
                avoidance += hit.normal;
        }

        _currentAvoidance = Vector3.Lerp(_currentAvoidance, avoidance, Time.fixedDeltaTime * 3f);

        return (targetDirection + _currentAvoidance * avoidanceStrength).normalized;
    }

    public void MoveCreature(Vector3 direction, float speed)
    {
        if (direction == Vector3.zero)
            return;

        if (IsStuck())
        {
            direction = (-transform.forward + Random.insideUnitSphere).normalized;
            speed *= 1.5f;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), _rotationSpeed * Time.fixedDeltaTime);

        Rb.linearVelocity = direction * speed;
    }

    public bool IsObservedByPlayer()
    {
        return _renderer.isVisible && Vector3.Dot(PlayerMovement.Instance.CameraHead.transform.forward, transform.position - PlayerMovement.Instance.transform.position) > 0.5f;
    }

    private void DebugState()
    {
        string currentState = StateMachine.CurrentState.GetType().Name;

        if (currentState != _lastState)
        {
            Debug.Log("Stalker changed state: " + currentState);
            _lastState = currentState;
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Transform t = transform;

        if (_showWanderGizmos)
        {
            Gizmos.color = new Color(0.5f, 1f, 0.1f);
            Gizmos.DrawWireSphere(t.position + t.forward * WanderState.WanderCircleDistance, WanderState.WanderCircleRadius);

            Gizmos.color = new Color(0.1f, 1f, 0.5f);
            Gizmos.DrawLine(t.position, t.position + t.forward * WanderState.AvoidDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(LookAtPoint.position, 0.5f);
        }

        if (_showPursuitGizmos)
        {
            Gizmos.color = Color.red;

            float halfFOV = _fieldOfViewInspector * 0.5f;

            Vector3 leftDir = Quaternion.AngleAxis(-halfFOV, Vector3.up) * t.forward;
            Vector3 rightDir = Quaternion.AngleAxis(halfFOV, Vector3.up) * t.forward;

            Gizmos.DrawLine(t.position, t.position + leftDir * PursuitState.PursuitDetectionRange);
            Gizmos.DrawLine(t.position, t.position + rightDir * PursuitState.PursuitDetectionRange);

            DrawArc(t.position, t.forward, Vector3.up, _fieldOfViewInspector, PursuitState.PursuitDetectionRange);
        }
    }

    private void DrawArc(Vector3 center, Vector3 forward, Vector3 axis, float angle, float radius)
    {
        int segments = 32;
        float step = angle / segments;

        Vector3 previous = center + Quaternion.AngleAxis(-angle / 2f, axis) * forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -angle / 2f + step * i;
            Vector3 next = center + Quaternion.AngleAxis(currentAngle, axis) * forward * radius;

            Gizmos.DrawLine(previous, next);

            previous = next;
        }
    }

#endif
} 