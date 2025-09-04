using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GameplayUIScreen : BaseScreen
    {
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button retryButton;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            pauseButton.ButtonRegister(OnPause);
            retryButton.ButtonRegister(OnRetry);
        }

        public override void Close()
        {
            base.Close();
            pauseButton.ButtonDeRegister(OnPause);
            retryButton.ButtonDeRegister(OnRetry);
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
