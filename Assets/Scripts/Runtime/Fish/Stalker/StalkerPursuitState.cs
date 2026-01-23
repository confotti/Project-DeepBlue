using System;
using UnityEngine;

[Serializable]
public class StalkerPursuitState : State<StalkerBehaviour>
{
    [SerializeField] private float _pursuitSpeed = 25f;
    public float PursuitDetectionRange = 70f;
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private int _attackDamage = 20;

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        obj.Rb.linearVelocity = (PlayerMovement.Instance.transform.position - obj.transform.position).normalized * _pursuitSpeed;
        obj.transform.LookAt(PlayerMovement.Instance.transform);

        if (obj.DistanceToPlayer < _attackRange)
        {
            PlayerMovement.Instance.GetComponent<PlayerStats>().ChangeHealth(-_attackDamage);
            obj.TimeSinceLastAttack = 0;
            obj.StateMachine.ChangeState(obj.WanderState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        //TODO: Probably create a state where it goes to player last seen position here instead. 
        if (!obj.PlayerInPursuitRange || !obj.PlayerInLineOfSight() || !PlayerMovement.Instance.IsSwimming)
        {
            obj.StateMachine.ChangeState(obj.WanderState);
            return;
        }

        obj.LookAtPoint.position = PlayerMovement.Instance.CameraHead.transform.position;
    }
}
