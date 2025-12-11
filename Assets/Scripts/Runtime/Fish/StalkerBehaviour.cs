using UnityEngine;

public class StalkerBehaviour : MonoBehaviour
{
    [SerializeField] private LayerMask _lineOfSightMask;

    public StateMachine<StalkerBehaviour> StateMachine = new();
    [Header("Green Gizmos")] public StalkerWanderState WanderState = new();
    [Header("Red Gizmos")] public StalkerPursuitState PursuitState = new();

    public Rigidbody Rb { get; private set; }
    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        WanderState.Init(this, StateMachine);
        PursuitState.Init(this, StateMachine);

        StateMachine.Initialize(WanderState);
    }

    void Update()
    {
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
        if(Physics.Raycast(transform.position, PlayerMovement.Instance.transform.position - transform.position, out RaycastHit hit, _lineOfSightMask))
        {
            if (hit.collider.CompareTag("Player")) return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, PursuitState.PursuitDetectionRange);

        Gizmos.color = new Color(0.5f, 1f, 0.1f);
        Gizmos.DrawWireSphere(transform.position + transform.forward * WanderState.WanderCircleDistance, WanderState.WanderCircleRadius);
        Gizmos.color = new Color(0.1f, 1f, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * WanderState.AvoidDistance);

    }
}
