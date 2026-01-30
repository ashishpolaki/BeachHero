using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameplayUIScreen : BaseScreen
    {
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Image medal1;
        [SerializeField] private Image medal2;
        [SerializeField] private Image medal3;
        [SerializeField] private Color medalEarned;
        [SerializeField] private Color medalUnEarned;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            GameController.GetInstance.LevelController.OnMedalCountUpdated += OnMedalCountUpdated;
            pauseButton.ButtonRegister(OnPause);
            retryButton.ButtonRegister(OnRetry);
        }

        public override void Close()
        {
            base.Close();
            ResetMedals();
            GameController.GetInstance.LevelController.OnMedalCountUpdated -= OnMedalCountUpdated;
            pauseButton.ButtonDeRegister(OnPause);
            retryButton.ButtonDeRegister(OnRetry);
        }

        private void ResetMedals()
        {
            medal1.color = medalUnEarned;
            medal2.color = medalUnEarned;
            medal3.color = medalUnEarned;
        }

        private void OnMedalCountUpdated(int medalCount)
        {
            medal1.color = medalCount >= 1 ? medalEarned : medalUnEarned;
            medal2.color = medalCount >= 2 ? medalEarned : medalUnEarned;
            medal3.color = medalCount >= 3 ? medalEarned : medalUnEarned;
        }

        private void OnPause()
        {
            GameController.GetInstance.SetGameState(GameState.Paused);
            OpenTab(ScreenTabType.GamePause);
        }
        private void OnRetry()
        {
            GameController.GetInstance.SetGameState(GameState.Paused);
            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Push);
        }
    }
}
