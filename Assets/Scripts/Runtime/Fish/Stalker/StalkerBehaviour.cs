using UnityEngine;

public class StalkerBehaviour : MonoBehaviour
{
    [SerializeField] private LayerMask _lineOfSightMask;
    [SerializeField, Range(45, 360)] private float _fieldOfViewInspector;
    private float _fovThreshhold;

    [SerializeField] public Transform LookAtPoint;
    [SerializeField] private Renderer _renderer;

    public Rigidbody Rb { get; private set; }

    public float TimeSinceLastAttack = 100;


    public StateMachine<StalkerBehaviour> StateMachine = new();

#if UNITY_EDITOR
    [Header("Green Gizmos"), SerializeField] private bool _showWanderGizmos = true;
#endif
    public StalkerWanderState WanderState = new();

#if UNITY_EDITOR
    [Header("Red Gizmos"), SerializeField] private bool _showPursuitGizmos = true;
#endif
    public StalkerPursuitState PursuitState = new();
    public StalkerStalkState StalkState = new();
    public StalkerScaredState ScaredState = new();




    private void OnValidate()
    {
        _fovThreshhold = Mathf.Cos(_fieldOfViewInspector * Mathf.Deg2Rad * 0.5f);
    }

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        WanderState.Init(this, StateMachine);
        PursuitState.Init(this, StateMachine);
        StalkState.Init(this, StateMachine);
        ScaredState.Init(this, StateMachine);

        StateMachine.Initialize(StalkState);

        _fovThreshhold = Mathf.Cos(_fieldOfViewInspector * Mathf.Deg2Rad * 0.5f);

    }

    void Update()
    {
        TimeSinceLastAttack += Time.deltaTime;
        StateMachine.CurrentState.LogicUpdate();
    }

    void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    public float DistanceToPlayer => Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position);

    public bool PlayerInPursuitRange => DistanceToPlayer < PursuitState.PursuitDetectionRange;

    public bool PlayerInLineOfSight()
    {
        if (PlayerInFOV() &&
        Physics.Raycast(transform.position, PlayerMovement.Instance.transform.position - transform.position, out RaycastHit hit, _lineOfSightMask))
        {
            if (hit.collider.CompareTag("Player")) return true;
        }

        return false;
    }

    public bool PlayerInFOV()
    {
        return Vector3.Dot(transform.forward, (PlayerMovement.Instance.transform.position - transform.position).normalized) >= _fovThreshhold;
    }

    public bool IsObservedByPlayer()
    {
        /*TODO: Probably make this include raycasts and shit: example (from chatgpt, not looked through)
        
            bool IsTrulyVisible(Camera cam, Renderer rend)
            {
                if (!GeometryUtility.TestPlanesAABB(
                    GeometryUtility.CalculateFrustumPlanes(cam), rend.bounds))
                    return false;

                Vector3 direction = rend.bounds.center - cam.transform.position;
                if (Physics.Raycast(cam.transform.position, direction, out RaycastHit hit))
                    return hit.transform == rend.transform;

                return false;
            }
        */
        return (_renderer.isVisible && Vector3.Dot(PlayerMovement.Instance.CameraHead.transform.forward, transform.position - PlayerMovement.Instance.transform.position) > 0.5f); //0.5 is 60 degrees. 
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
            Gizmos.DrawSphere(LookAtPoint.transform.position, 0.5f);
        }

        if (_showPursuitGizmos)
        {
            //Field of view gizmo
            Gizmos.color = Color.red;

            // Half FOV
            float halfFOV = _fieldOfViewInspector * 0.5f;

            // Boundary directions
            Vector3 leftDir = Quaternion.AngleAxis(-halfFOV, Vector3.up) * t.forward;
            Vector3 rightDir = Quaternion.AngleAxis(halfFOV, Vector3.up) * t.forward;
            Vector3 upDir = Quaternion.AngleAxis(-halfFOV, Vector3.right) * t.forward;
            Vector3 downDir = Quaternion.AngleAxis(halfFOV, Vector3.right) * t.forward;

            Gizmos.DrawLine(t.position, t.position + leftDir * PursuitState.PursuitDetectionRange);
            Gizmos.DrawLine(t.position, t.position + rightDir * PursuitState.PursuitDetectionRange);
            Gizmos.DrawLine(t.position, t.position + upDir * PursuitState.PursuitDetectionRange);
            Gizmos.DrawLine(t.position, t.position + downDir * PursuitState.PursuitDetectionRange);

            // Optional arc visualization
            DrawArc(t.position, t.forward, Vector3.up, _fieldOfViewInspector, PursuitState.PursuitDetectionRange);
            DrawArc(t.position, t.forward, t.right, _fieldOfViewInspector, PursuitState.PursuitDetectionRange);
            DrawArc(t.position + t.rotation * new Vector3(0, 0, Mathf.Cos(_fieldOfViewInspector * 0.5f * Mathf.Deg2Rad) * PursuitState.PursuitDetectionRange),
            t.up, t.forward, 360, PursuitState.PursuitDetectionRange * Mathf.Sin(_fieldOfViewInspector * 0.5f * Mathf.Deg2Rad));
        }
        
    }


    void DrawArc(Vector3 center, Vector3 forward, Vector3 axis, float angle, float radius)
    {
        int segments = 32;
        float step = angle / segments;

        Vector3 prevPoint = center + Quaternion.AngleAxis(-angle / 2f, axis) * forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -angle / 2f + step * i;
            Vector3 nextPoint = center + Quaternion.AngleAxis(currentAngle, axis) * forward * radius;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}
