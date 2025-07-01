using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public enum ButtonAnimation
    {
        None,
        ScalePunch,
        Scale,
        ScaleAndRotate,
        ScaleOverShoot,
        ScaleHorizontal,
        SlideInFromLeft,
        SlideInFromRight,
        SlideInFromTop,
        SlideInFromBottom,
        FlipIn,
    }

    [System.Serializable]
    public struct ButtonAnimationData
    {
        public ButtonAnimation Type;
        public Ease Ease;
        public float Duration;
        public float Strength;
        public float Delay; // Delay before the animation starts
        public int Vibration;
        public float Offset;
        public float Elasticity;
    }

    public class UIButtonAudio : MonoBehaviour
    {
        [SerializeField] private ButtonAnimationData openingAnimation;
        [SerializeField] private AudioType buttonAudioType;
        [SerializeField] private Button button;
        [SerializeField] private RectTransform rect;

        private void OnEnable()
        {
            if (button != null)
            {
                button.onClick.AddListener(PlayAudio);
            }
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(PlayAudio);
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

        public void PlayTweenAnimation()
        {
            PlayOpenAnimation(openingAnimation);
        }

        private void PlayOpenAnimation(ButtonAnimationData buttonAnimationData)
        {
            switch (buttonAnimationData.Type)
            {
                case ButtonAnimation.ScalePunch:
                    rect.DOPunchScale(Vector3.one * buttonAnimationData.Strength, buttonAnimationData.Duration, buttonAnimationData.Vibration, buttonAnimationData.Elasticity).SetDelay(buttonAnimationData.Delay);
                    break;
                case ButtonAnimation.Scale:
                    rect.localScale = Vector3.zero;
                    rect.DOScale(Vector3.one, buttonAnimationData.Duration).SetEase(buttonAnimationData.Ease, 1.7f).SetDelay(buttonAnimationData.Delay);
                    break;
                case ButtonAnimation.ScaleAndRotate:
                    rect.localScale = Vector3.zero;
                    rect.localRotation = Quaternion.Euler(0, 0, 90); // Start rotated
                    rect.DOScale(Vector3.one, buttonAnimationData.Duration)
                        .SetEase(buttonAnimationData.Ease) //Outback 
                        .SetDelay(buttonAnimationData.Delay);
                    rect.DOLocalRotate(Vector3.zero, buttonAnimationData.Duration)
                        .SetEase(buttonAnimationData.Ease) //Outback 
                        .SetDelay(buttonAnimationData.Delay);
                    break;
                case ButtonAnimation.ScaleOverShoot:
                    rect.localScale = Vector3.zero;
                    rect.DOScale(Vector3.one, buttonAnimationData.Duration)
                        .SetEase(buttonAnimationData.Ease, 1.7f) // Flash 
                        .SetDelay(buttonAnimationData.Delay);
                    break;
                case ButtonAnimation.ScaleHorizontal:
                    rect.localScale = new Vector3(0, 1, 1);
                    rect.DOScale(Vector3.one, buttonAnimationData.Duration)
                        .SetEase(buttonAnimationData.Ease) //OutBack
                        .SetDelay(buttonAnimationData.Delay);
                    break;
                case ButtonAnimation.SlideInFromLeft:
                    Vector2 originalPos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(-buttonAnimationData.Offset, originalPos.y);
                    rect.DOAnchorPos(originalPos, buttonAnimationData.Duration).SetEase(buttonAnimationData.Ease).SetDelay(buttonAnimationData.Delay);
                    break;
                case ButtonAnimation.SlideInFromRight:
                    originalPos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(buttonAnimationData.Offset, originalPos.y);
                    rect.DOAnchorPos(originalPos, buttonAnimationData.Duration).SetEase(buttonAnimationData.Ease);
                    break;
                case ButtonAnimation.SlideInFromTop:
                    originalPos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(originalPos.x, buttonAnimationData.Offset);
                    rect.DOAnchorPos(originalPos, buttonAnimationData.Duration).SetEase(buttonAnimationData.Ease);
                    break;
                case ButtonAnimation.SlideInFromBottom:
                    originalPos = rect.anchoredPosition;
                    rect.anchoredPosition = new Vector2(originalPos.x, -buttonAnimationData.Offset);
                    rect.DOAnchorPos(originalPos, buttonAnimationData.Duration).SetEase(buttonAnimationData.Ease);
                    break;
                case ButtonAnimation.FlipIn:
                    //Rotates on Y-axis like a card flip
                    rect.localScale = Vector3.zero;
                    rect.localRotation = Quaternion.Euler(0, 180, 0); // Start with the back side facing up
                    rect.DOScale(Vector3.one, buttonAnimationData.Duration).SetEase(Ease.OutBack);
                    rect.DORotate(new Vector3(0, 0, 0), buttonAnimationData.Duration).SetEase(buttonAnimationData.Ease);
                    break;
                default:
                    break;
            }
        }
    }
}

