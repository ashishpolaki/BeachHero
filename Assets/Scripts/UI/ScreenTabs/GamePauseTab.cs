using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class GamePauseTab : BaseScreenTab
    {
        [SerializeField] private Button panelCloseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button settingsButton;

        public override void Open()
        {
            base.Open();
            panelCloseButton.ButtonRegister(OnPanelCloseClick);
            resumeButton.ButtonRegister(OnResumeButtonClick);
            homeButton.ButtonRegister(OnHomeASync);
            settingsButton.ButtonRegister(OnSettings);
        }
        public override void Close()
        {
            base.Close();
            panelCloseButton.ButtonDeRegister();
            resumeButton.ButtonDeRegister();
            homeButton.ButtonDeRegister();
            settingsButton.ButtonDeRegister();
        }
        private void OnSettings()
        {
            // Open settings tab.
            // UIController.GetInstance.ScreenEvent(ScreenType.Settings, UIScreenEvent.Open);
        }
        private void OnPanelCloseClick()
        {
            GameController.GetInstance.SetGameState(GameState.Playing);
            Close();
        }
        private void OnResumeButtonClick()
        {
            GameController.GetInstance.SetGameState(GameState.Playing);
            Close();
        }
        private async void OnHomeASync()
        {
            await UIController.GetInstance.FadeInASync();
            GameController.GetInstance.RetryLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            UIController.GetInstance.FadeOut();
        }
    }
}
