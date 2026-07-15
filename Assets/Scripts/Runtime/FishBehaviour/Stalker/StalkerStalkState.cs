using System;
using UnityEngine;

[Serializable]
public class StalkerStalkState : State<StalkerBehaviour>
{
    [SerializeField] private float _getToPositionSpeed = 30;
    [SerializeField] private float _stalkSwimSpeed = 10;
    [SerializeField, Range(0, 1)] private float _gainStalkRate = 0.1f;
    [SerializeField, Range(0, 0.2f)] private float _loseStalkOutsideStalkStateRate = 0.01f;
    [SerializeField] private float _rangeToEnterPursuit = 10;
    [SerializeField] private float _stalkPositionDistance = 100;

    private Vector3 _targetPos;
    private float _currentStalk;

    private float _timeWhenLeftStalk;
    private float _targetUpdateTimer;

    public override void Enter()
    {
        base.Enter();

        _currentStalk = Mathf.Max(0, _currentStalk - (Time.time - _timeWhenLeftStalk) * _loseStalkOutsideStalkStateRate);
    }

    public override void Exit()
    {
        base.Exit();

        _timeWhenLeftStalk = Time.time;
    }

    public override void PhysicsUpdate()
    {
        if (Vector3.Distance(obj.transform.position, _targetPos) > 15)
        {
            Vector3 direction = (_targetPos - obj.transform.position).normalized;

            Vector3 steeredDirection =
                obj.GetSteeredDirection(direction, 8f);

            obj.MoveCreature(
                steeredDirection,
                _getToPositionSpeed
            );
        }
        else
        {
            _currentStalk = Mathf.Min(
                _currentStalk + _gainStalkRate * Time.fixedDeltaTime,
                0.8f 
            );

            Vector3 direction = (_targetPos - obj.transform.position).normalized; 

            obj.MoveCreature(
                direction,
                Mathf.Lerp(2f, _stalkSwimSpeed,
                Vector3.Distance(obj.transform.position, _targetPos) / 20f) 
            );
        } 
    }

    public override void LogicUpdate()
    {
        if (obj.PlayerCanScareCreature && obj.IsObservedByPlayer())
        {
            obj.StateMachine.ChangeState(obj.ScaredState);
            return;
        }

        if (obj.PlayerIsAggressiveRange)
        {
            obj.StateMachine.ChangeState(obj.PursuitState);
            return;
        } 
        if (!PlayerMovement.Instance.IsSwimming)
        {
            obj.StateMachine.ChangeState(obj.WanderState);
            return;
        }

        _targetPos = GetTargetPosition(); 

        obj.LookAtPoint.transform.position = PlayerMovement.Instance.transform.position; 
    }

    private Vector3 GetTargetPosition()
    {
        return PlayerMovement.Instance.transform.position - (PlayerMovement.Instance.transform.forward * _stalkPositionDistance * (1 - _currentStalk));
    }
}
