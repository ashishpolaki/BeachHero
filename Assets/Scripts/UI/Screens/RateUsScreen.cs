using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class RateUsScreen : BaseScreen
    {
        [SerializeField] private Sprite inactiveStarSprite;
        [SerializeField] private Sprite activeStarSprite;
        [SerializeField] private Image[] starImages;
        [SerializeField] private UIButton[] starButtons;
        [SerializeField] private UIButton submitRatingButton;

        private int currentRating;
        private System.Action[] starButtonActions;

        private void Awake()
        {
            for (int i = 0; i < starButtons.Length; i++)
            {
                int index = i;
                starButtons[index].OnButtonReleased += () => HandleStarButton(index);
            }
        }

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            currentRating = 0;
            SaveSystem.CurrentData.isRateUsShown = true;
            SaveSystem.SaveGameData();
            PlayGamesController.GetInstance.SaveDataInCloud();

            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].sprite = inactiveStarSprite;
            }
            submitRatingButton.OnButtonReleased += HandleSubmitButton;
        }

        public override void Close()
        {
            base.Close();
            submitRatingButton.OnButtonReleased -= HandleSubmitButton;
        }

        private void HandleSubmitButton()
        {
            if(currentRating >= IntUtils.RATE_US_MIN_RATING_FOR_STORE || currentRating == 0)
            {
                //Go to playstore.
                var url = "https://play.google.com/store/apps/details?id=com.hunterKirito.BeachHero";
                Close();
                Application.OpenURL(url);
            }
            else
            {
                Close();
            }
        }

        private void HandleStarButton(int index)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].sprite = i <= index ? activeStarSprite : inactiveStarSprite;
            }
            currentRating = index + 1;
        }
    }
}
