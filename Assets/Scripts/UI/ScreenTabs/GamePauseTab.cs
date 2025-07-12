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
            panelCloseButton.onClick.AddListener(Close);
            resumeButton.onClick.AddListener(Close);
            homeButton.onClick.AddListener(OnHomeASync);
            settingsButton.onClick.AddListener(OnSettings);
        }
        public override void Close()
        {
            base.Close();
            panelCloseButton.onClick.RemoveListener(Close);
            resumeButton.onClick.RemoveListener(Close);
            homeButton.onClick.RemoveListener(OnHomeASync);
            GameController.GetInstance.SetGameState(GameState.Playing);
        }
        private void OnSettings()
        {
            // Open settings tab.
            // UIController.GetInstance.ScreenEvent(ScreenType.Settings, UIScreenEvent.Open);
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
