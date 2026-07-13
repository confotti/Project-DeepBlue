using System;
using UnityEngine;

[Serializable]
public class StalkerPursuitState : State<StalkerBehaviour>
{
    [SerializeField] private float _pursuitSpeed = 25f;
    public float PursuitDetectionRange = 50f;
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
            obj.StateMachine.ChangeState(obj.StalkState); 
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!PlayerMovement.Instance.IsSwimming)
        {
            obj.StateMachine.ChangeState(obj.WanderState);
            return;
        }

        if (!obj.PlayerInPursuitRange)
        {
            obj.StateMachine.ChangeState(obj.StalkState);
            return;
        }

        if (!obj.PlayerInLineOfSight())
        {
            obj.StateMachine.ChangeState(obj.StalkState);
            return;
        }

        obj.LookAtPoint.position = PlayerMovement.Instance.CameraHead.transform.position;
    } 
}
