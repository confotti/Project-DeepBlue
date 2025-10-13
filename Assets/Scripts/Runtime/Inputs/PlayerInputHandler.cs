using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private DefaultInputActions defaultInputActions;
    private InputAction movement;
    private InputAction look;
    private InputAction run;

    public Vector2 Move { get { return movement.ReadValue<Vector2>(); } }
    public Vector2 Look { get { return look.ReadValue<Vector2>(); } }
    public bool Run { get { return run.ReadValue<float>() == 1; } }
    public Action OnInteract;
    public Action OnJump;
    public Action<InputAction.CallbackContext> OnRun;

    private void Awake()
    {
        defaultInputActions = new DefaultInputActions();
    }

    private void OnEnable()
    {
        movement = defaultInputActions.PlayerMovement.Move;
        movement.Enable();

        look = defaultInputActions.PlayerMovement.Look;
        look.Enable();

        run = defaultInputActions.PlayerMovement.Run;
        run.Enable();

        //Subscriptions
        defaultInputActions.PlayerMovement.Interact.Enable();
        defaultInputActions.PlayerMovement.Interact.performed += Interact;
        
        defaultInputActions.PlayerMovement.Jump.Enable();
        defaultInputActions.PlayerMovement.Jump.performed += Jump;      
    }

    private void OnDisable()
    {
        //Unsubscriptions
        defaultInputActions.PlayerMovement.Interact.performed -= Interact;
        defaultInputActions.PlayerMovement.Jump.performed -= Jump;
    }

    private void Interact(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }
}
