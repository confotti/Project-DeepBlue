using System;
using UnityEngine;

[Serializable]
public class PlayerSwimmingState : State<PlayerMovement>
{
    [SerializeField] private float _swimmingSpeed = 20;
    [SerializeField] private float _swimmingFastSpeed = 40;

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        obj.Rb.linearVelocity = (new Vector3(0, obj.InputHandler.SwimUp - obj.InputHandler.SwimDown, 0) +
                obj.CameraHead.transform.rotation * new Vector3(obj.InputHandler.Move.x, 0, obj.InputHandler.Move.y)).normalized *
                (obj.InputHandler.Run ? _swimmingFastSpeed : _swimmingSpeed);
    }
}
