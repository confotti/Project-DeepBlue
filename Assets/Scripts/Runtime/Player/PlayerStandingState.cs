using System;
using UnityEngine;

[Serializable]
public class PlayerStandingState : State<PlayerMovement>
{
    [SerializeField] private float _walkingSpeed = 20;
    [SerializeField] private float _runningSpeed = 40;
    [SerializeField] private float _gravity = 70;
    [SerializeField] private float _jumpPower = 40;

    [SerializeField] private float _rideHeight = 3;
    [SerializeField] private float _rideSpringStrength = 500;
    [SerializeField] private float _rideSpringDamper = 40;
    [SerializeField] private LayerMask springLayerMask;

    private bool shouldSpring = true;
    private bool grounded = false;

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        Movement();
    }

    public override void Enter()
    {
        base.Enter();

        obj.InputHandler.OnJump += OnJump;
        obj.Animator.SetBool("IsStanding", true);
    }

    public override void Exit()
    {
        base.Exit();

        obj.InputHandler.OnJump -= OnJump;
        obj.Animator.SetBool("IsStanding", false);
    }

    private void Movement()
    {
        var move = obj.transform.rotation * new Vector3(obj.InputHandler.Move.x, 0, obj.InputHandler.Move.y);
        move.y = 0;
        move = move.normalized * (obj.InputHandler.Run ? _runningSpeed : _walkingSpeed);
        move.y = obj.Rb.linearVelocity.y - _gravity * Time.fixedDeltaTime;
        obj.Rb.linearVelocity = move;

        if (!shouldSpring)
        {
            grounded = false;
            if (obj.Rb.linearVelocity.y < 0.2f) shouldSpring = true;
            else return;
        }

        RaycastHit hit;
        var a = obj.Col.bounds.center;
        a.y = obj.Col.bounds.min.y + obj.Col.radius * obj.transform.lossyScale.y;
        if (Physics.SphereCast(a, obj.Col.radius * obj.transform.lossyScale.y * 0.9f, Vector3.down, out hit, _rideHeight * 1.5f, springLayerMask))
        {
            grounded = true;
            springThing(hit);
        }
        else grounded = false;

        //if (Physics.Raycast(a, Vector3.down, out hit, rideHeight*2, ~0))
    }

    private void springThing(RaycastHit hit)
    {
        Vector3 vel = obj.Rb.linearVelocity;
        Vector3 rayDir = Vector3.down;

        Vector3 otherVel = Vector3.zero;
        Rigidbody hitBody = hit.rigidbody;
        if (hitBody != null)
        {
            otherVel = hitBody.linearVelocity;
        }

        float rayDirVel = Vector3.Dot(rayDir, vel);
        float otherDirVel = Vector3.Dot(rayDir, otherVel);

        float relVel = rayDirVel - otherDirVel;

        float x = hit.distance - _rideHeight;

        float springForce = (x * _rideSpringStrength) - (relVel * _rideSpringDamper);

        obj.Rb.AddForce(rayDir * springForce);

        //If we want to impact what we're standing on
        /*
        if(hitBody != null)
        {
            hitBody.AddForceAtPosition(rayDir * -springForce, hit.point);
        }
        */
    }

    private void OnJump()
    {
        if (!grounded) return;

        var move = obj.Rb.linearVelocity;
        move.y = _jumpPower;
        obj.Rb.linearVelocity = move;

        shouldSpring = false;
    }
}
