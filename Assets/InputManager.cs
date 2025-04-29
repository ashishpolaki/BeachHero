using System;
using Bokka;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions inputSystemActions;

    public event Action<Vector2> OnMouseClickDown;

    public event Action<Vector2> OnMouseClickUp;

    public static Vector3 MousePosition => Mouse.current.position.ReadValue();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputSystemActions = new InputSystem_Actions();
        inputSystemActions.Game.Enable();

        inputSystemActions.Game.Click.performed += OnClickPerformed;
        inputSystemActions.Game.Release.performed += OnClickReleased;
    }


    private void OnClickPerformed(InputAction.CallbackContext obj)
    {
        OnMouseClickDown?.Invoke(inputSystemActions.Game.Point.ReadValue<Vector2>());
    }

    private void OnClickReleased(InputAction.CallbackContext obj)
    {
        $"Release".Log();
        OnMouseClickUp?.Invoke(inputSystemActions.Game.Point.ReadValue<Vector2>());
    }

    private void OnDestroy()
    {
        inputSystemActions.Game.Click.performed -= OnClickPerformed;
        inputSystemActions.Game.Click.canceled -= OnClickReleased;

        inputSystemActions.Game.Disable();
    }
}
