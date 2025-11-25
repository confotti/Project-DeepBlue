using System;
using SaveLoadSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static UnityAction<bool> ToggleLooking;

    private DefaultInputActions defaultInputActions;
    private InputAction movement;
    private InputAction look;
    private InputAction run;
    private InputAction jump;
    private InputAction crouch;

    public Vector2 Move { get { return movement.ReadValue<Vector2>(); } }
    public Vector2 Look { get { return look.ReadValue<Vector2>(); } }
    public bool Run { get { return run.ReadValue<float>() == 1; } }
    public float SwimUp { get { return jump.ReadValue<float>(); } }
    public float SwimDown { get { return crouch.ReadValue<float>(); } }
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
        ToggleLooking += OnToggleLooking;

        run = defaultInputActions.PlayerMovement.Run;
        run.Enable();

        //Subscriptions
        defaultInputActions.PlayerMovement.Interact.Enable();
        defaultInputActions.PlayerMovement.Interact.performed += Interact;

        jump = defaultInputActions.PlayerMovement.Jump;
        jump.Enable();
        jump.performed += Jump;

        crouch = defaultInputActions.PlayerMovement.Crouch;
        crouch.Enable();
    }

    private void OnDisable()
    {
        //Unsubscriptions
        ToggleLooking -= OnToggleLooking;
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

    private void OnToggleLooking(bool enabled)
    {
        if (enabled) look.Enable();
        else look.Disable();
    }


    [ContextMenu("Save")]
    private void OnSave()
    {
        SaveLoad.Save();
    }

    [ContextMenu("Load")]
    private void OnLoad()
    {
        SaveLoad.Load();
    }
}
