using UnityEngine;

public class StalkerStalkState : State<StalkerBehaviour>
{
    [SerializeField] private float _stalkSpeed = 20;
    [SerializeField] private float _stalkPositionLength = 100;

    private Vector3 _targetPos;
    private float _currentStalk;

    public override void PhysicsUpdate()
    {
        obj.Rb.angularVelocity = (_targetPos - obj.transform.position).normalized * _stalkSpeed;
        obj.transform.LookAt(_targetPos);
    }

    public override void LogicUpdate()
    {
        if (obj.IsObservedByPlayer())
        {
            obj.StateMachine.ChangeState(obj.ScaredState);
            return;
        }

        if(Vector3.Distance(obj.transform.position, _targetPos) < 5) _currentStalk -= 0.1f * Time.deltaTime; 

        _targetPos = GetTargetPosition();
        obj.LookAtPoint.transform.position = PlayerMovement.Instance.transform.position;
    }

    private Vector3 GetTargetPosition()
    {
        return PlayerMovement.Instance.transform.position - (PlayerMovement.Instance.transform.forward * _stalkPositionLength * (1 - _currentStalk));
    }
}
