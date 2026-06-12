using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeachHero
{
    public class InputManager : SingleTon<InputManager>
    {
        private InputSystem_Actions inputSystemActions;
        [SerializeField] private bool enableMobileInputs;

        public event Action<Vector2> OnMouseClickDown;
        public event Action<Vector2> OnMouseClickUp;
        public event Action OnEscapePressed;

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
#if UNITY_EDITOR
            if (enableMobileInputs)
            {
                MousePosition = obj.ReadValue<Vector2>();
            }
            else
                MousePosition = Mouse.current.position.ReadValue();
#else
            MousePosition = obj.ReadValue<Vector2>();
#endif
        }

        private void OnClickPerformed(InputAction.CallbackContext obj)
        {
#if UNITY_EDITOR
            if (enableMobileInputs)
            {
                MousePosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else
                MousePosition = Mouse.current.position.ReadValue();
#else
            MousePosition = Touchscreen.current.primaryTouch.position.ReadValue();
#endif
            OnMouseClickDown?.Invoke(MousePosition);
        }

        private void OnClickReleased(InputAction.CallbackContext obj)
        {
#if UNITY_EDITOR
            if (enableMobileInputs)
            {
                MousePosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else
                MousePosition = Mouse.current.position.ReadValue();
#else
            MousePosition = Touchscreen.current.primaryTouch.position.ReadValue();
#endif
            OnMouseClickUp?.Invoke(MousePosition);
        }
    }
}

