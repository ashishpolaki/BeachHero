using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeachHero
{
    public class InputManager : SingleTon<InputManager>
    {
        private InputSystem_Actions inputSystemActions;

        public event Action<Vector2> OnMouseClickDown;
        public event Action<Vector2> OnMouseClickUp;
        public event Action OnEscapePressed;

        // public static Vector3 MousePosition => Mouse.current.position.ReadValue();
        public static Vector3 MousePosition { get; private set; }

        private void Awake()
        {
            inputSystemActions = new InputSystem_Actions();
        }
        void OnEnable()
        {
            inputSystemActions.Game.Enable();

            inputSystemActions.Game.Click.performed += OnClickPerformed;
            inputSystemActions.Game.Release.performed += OnClickReleased;
            inputSystemActions.Game.TouchPosition.performed += OnTouchPosition;
            inputSystemActions.Game.Escape.performed += OnEscape;
        }
        void OnDisable()
        {
            inputSystemActions.Game.Click.performed -= OnClickPerformed;
            inputSystemActions.Game.Release.performed -= OnClickReleased;
            inputSystemActions.Game.TouchPosition.performed -= OnTouchPosition;
            inputSystemActions.Game.Escape.performed -= OnEscape;

            inputSystemActions.Game.Disable();
        }
        private void OnEscape(InputAction.CallbackContext obj)
        {
            OnEscapePressed?.Invoke();
        }
        private void OnTouchPosition(InputAction.CallbackContext obj)
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
    }
}

