using System;
using UnityEngine;

[Serializable]
public class StalkerPursuitState : State<StalkerBehaviour>
{
    [SerializeField] private float _pursuitSpeed;

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        //obj.Rb.linearVelocity = 
    }
}
