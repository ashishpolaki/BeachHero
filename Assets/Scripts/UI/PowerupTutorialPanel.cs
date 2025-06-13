using DG.Tweening;
using UnityEngine;

namespace BeachHero
{
    public class PowerupTutorialPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform tutorialHandRect;
        [SerializeField] private RectTransform powerUpTutorialRect;
        [SerializeField] private RectTransform playButtonTutorialRect;


        // Hand animation parameters
        [Header("Hand Animation")]
        [SerializeField] private float handScaleDuration = 0.5f;
        [SerializeField] private float handScaleElasticity = 0.2f;
        [SerializeField] private float handScalePunch = 0.2f;

        public void Deactivate()
        {
            powerUpTutorialRect.gameObject.SetActive(false);
            playButtonTutorialRect.gameObject.SetActive(false);
            tutorialHandRect.gameObject.SetActive(false);
            tutorialHandRect.DOKill();
        }

        public void ShowMagnetPowerupTutorial(Vector3 position)
        {
            powerUpTutorialRect.gameObject.SetActive(true);
            powerUpTutorialRect.position = position;
            tutorialHandRect.gameObject.SetActive(true);
            tutorialHandRect.position = powerUpTutorialRect.position;
            tutorialHandRect.DOKill();
            tutorialHandRect.DOPunchScale(Vector3.one * handScalePunch, handScaleDuration, 0, handScaleElasticity).SetLoops(-1);
        }

        public void ShowSpeedBoostPowerupTutorial(Vector3 position)
        {
            powerUpTutorialRect.gameObject.SetActive(true);
            powerUpTutorialRect.position = position;
            tutorialHandRect.gameObject.SetActive(true);
            tutorialHandRect.position = powerUpTutorialRect.position;
            tutorialHandRect.DOKill();
            tutorialHandRect.DOPunchScale(Vector3.one * handScalePunch, handScaleDuration, 0, handScaleElasticity).SetLoops(-1);
        }

        public void OnPowerupButtonPressed()
        {
            powerUpTutorialRect.gameObject.SetActive(false);
            playButtonTutorialRect.gameObject.SetActive(true);
            tutorialHandRect.position = playButtonTutorialRect.position;
        }
    }
}
