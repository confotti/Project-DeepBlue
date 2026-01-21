using UnityEngine;

public class StalkerScaredState : State<StalkerBehaviour>
{
    [SerializeField] private float _timeInScaredState = 5;
    private float _scaredTimer = 0;

    public override void Enter()
    {
        _scaredTimer = _timeInScaredState;
    }

    public override void LogicUpdate()
    {
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
}
