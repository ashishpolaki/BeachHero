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
            AdController.GetInstance.HideBanner();
        }
        public override void Close()
        {
            base.Close();
            panelCloseButton.ButtonDeRegisterAll();
            resumeButton.ButtonDeRegisterAll();
            homeButton.ButtonDeRegisterAll();
            settingsButton.ButtonDeRegisterAll();
        }
        private void OnSettings()
        {
            // Open settings tab.
             UIController.GetInstance.ScreenEvent(ScreenType.Settings, UIScreenEvent.Push);
        }
        private void OnPanelCloseClick()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
            AdController.GetInstance.ShowBanner();
            GameController.GetInstance.SetGameState(GameState.Playing);
            Close();
        }
        private void OnResumeButtonClick()
        {
            AdController.GetInstance.ShowBanner();
            GameController.GetInstance.SetGameState(GameState.Playing);
            Close();
        }
        private async void OnHomeASync()
        {
            await UIController.GetInstance.LoadingUI.ShowLoadingScreen();
            GameController.GetInstance.BackToMainMenu();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
        }
    }
}
