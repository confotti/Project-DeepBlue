using System;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[Serializable]
public class StalkerWanderState : State<StalkerBehaviour>
{
    [SerializeField] private float _wanderSpeed = 15f;
    [SerializeField] private float _turnSpeed = 2f;

    [SerializeField] private float _wanderCircleDistance = 2f;
    [SerializeField] private float _wanderCircleRadius = 1f;
    [SerializeField] private float _wanderJitter = 0.5f;

    [SerializeField] private float _avoidDistance = 20f;

    private Vector3 _wanderTarget;

    public float WanderCircleDistance => _wanderCircleDistance;
    public float WanderCircleRadius => _wanderCircleRadius;
    public float AvoidDistance => _avoidDistance;

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (obj.PlayerInPursuitRange && obj.PlayerInLineOfSight())
            obj.StateMachine.ChangeState(obj.PursuitState);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        //Movement thingys here
        Wander();
        AvoidObstacles();
        obj.Rb.linearVelocity = obj.transform.forward * _wanderSpeed;

        obj.LookAtPoint.position = Vector3.Lerp(obj.LookAtPoint.position, obj.transform.position + obj.transform.forward * _wanderCircleDistance + _wanderTarget, 0.1f);
        
    }

    private void Wander()
    {
        // Add slight random jitter to the wander target
        _wanderTarget += new Vector3(
            Random.Range(-_wanderJitter, _wanderJitter),
            Random.Range(-_wanderJitter, _wanderJitter),
            Random.Range(-_wanderJitter, _wanderJitter)
        );

        _wanderTarget.y = Mathf.Clamp(_wanderTarget.y, -_wanderCircleRadius * 0.95f, _wanderCircleRadius * 0.95f);
        _wanderTarget = _wanderTarget.normalized * _wanderCircleRadius;

        // Project circle in front of fish
        Vector3 targetWorldPos = obj.transform.position + obj.transform.forward * _wanderCircleDistance + _wanderTarget;

        // Turn toward that target smoothly
        Vector3 direction = (targetWorldPos - obj.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        obj.transform.rotation = Quaternion.Slerp(
            obj.transform.rotation,
            targetRotation,
            _turnSpeed
        );
    }

    /*
    private void KeepInsideBounds()
    {
        Vector3 offset = obj.transform.position - center;

        if (offset.magnitude > radius)
        {
            // Steer back towards center
            Vector3 direction = (center - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                turnSpeed * Time.deltaTime
            );
        }
    }
    */

    void AvoidObstacles()
    {
        if (Physics.Raycast(obj.transform.position, obj.transform.forward, out RaycastHit hit, _avoidDistance))
        {
            Vector3 avoidDir = Vector3.Reflect(obj.transform.forward, hit.normal);
            _wanderTarget += avoidDir * 0.2f;
            Quaternion avoidRot = Quaternion.LookRotation(avoidDir);

            obj.transform.rotation = Quaternion.Slerp(
                obj.transform.rotation,
                avoidRot,
                _turnSpeed
            );
        }
    }
}