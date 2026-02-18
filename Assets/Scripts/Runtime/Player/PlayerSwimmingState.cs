using System;
using UnityEngine;

[Serializable]
public class PlayerSwimmingState : State<PlayerMovement>
{
    [SerializeField] private float _baseSwimSpeed = 24;
    private float _swimmingSpeed = 24;
    private float _swimmingFastSpeed = 40;
    [SerializeField] private float _baseSwimmingFastSpeed = 40;
    [SerializeField, Range(0f, 1f)] private float _accelaration = 0.1f;

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        var targetVel = (new Vector3(0, obj.InputHandler.SwimUp - obj.InputHandler.SwimDown, 0) +
                    obj.CameraHead.transform.rotation * new Vector3(obj.InputHandler.Move.x, 0, obj.InputHandler.Move.y)).normalized *
                    (obj.InputHandler.Run ? _swimmingFastSpeed : _swimmingSpeed);

        obj.Rb.linearVelocity += (targetVel - obj.Rb.linearVelocity) * _accelaration;

        /*
        obj.Rb.linearVelocity = (new Vector3(0, obj.InputHandler.SwimUp - obj.InputHandler.SwimDown, 0) +
                    obj.CameraHead.transform.rotation * new Vector3(obj.InputHandler.Move.x, 0, obj.InputHandler.Move.y)).normalized *
                    (obj.InputHandler.Run ? _swimmingFastSpeed : _swimmingSpeed);
        */
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
    }

    public override void Enter()
    {
        base.Enter();
        
        PlayerMovement.Instance.SetUnderwaterParticlesActive(true); 
        obj.Animator.SetBool("IsSwimming", true);
    }

    public override void Exit()
    {
        base.Exit();

        PlayerMovement.Instance.SetUnderwaterParticlesActive(false); 
        obj.Animator.SetBool("IsSwimming", false);
    }

    public void SetSwimSpeedsModifiers(float slowSpeed, float fastSpeed)
    {
        _swimmingSpeed = _baseSwimSpeed + slowSpeed;
        _swimmingFastSpeed = _baseSwimmingFastSpeed + fastSpeed;
    }

    public void SetSwimSpeedsModifiers(float modifier)
    {
        SetSwimSpeedsModifiers(modifier, modifier);
    }
}
