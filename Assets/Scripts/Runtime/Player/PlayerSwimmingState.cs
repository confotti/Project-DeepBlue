using System;
using UnityEngine;

[Serializable]
public class PlayerSwimmingState : State<PlayerMovement>
{
    [SerializeField] private float _swimmingSpeed = 20;
    [SerializeField] private float _swimmingFastSpeed = 40;
    [SerializeField, Range(0f, 1f)] private float _accelaration = 0.1f;

    public InventoryItemData finsItemData;

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
        
        if(obj.GetComponent<PlayerInventoryHolder>().InventorySystem.ContainsItem(finsItemData, out var ab)) 
        {
            _swimmingSpeed = 28; 
            _swimmingFastSpeed = 45;
        }
    }

    public override void Enter()
    {
        base.Enter();

        obj.Animator.SetBool("IsSwimming", true);
    }

    public override void Exit()
    {
        base.Exit();

        obj.Animator.SetBool("IsSwimming", false);
    }
}
