using DG.Tweening;
using UnityEngine;

namespace BeachHero
{
    public class PowerupTutorialPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform tutorialHandRect;
        [SerializeField] private RectTransform powerUpMaskRect;
        [SerializeField] private RectTransform playButtonMaskRect;
        [SerializeField] private GameObject rayCastPanel;

        [Header("Button Animation Parameters")]
        [SerializeField] private Vector2 powerupButtonSizeRect;
        [SerializeField] private Vector2 playButtonSizeRect;
        [SerializeField] private float buttonScaleDuration = 0.5f;
        [SerializeField] private float buttonScaleDelay = 0.2f;
        [SerializeField] private Ease buttonScaleEase = Ease.OutBack;

        [Header("Hand Animation")]
        [SerializeField] private float handMoveOffset = 50f;        // How far up/down the hand moves
        [SerializeField] private float handMoveDuration = 0.5f;
        [SerializeField] private Ease handMoveEase = Ease.InOutSine;

        private Transform playButtonTransform;
        private Transform currentPowerupTransform;

        public void Deactivate()
        {
            powerUpMaskRect.sizeDelta = Vector2.zero;
            powerUpMaskRect.gameObject.SetActive(false);
            playButtonMaskRect.gameObject.SetActive(false);
            tutorialHandRect.gameObject.SetActive(false);
            rayCastPanel.SetActive(false);
            tutorialHandRect.DOKill();
            powerUpMaskRect.DOKill();
            playButtonMaskRect.DOKill();
        }

        public void ShowMagnetPowerupTutorial(Transform _powerupButton, Transform _playButton)
        {
            powerUpMaskRect.gameObject.SetActive(true);
            rayCastPanel.SetActive(true); // Enable the raycast panel to block input during the tutorial
            powerUpMaskRect.position = _powerupButton.position;
            currentPowerupTransform = _powerupButton;
            playButtonTransform = _playButton;
            powerUpMaskRect.DOSizeDelta(powerupButtonSizeRect, buttonScaleDuration).SetDelay(buttonScaleDelay).SetEase(buttonScaleEase).OnComplete
                (() =>
                {
                    powerUpMaskRect.DOKill();
                    PlayTutorialHandAnimation(_powerupButton);
                });
        }

        public void ShowSpeedBoostPowerupTutorial(Transform _powerupButton, Transform _playButton)
        {
            powerUpMaskRect.gameObject.SetActive(true);
            rayCastPanel.SetActive(true);
            powerUpMaskRect.position = _powerupButton.position;
            playButtonTransform = _playButton;
            currentPowerupTransform = _powerupButton;
            powerUpMaskRect.DOSizeDelta(powerupButtonSizeRect, buttonScaleDuration).SetEase(buttonScaleEase).OnComplete
              (() =>
              {
                  powerUpMaskRect.DOKill();
                  PlayTutorialHandAnimation(_powerupButton);
              });
        }
        public void OnPowerupButtonPressed(Transform _buttonsParent)
        {
            powerUpMaskRect.gameObject.SetActive(false);
            tutorialHandRect.gameObject.SetActive(false);
            playButtonMaskRect.gameObject.SetActive(true);
            currentPowerupTransform.SetParent(_buttonsParent);
            playButtonMaskRect.position = playButtonTransform.position;
            playButtonMaskRect.DOSizeDelta(playButtonSizeRect, buttonScaleDuration).SetEase(buttonScaleEase).OnComplete
              (() =>
              {
                  PlayTutorialHandAnimation(playButtonTransform);
              });
        }

        private void PlayTutorialHandAnimation(Transform _transform)
        {
            tutorialHandRect.DOKill();
            tutorialHandRect.position = _transform.position;
            tutorialHandRect.gameObject.SetActive(true);
            Vector2 anchoredPos = tutorialHandRect.anchoredPosition;
            tutorialHandRect.DOAnchorPosY(anchoredPos.y + handMoveOffset, handMoveDuration).SetEase(handMoveEase).SetLoops(-1, LoopType.Yoyo);
            _transform.SetParent(transform);
            //  playButtonTransform.SetAsLastSibling(); // Ensure the play button is on top
        }
    }
}
