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

        public void ShowPowerupTutorial(Transform powerupButton, Transform playButton)
        {
            // Reset & enable UI
            powerUpMaskRect.sizeDelta = Vector2.zero;
            powerUpMaskRect.gameObject.SetActive(true);
            rayCastPanel.SetActive(true);

            // Track references
            powerUpMaskRect.position = powerupButton.position;
            currentPowerupTransform = powerupButton;
            playButtonTransform = playButton;

            // Animate mask & show hand
            powerUpMaskRect.DOSizeDelta(powerupButtonSizeRect, buttonScaleDuration).SetDelay(buttonScaleDelay).SetEase(buttonScaleEase).OnComplete
           (() =>
           {
               powerUpMaskRect.DOKill();
               PlayTutorialHandAnimation(powerupButton);
           });
        }

        public void OnPowerupButtonPressed(Transform _buttonsParent)
        {
            powerUpMaskRect.gameObject.SetActive(false);
            tutorialHandRect.gameObject.SetActive(false);
            playButtonMaskRect.gameObject.SetActive(true);
            currentPowerupTransform.SetParent(_buttonsParent);
            playButtonMaskRect.sizeDelta = Vector2.zero;
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
