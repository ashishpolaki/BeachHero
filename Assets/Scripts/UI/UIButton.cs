using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeachHero
{
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private AudioType buttonAudioType;
        [SerializeField] private Button button;

        [Header("Scale Settings")]
        [Tooltip("Scale when button is pressed")]
        public float pressedScale = 0.9f;

        [Tooltip("Scale when hovered (optional, leave same as 1 for mobile)")]
        public float hoverScale = 1.05f;

        [Tooltip("Time taken for the tween animation")]
        public float tweenDuration = 0.15f;

        [Tooltip("Ease type for animation (Ex: OutBack, OutQuad, etc.)")]
        public Ease tweenEase = Ease.OutBack;

        [Tooltip("Enable hover animation (useful for PC or console)")]
        public bool enableHover = false;
        private Tween _scaleTween;
        public Vector3 _originalScale = new Vector3(1, 1, 1);

        private void Awake()
        {
            if (button != null)
            {
                button.ButtonRegister(PlayAudio);
            }
            transform.localScale = _originalScale;
        }
        private void OnDestroy()
        {
            if (button != null)
            {
                button.ButtonDeRegisterAll();
            }
            _scaleTween?.Kill();
        }

        private void PlayAudio()
        {
            if (buttonAudioType != AudioType.None)
            {
                if (AudioController.GetInstance != null)
                {
                    AudioController.GetInstance.PlaySound(buttonAudioType);
                }
            }
        }
        #region Pointers

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateTo(pressedScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateTo(_originalScale.x);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enableHover) return;

            AnimateTo(hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableHover) return;

            AnimateTo(_originalScale.x);
        }
        #endregion

        private void AnimateTo(float targetScale)
        {
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(targetScale, tweenDuration).SetEase(tweenEase);
        }
    }
}

