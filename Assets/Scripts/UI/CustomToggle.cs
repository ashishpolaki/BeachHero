using LitMotion;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeachHero
{
    public class CustomToggle : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform knobRect;
        [SerializeField] private Slider fillSlider;
        [SerializeField] private float moveDuration = 0.25f;
        [SerializeField] private TextMeshProUGUI onText;
        [SerializeField] private TextMeshProUGUI offText;
        [SerializeField] private Color enableTextColor;
        [SerializeField] private Color disableTextColor;

        private bool toggled = false;
        private float knobAnchorX;

        public Action<bool> OnToggleChanged;

        public void OnPointerClick(PointerEventData eventData)
        {
            toggled = !toggled;
            SetToggle(toggled, true);
            AudioController.GetInstance.PlaySound(AudioType.Swipe);
        }

        public void Init(bool value)
        {
            if (knobRect != null)
            {
                knobAnchorX = Math.Abs(knobRect.anchoredPosition.x);
            }
            toggled = value;
            SetToggle(value, false); // Set initial state without animation
        }

        /// <summary>
        /// Sets the toggle state, optionally animating the change.
        /// </summary>
        private void SetToggle(bool value, bool animate)
        {
            toggled = value;

            if (animate)
            {
                //Set slider with move duration
                TweenManager.SetFloat(fillSlider.value, toggled ? 1f : 0f, moveDuration, val => fillSlider.value = val, Ease.OutQuad);
              //  TweenManager.MoveAnchorOnAxis(knobRect, knobRect.anchoredPosition.x, toggled ? -knobAnchorX : knobAnchorX, moveDuration, Ease.Linear, TransformAxis.X);
                OnToggleChanged?.Invoke(toggled);
            }
            else
            {
              //  knobRect.anchoredPosition = new Vector2(toggled ? -knobAnchorX : knobAnchorX, knobRect.anchoredPosition.y);
                fillSlider.value = toggled ? 1f : 0f;
            }

            //Set Color
            if(onText != null && offText != null)
            {
                onText.color = toggled ? enableTextColor : disableTextColor;
                offText.color = !toggled ? enableTextColor : disableTextColor;
            }
        }
    }
}
