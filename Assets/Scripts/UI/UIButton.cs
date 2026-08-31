using UnityEngine;
using UnityEngine.EventSystems;
using LitMotion;
using UnityEngine.UI;

namespace BeachHero
{
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Audio")]
        [SerializeField] private AudioType buttonAudioType;

        [Header("Interaction")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool enableHover = false;

        [Header("Scale Animation")]
        [SerializeField] private Vector3 _originalScale = new Vector3(1, 1, 1);
        [SerializeField] private Vector3 pressedScale = new Vector3(0.9f, 0.9f, 0.9f);
        //   [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float tweenDuration = 0.15f;
        [SerializeField] private Ease pressEase = Ease.OutBack;
        [SerializeField] private Ease releaseEase = Ease.OutBack;

        private TweenHandle scaleHandle;
        [Tooltip("Event triggered when button animation completes")]
        public event System.Action OnButtonReleased;
        public event System.Action OnButtonPressed;

        private bool isInteractable = true;

        #region Unity Methods
        private void Awake()
        {
            transform.localScale = _originalScale;
        }
        private void OnDestroy()
        {
            scaleHandle.Cancel();
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
            if (!isInteractable) return;

            if (UIController.GetInstance.IsScreenTransitioning || UIController.GetInstance.IsInputBlocked)
            {
                return;
            }
            PlayPressAnimation();
            UIController.GetInstance.BlockInput(true);
        }

        private bool IsPointerInside(PointerEventData eventData)
        {
            RectTransform rect = transform as RectTransform;
            if (rect == null)
                return false;

            Camera cam = eventData.pressEventCamera;
            return RectTransformUtility.RectangleContainsScreenPoint(
                rect,
                eventData.position,
                cam
            );
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isInteractable) return;

            if (IsPointerInside(eventData))
            {
                PlayReleaseAnimation();
            }
            else
            {
                CancelPressAnimation();
                UIController.GetInstance.BlockInput(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable || !enableHover) return;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable || !enableHover) return;
        }
        #endregion

        #region Animations
        private void AnimateScale(Vector3 target, Ease ease, System.Action onComplete = null)
        {
            scaleHandle.Cancel();
            scaleHandle = TweenManager.Scale(transform.localScale, target, transform, tweenDuration, ease, onComplete);
        }
        private void CancelPressAnimation()
        {
            AnimateScale(_originalScale, releaseEase);
        }
        public virtual void PlayPressAnimation()
        {
            OnButtonPressed?.Invoke();
            AnimateScale(pressedScale, pressEase);
        }
        public virtual void PlayReleaseAnimation()
        {
            PlayAudio();
            AnimateScale(_originalScale, releaseEase, () =>
            {
                // while the screen is in transition, the buttons action should not be happen.
                if (!UIController.GetInstance.IsScreenTransitioning)
                {
                    OnButtonReleased?.Invoke();
                }
                UIController.GetInstance.BlockInput(false);
            });
        }
        #endregion

        #region Interactability
        public virtual void SetInteractable(bool state)
        {
            isInteractable = state;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isInteractable ? 1f : 0.4f;
                canvasGroup.blocksRaycasts = isInteractable;
                canvasGroup.interactable = isInteractable;
            }
        }
        #endregion
    }
}
