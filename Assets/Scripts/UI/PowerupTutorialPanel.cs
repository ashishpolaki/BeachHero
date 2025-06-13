using DG.Tweening;
using UnityEngine;

namespace BeachHero
{
    public class PowerupTutorialPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform tutorialHandRect;
        [SerializeField] private RectTransform powerUpButtonRect;
        [SerializeField] private RectTransform playButtonTutorialRect;

        [Header("Button Animation Parameters")]
        [SerializeField] private Vector2 buttonSizeRect;
        [SerializeField] private float buttonScaleDuration = 0.5f;
        [SerializeField] private float buttonScaleDelay = 0.2f;
        [SerializeField] private Ease buttonScaleEase = Ease.OutBack;

        [Header("Hand Animation")]
        [SerializeField] private float handScaleDuration = 0.5f;
        [SerializeField] private float handScaleElasticity = 0.2f;
        [SerializeField] private float handScalePunch = 0.2f;

        public void Deactivate()
        {
            powerUpButtonRect.sizeDelta = Vector2.zero;
            powerUpButtonRect.gameObject.SetActive(false);
            playButtonTutorialRect.gameObject.SetActive(false);
            tutorialHandRect.gameObject.SetActive(false);
            tutorialHandRect.DOKill();
        }

        public void ShowMagnetPowerupTutorial(Vector3 position)
        {
            powerUpButtonRect.gameObject.SetActive(true);
            powerUpButtonRect.position = position;
            powerUpButtonRect.DOSizeDelta(buttonSizeRect, buttonScaleDuration).SetDelay(buttonScaleDelay).SetEase(buttonScaleEase).OnComplete
                (() =>
                {
                    tutorialHandRect.position = powerUpButtonRect.position;
                    tutorialHandRect.gameObject.SetActive(true);
                    tutorialHandRect.DOKill();
                    tutorialHandRect.DOPunchScale(Vector3.one * handScalePunch, handScaleDuration, 0, handScaleElasticity).SetLoops(-1);
                    powerUpButtonRect.DOKill();
                });
        }

        public void ShowSpeedBoostPowerupTutorial(Vector3 position)
        {
            powerUpButtonRect.gameObject.SetActive(true);
            powerUpButtonRect.position = position;
            powerUpButtonRect.DOSizeDelta(buttonSizeRect, buttonScaleDuration).SetEase(buttonScaleEase).OnComplete
              (() =>
              {
                  tutorialHandRect.position = powerUpButtonRect.position;
                  tutorialHandRect.gameObject.SetActive(true);
                  tutorialHandRect.DOKill();
                  tutorialHandRect.DOPunchScale(Vector3.one * handScalePunch, handScaleDuration, 0, handScaleElasticity).SetLoops(-1);
                  powerUpButtonRect.DOKill();
              });
        }

        public void OnPowerupButtonPressed()
        {
            powerUpButtonRect.gameObject.SetActive(false);
            playButtonTutorialRect.gameObject.SetActive(true);
            tutorialHandRect.position = playButtonTutorialRect.position;
        }
    }
}
