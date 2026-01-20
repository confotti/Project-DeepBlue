using UnityEngine;

public class StalkerStalkState : State<StalkerBehaviour>
{
    [SerializeField] private float _stalkPositionLength = 100;

    private Vector3 _targetPos;
    private float _currentStalk;

    public override void PhysicsUpdate()
    {
        obj.Rb.angularVelocity = _targetPos - obj.transform.position;
        obj.transform.LookAt(_targetPos);
    }

    public override void LogicUpdate()
    {
        _targetPos = GetTargetPosition();
        obj.LookAtPoint.transform.position = PlayerMovement.Instance.transform.position;
    }

    private Vector3 GetTargetPosition()
    {
        return PlayerMovement.Instance.transform.position - (PlayerMovement.Instance.transform.forward * _stalkPositionLength * (1 - _currentStalk));
    }
}
