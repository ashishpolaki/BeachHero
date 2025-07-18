using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeachHero
{
    public class CustomToggle : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform knobObject;
        [SerializeField] private Image fillColorImg;
        [SerializeField] private Color defaultColor = Color.gray;
        [SerializeField] private Color toggledColor = Color.green;
        [SerializeField] private float moveDistance = 50f;
        [SerializeField] private float moveDuration = 0.25f;

        private bool toggled = false;
        private float initialX;
        private Vector2 initialPosition;

        public Action<bool> OnToggleChanged;

        public void OnPointerClick(PointerEventData eventData)
        {
            toggled = !toggled;
            SetToggle(toggled, true);

            //if (knobObject != null)
            //{
            //    float targetX = toggled ? initialX + moveDistance : initialX;
            //    knobObject.DOAnchorPosX(targetX, moveDuration).SetEase(Ease.OutQuad);

            //    if (fillColorImg != null)
            //    {
            //        fillColorImg.DOColor(toggled ? toggledColor : defaultColor, moveDuration);
            //    }
            //}
            //OnToggleChanged?.Invoke(toggled); // Notify listeners about the toggle change
        }

        public void Init(bool value)
        {
            if (knobObject != null)
            {
                initialX = knobObject.anchoredPosition.x;
                initialPosition = knobObject.anchoredPosition;
            }
            toggled = value;
            SetToggle(value, false); // Set initial state without animation

            //if (knobObject != null)
            //{
            //    float targetX = toggled ? initialX + moveDistance : initialX;
            //    knobObject.anchoredPosition = new Vector2(targetX, knobObject.anchoredPosition.y);
            //}
            //if (fillColorImg != null)
            //{
            //    fillColorImg.color = toggled ? toggledColor : defaultColor;
            //}
        }

        public void Close()
        {
            knobObject.anchoredPosition = initialPosition; // Reset position if needed
        }

        /// <summary>
        /// Sets the toggle state, optionally animating the change.
        /// </summary>
        private void SetToggle(bool value, bool animate)
        {
            toggled = value;
            float targetX = toggled ? initialX + moveDistance : initialX;

            if (animate)
            {
                knobObject.DOAnchorPosX(targetX, moveDuration).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    if (fillColorImg != null)
                    {
                        fillColorImg.color = toggled ? toggledColor : defaultColor;
                    }
                });
                OnToggleChanged?.Invoke(toggled);
            }
            else
            {
                knobObject.anchoredPosition = new Vector2(targetX, knobObject.anchoredPosition.y);
                fillColorImg.color = toggled ? toggledColor : defaultColor;
            }
        }
    }
}
