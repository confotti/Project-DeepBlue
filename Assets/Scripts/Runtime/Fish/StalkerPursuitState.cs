using System;
using UnityEngine;

[Serializable]
public class StalkerPursuitState : State<StalkerBehaviour>
{
    [SerializeField] private float _pursuitSpeed = 25f;
    public float PursuitDetectionRange = 70f;

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        obj.Rb.linearVelocity = (PlayerMovement.Instance.transform.position - obj.transform.position).normalized * _pursuitSpeed;
        obj.transform.LookAt(PlayerMovement.Instance.transform);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        //TODO: Probably create a state where it goes to player last seen position here instead. 
        if (!obj.PlayerInPursuitRange || !obj.PlayerInLineOfSight())
            obj.StateMachine.ChangeState(obj.WanderState);
    }
}
