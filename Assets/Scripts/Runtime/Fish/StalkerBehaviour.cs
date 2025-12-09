using UnityEngine;

public class StalkerBehaviour : MonoBehaviour
{
    [SerializeField] private LayerMask _lineOfSightMask;

    public StateMachine<StalkerBehaviour> StateMachine = new();
    public StalkerRoamingState RoamingState = new();
    public StalkerPursuitState PursuitState = new();

    public Rigidbody Rb { get; private set; }
    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        RoamingState.Init(this, StateMachine);
        PursuitState.Init(this, StateMachine);

        StateMachine.Initialize(RoamingState);
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
    }
}
