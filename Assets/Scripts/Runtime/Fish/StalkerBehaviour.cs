using UnityEngine;

public class StalkerBehaviour : MonoBehaviour
{
    public StateMachine<StalkerBehaviour> StateMachine;
    public StalkerRoamingState RoamingState = new();
    public StalkerPursuitState PursuitState = new();
    [SerializeField] private float speed = 2;

    private Rigidbody rb;
    private void Awake()
    {
        RoamingState.Init(this, StateMachine);
        PursuitState.Init(this, StateMachine);

        StateMachine.Initialize(RoamingState);
    }

    void Update()
    {
        var a = transform.position;
        a += transform.forward * speed * Time.deltaTime;
        transform.position = a;
    }
}
