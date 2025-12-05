using System;
using UnityEngine;

[Serializable]
public class StalkerRoamingState : State<StalkerBehaviour>
{
    [SerializeField] private float _RoamingSpeed;

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (obj.PlayerInPursuitRange && obj.PlayerInLineOfSight())
            obj.StateMachine.ChangeState(obj.PursuitState);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        //Movement thingys here
    }
}