using DG.Tweening;
using UnityEngine;

namespace BeachHero
{
    public class PowerupTutorialPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform tutorialHandRect;
        [SerializeField] private RectTransform powerUpButtonRect;
        [SerializeField] private RectTransform playButtonTutorialRect;
        [SerializeField] private GameObject rayCastPanel;

        [Header("Button Animation Parameters")]
        [SerializeField] private Vector2 powerupButtonSizeRect;
        [SerializeField] private Vector2 playButtonSizeRect;
        [SerializeField] private float buttonScaleDuration = 0.5f;
        [SerializeField] private float buttonScaleDelay = 0.2f;
        [SerializeField] private Ease buttonScaleEase = Ease.OutBack;

        [Header("Hand Animation")]
        [SerializeField] private float handScaleDuration = 0.5f;
        [SerializeField] private float handScaleElasticity = 0.2f;
        [SerializeField] private float handScalePunch = 0.2f;

        private Transform playButtonTransform;
        private Transform currentPowerupTransform;

        public void Deactivate()
        {
            powerUpButtonRect.sizeDelta = Vector2.zero;
            powerUpButtonRect.gameObject.SetActive(false);
            playButtonTutorialRect.gameObject.SetActive(false);
            tutorialHandRect.gameObject.SetActive(false);
            rayCastPanel.SetActive(false);
            tutorialHandRect.DOKill();
            powerUpButtonRect.DOKill();
            playButtonTutorialRect.DOKill();
        }

        public void ShowMagnetPowerupTutorial(Transform _powerupButton, Transform _playButton)
        {
            powerUpButtonRect.gameObject.SetActive(true);
            rayCastPanel.SetActive(true); // Enable the raycast panel to block input during the tutorial
            powerUpButtonRect.position = _powerupButton.position;
            powerUpButtonRect.DOSizeDelta(powerupButtonSizeRect, buttonScaleDuration).SetDelay(buttonScaleDelay).SetEase(buttonScaleEase).OnComplete
                (() =>
                {
                    powerUpButtonRect.DOKill();
                    currentPowerupTransform = _powerupButton;
                    _powerupButton.SetParent(transform);
                    playButtonTransform = _playButton;
                });
        }

        public void ShowSpeedBoostPowerupTutorial(Transform _powerupButton, Transform _playButton)
        {
            powerUpButtonRect.gameObject.SetActive(true);
            rayCastPanel.SetActive(true); // Enable the raycast panel to block input during the tutorial
            powerUpButtonRect.position = _powerupButton.position;
            powerUpButtonRect.DOSizeDelta(powerupButtonSizeRect, buttonScaleDuration).SetEase(buttonScaleEase).OnComplete
              (() =>
              {
                  powerUpButtonRect.DOKill();
                  _powerupButton.SetParent(transform);
                  playButtonTransform = _playButton;
                  currentPowerupTransform = _powerupButton;
              });
        }
        public void OnPowerupButtonPressed(Transform _buttonsParent)
        {
            powerUpButtonRect.gameObject.SetActive(false);
            currentPowerupTransform.SetParent(_buttonsParent);

            playButtonTutorialRect.gameObject.SetActive(true);
            playButtonTutorialRect.position = playButtonTransform.position;
            playButtonTutorialRect.DOSizeDelta(playButtonSizeRect, buttonScaleDuration).SetEase(buttonScaleEase).OnComplete
              (() =>
              {
                  tutorialHandRect.position = playButtonTransform.position;
                  tutorialHandRect.gameObject.SetActive(true);
                  tutorialHandRect.DOKill();
                  tutorialHandRect.DOPunchScale(Vector3.one * handScalePunch, handScaleDuration, 0, handScaleElasticity).SetLoops(-1);
                  playButtonTransform.SetParent(transform);
                  //  playButtonTransform.SetAsLastSibling(); // Ensure the play button is on top
              });
        }
    }
}
