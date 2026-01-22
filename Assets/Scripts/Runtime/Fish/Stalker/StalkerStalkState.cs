using UnityEngine;

public class StalkerStalkState : State<StalkerBehaviour>
{
    [SerializeField] private float _getToPositionSpeed = 30;
    [SerializeField] private float _stalkSwimSpeed = 10;
    [SerializeField, Range(0, 1)] private float _stalkRate = 0.1f;
    [SerializeField] private float _rangeToEnterPursuit = 10;
    [SerializeField] private float _stalkPositionDistance = 100;

    private Vector3 _targetPos;
    private float _currentStalk;

    public override void PhysicsUpdate()
    {
        if(Vector3.Distance(obj.transform.position, _targetPos) > 5) //Getting into stalkposition
        {
            obj.Rb.angularVelocity = (_targetPos - obj.transform.position).normalized * _getToPositionSpeed;
            obj.transform.LookAt(_targetPos);
        }
        else //Stalking
        {
            _currentStalk = Mathf.Min(_currentStalk + _stalkRate * Time.fixedDeltaTime, 1);
            obj.Rb.angularVelocity = (_targetPos - obj.transform.position).normalized * _stalkSwimSpeed;
            obj.transform.LookAt(_targetPos);

            if (Vector3.Distance(obj.transform.position, PlayerMovement.Instance.transform.position) < _rangeToEnterPursuit)
            {
                obj.StateMachine.ChangeState(obj.PursuitState);
                _currentStalk = 0;
            }
        }
    }

    public override void LogicUpdate()
    {
        if (obj.IsObservedByPlayer())
        {
            obj.StateMachine.ChangeState(obj.ScaredState);
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
