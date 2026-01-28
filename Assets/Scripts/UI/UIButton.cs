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
        [SerializeField] private bool enableHover = false;

        [Header("Scale Settings")]
        [Tooltip("Scale when button is pressed")]
        public float pressedScale = 0.9f;

        [Tooltip("Scale when hovered (optional, leave same as 1 for mobile)")]
        public float hoverScale = 1.05f;

        [Tooltip("Time taken for the tween animation")]
        public float tweenDuration = 0.15f;

        [SerializeField] private Ease pressEase = Ease.OutBack;
        [SerializeField] private Ease releaseEase = Ease.OutBack;

        private Tween _scaleTween;
        public Vector3 _originalScale = new Vector3(1, 1, 1);
        [Tooltip("Event triggered when button animation completes")]
        public event System.Action OnButtonReleased;

        #region Unity Methods
        private void Awake()
        {
            transform.localScale = _originalScale;
        }
        private void OnDestroy()
        {
            _scaleTween?.Kill();
        }
        #endregion

        #region Audio
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
        #endregion

        #region Pointers
        public void OnPointerDown(PointerEventData eventData)
        {
            PlayPressAnimation();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PlayReleaseAnimation();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!enableHover) return;
            // AnimateTo(hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableHover) return;
            // AnimateTo(_originalScale.x);
        }
        #endregion

        #region Animations
        private void PlayPressAnimation()
        {
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(pressedScale, tweenDuration).SetEase(pressEase);
        }

        private void PlayReleaseAnimation()
        {
            _scaleTween?.Kill();
            PlayAudio();
            _scaleTween = transform.DOScale(_originalScale.x, tweenDuration).SetEase(releaseEase)
                .OnComplete(() =>
                {
                    OnButtonReleased?.Invoke();
                });
        }
        #endregion
    }
}

