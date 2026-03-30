using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace BeachHero
{
    public class TutorialSkipOverlay : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject backgroundImage;
        private Action OnSkip;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSkip?.Invoke();
            Hide();
        }

        public void Show(Action action)
        {
            backgroundImage.SetActive(true);
            OnSkip = action;
        }

        public void Hide()
        {
            OnSkip = null;
            backgroundImage.SetActive(false);
        }
    }
}
