using System;
using UnityEngine;

[Serializable]
public class StalkerScaredState : StalkerWanderState
{
    [Header("Stalker things")]
    [SerializeField] private float _timeInScaredState = 5;
    private float _scaredTimer = 0;

    public override void Enter()
    {
        _scaredTimer = _timeInScaredState;
    }

    public override void LogicUpdate()
    {
        Debug.Log(Vector3.Dot(obj.transform.forward, (PlayerMovement.Instance.transform.position - obj.transform.position).normalized));
        if (obj.IsObservedByPlayer())
        {
            _scaredTimer = _timeInScaredState;
        }
        if(_scaredTimer <= 0)
        {
            obj.StateMachine.ChangeState(obj.StalkState);
            return; 
        }
        _scaredTimer -= Time.deltaTime;
            
    }
     
    public override void PhysicsUpdate()
    {
        Wander();
        if (Vector3.Dot(obj.transform.forward, (PlayerMovement.Instance.transform.position - obj.transform.position).normalized) > -0.18f)
        {
            AvoidPlayer();
            Debug.Log("Avoiding stalk");
        }
        AvoidObstacles();

        obj.Rb.linearVelocity = obj.transform.forward * _wanderSpeed;
    }
}
