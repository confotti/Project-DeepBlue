using UnityEngine;

public class StalkerBehaviour : MonoBehaviour
{
    public StateMachine<StalkerBehaviour> StateMachine;
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
}
