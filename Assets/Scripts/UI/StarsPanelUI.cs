using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class StarsPanelUI : MonoBehaviour
    {
        #region Inspector Variables
        [Header("References")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite emptyStarSprite;
        [SerializeField] private Sprite filledStarSprite;
        [SerializeField] private Transform starPanel;
        public Transform StarPanel => starPanel;

        [Header("StarPanel Anim Settings")]
        [SerializeField] private Vector3 starPanelAnimScale = new Vector3(1.03f, 1.03f, 1.03f);
        [SerializeField] private float starPanelAnimDuration = 0.09f;
        [SerializeField] private Ease starPanelAnimEase = Ease.OutQuad;
        [SerializeField] private float starPanelAnimReturnDuration = 0.09f;
        [SerializeField] private Ease starPanelAnimReturnEase = Ease.InQuad;

        [Header("Star Anim Settings")]
        [SerializeField] private Vector3 starPunchScale = new Vector3(-0.5f, -0.5f, 1);
        [SerializeField] private int starPunchFrequency = 3;
        [SerializeField] private float starPunchDamper = 0.55f;
        [SerializeField] private float starPunchDuration = 0.55f;

        [SerializeField] private Vector3 starPunchPositionStrength = new Vector3(0, 6f, 0);
        [SerializeField] private int starPunchPositionFrequency = 5;
        [SerializeField] private float starPunchPositionDamper = 0.5f;
        #endregion

        #region Private Variables
        private TweenHandle starPanelTween;
        private int starsCollected;
        #endregion

        public void Open()
        {
            starsCollected = 0;
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].sprite = emptyStarSprite;
                starImages[i].transform.localScale = Vector3.one;
            }
            GameController.GetInstance.LevelController.OnMedalCountUpdated += UpdateStarFill;
            GameController.GetInstance.LevelController.OnCoinCollectAnimation += HandleCoinCollection;
        }

        public void Close()
        {
            GameController.GetInstance.LevelController.OnMedalCountUpdated -= UpdateStarFill;
            GameController.GetInstance.LevelController.OnCoinCollectAnimation -= HandleCoinCollection;
        }

        private void HandleCoinCollection()
        {
            starPanel.transform.localScale = Vector3.one;
            starPanelTween.Cancel();
            starPanelTween = TweenManager.Scale(starPanel.localScale, starPanelAnimScale, starPanel.transform, starPanelAnimDuration, starPanelAnimEase,
                onComplete: () =>
                {
                    TweenManager.Scale(starPanel.localScale, Vector3.one, starPanel.transform, starPanelAnimReturnDuration, starPanelAnimReturnEase);
                    GameController.GetInstance.LevelController.CalculateStars();
                });
        }

        private void UpdateStarFill(int starsEarned)
        {
            if (starsEarned <= starsCollected)
                return;

            if (starImages == null)
                return;

            starPanelTween.Complete();
                         AudioController.GetInstance.PlaySound(AudioType.StarEarned);
            // ONLY animate new stars
            for (int i = starsCollected; i < starsEarned; i++)
            {
                int index = i;
                TweenManager.PunchScale(starImages[index].transform, Vector3.one, starPunchScale, starPunchFrequency, starPunchDamper, starPunchDuration
                 , onComplete: () =>
                 {
                     starImages[index].sprite = filledStarSprite;
                 });
                TweenManager.PunchPosition(starImages[index].transform, starImages[index].transform.localPosition,
                    starPunchPositionStrength, starPunchPositionFrequency, starPunchPositionDamper, starPunchDuration, transformSpace: TransformSpace.Local);
            }
            starsCollected = starsEarned;
        }
    }
}
