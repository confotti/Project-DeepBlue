using System;
using SaveLoadSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static UnityAction<bool> ToggleLooking;

    private DefaultInputActions defaultInputActions;
    private InputAction movement => defaultInputActions.PlayerMovement.Move;
    private InputAction look => defaultInputActions.PlayerMovement.Look;
    private InputAction run => defaultInputActions.PlayerMovement.Run;
    private InputAction jump => defaultInputActions.PlayerMovement.Jump;
    private InputAction crouch => defaultInputActions.PlayerMovement.Crouch;
    private InputAction itemPrimary => defaultInputActions.PlayerMovement.ItemPrimary;
    private InputAction itemSecondary => defaultInputActions.PlayerMovement.ItemSecondary;

    public Vector2 Move { get { return movement.ReadValue<Vector2>(); } }
    public Vector2 Look { get { return look.ReadValue<Vector2>(); } }
    public bool Run { get { return run.ReadValue<float>() == 1; } }
    public float SwimUp { get { return jump.ReadValue<float>(); } }
    public float SwimDown { get { return crouch.ReadValue<float>(); } }
    public Action OnInteract;
    public Action OnJump;
    public Action OnItemPrimary;
    public Action OnItemSecondary;


    //Hotbar stuff below
    public static Action<int> OnHotbarSelection;
    public static Action<int> OnHotbarChange;

    private void Awake()
    {
        defaultInputActions = new DefaultInputActions();
    }

    private void OnEnable()
    {
        defaultInputActions.PlayerMovement.Enable();

        //movement = defaultInputActions.PlayerMovement.Move;
        movement.Enable();

        //look = defaultInputActions.PlayerMovement.Look;
        look.Enable();
        ToggleLooking += OnToggleLooking;

        //run = defaultInputActions.PlayerMovement.Run;
        run.Enable();

        //Subscriptions
        defaultInputActions.PlayerMovement.Interact.Enable();
        defaultInputActions.PlayerMovement.Interact.performed += Interact;

        //jump = defaultInputActions.PlayerMovement.Jump;
        jump.Enable();
        jump.performed += Jump;

        //crouch = defaultInputActions.PlayerMovement.Crouch;
        crouch.Enable();

        itemPrimary.Enable();
        itemSecondary.Enable();
        itemPrimary.performed += ItemPrimary;
        itemSecondary.performed += ItemSecondary;

        defaultInputActions.Hotbar.Enable();
        defaultInputActions.Hotbar.HotbarSelection.Enable();
        defaultInputActions.Hotbar.HotbarSelection.performed += HotbarSelection;

        defaultInputActions.Hotbar.HotbarNext.performed += HotbarNext;
        defaultInputActions.Hotbar.HotbarPrevious.performed += HotbarPrevious;
    }

    private void OnDisable()
    {
        //Unsubscriptions
        ToggleLooking -= OnToggleLooking;
        defaultInputActions.PlayerMovement.Interact.performed -= Interact;
        defaultInputActions.PlayerMovement.Jump.performed -= Jump;

        defaultInputActions.Hotbar.HotbarSelection.performed -= HotbarSelection;

        defaultInputActions.Hotbar.HotbarNext.performed -= HotbarNext;
        defaultInputActions.Hotbar.HotbarPrevious.performed -= HotbarPrevious;
    }

    private void Interact(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }

    private void ItemPrimary(InputAction.CallbackContext context)
    {
        OnItemPrimary?.Invoke();
    }

    private void ItemSecondary(InputAction.CallbackContext context)
    {
        OnItemSecondary?.Invoke();
    }

    private void OnToggleLooking(bool enabled)
    {
        if (enabled) look.Enable();
        else look.Disable();
    }


    //Hotbar stuff below
    private void HotbarSelection(InputAction.CallbackContext context)
    {
        OnHotbarSelection?.Invoke((int)context.ReadValue<float>());
    }

    private void HotbarNext(InputAction.CallbackContext context)
    {
        OnHotbarChange?.Invoke(1);
    }

    private void HotbarPrevious(InputAction.CallbackContext context)
    {
        OnHotbarChange?.Invoke(-1);
    }


    //Remove these later. 
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
