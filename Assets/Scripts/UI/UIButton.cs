using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;

namespace BeachHero
{
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private AudioType buttonAudioType;
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

        private MotionHandle _scaleHandle;
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
            _scaleHandle.TryCancel();
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
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!enableHover) return;
        }
        #endregion

        #region Animations
        public virtual void PlayPressAnimation()
        {
            _scaleHandle.TryCancel();
            _scaleHandle = LMotion.Create(transform.localScale, Vector3.one * pressedScale, tweenDuration)
                .WithEase(pressEase)
                .BindToLocalScale(transform);
        }

        public virtual void PlayReleaseAnimation()
        {
            _scaleHandle.TryCancel();
            PlayAudio();
            _scaleHandle = LMotion.Create(transform.localScale, _originalScale, tweenDuration)
                .WithEase(releaseEase)
                .WithOnComplete(() =>
                {
                    OnButtonReleased?.Invoke();
                })
                .BindToLocalScale(transform);
        }
        #endregion
    }
}

