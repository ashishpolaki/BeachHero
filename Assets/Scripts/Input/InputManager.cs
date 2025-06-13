using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeachHero
{
    public class InputManager : MonoBehaviour
    {
        private InputSystem_Actions inputSystemActions;

        public event Action<Vector2> OnMouseClickDown;
        public event Action<Vector2> OnMouseClickUp;
        public event Action OnEscapePressed;

       // public static Vector3 MousePosition => Mouse.current.position.ReadValue();
        public static Vector3 MousePosition { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            inputSystemActions = new InputSystem_Actions();
            inputSystemActions.Game.Enable();

            inputSystemActions.Game.Click.performed += OnClickPerformed;
            inputSystemActions.Game.Release.performed += OnClickReleased;
            inputSystemActions.Game.TouchPosition.performed += OnTouchPOsition;
            inputSystemActions.Game.Escape.performed += OnEscape;
        }

        private void OnEscape(InputAction.CallbackContext obj)
        {
            OnEscapePressed?.Invoke();
        }

        private void OnTouchPOsition(InputAction.CallbackContext obj)
        {
            MousePosition = inputSystemActions.Game.TouchPosition.ReadValue<Vector2>();
        }

        private void OnClickPerformed(InputAction.CallbackContext obj)
        {
            MousePosition = inputSystemActions.Game.TouchPosition.ReadValue<Vector2>();
            OnMouseClickDown?.Invoke(MousePosition);
        }

        private void OnClickReleased(InputAction.CallbackContext obj)
        {
            MousePosition = inputSystemActions.Game.TouchPosition.ReadValue<Vector2>();
            OnMouseClickUp?.Invoke(MousePosition);
        }

        private void OnDestroy()
        {
            inputSystemActions.Game.Click.performed -= OnClickPerformed;
            inputSystemActions.Game.Click.canceled -= OnClickReleased;
            inputSystemActions.Game.TouchPosition.performed -= OnTouchPOsition;

            inputSystemActions.Game.Disable();
        }
    }
}

