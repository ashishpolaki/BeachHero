using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class NoInternetUIScreen : BaseScreen
    {
        [SerializeField] private Button closePanelButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button backButton;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            closePanelButton.ButtonRegister(OnClosePanelClick);
            retryButton.ButtonRegister(OnRetryClick);
            backButton.ButtonRegister(OnBackClick);
        }
        public override void Close()
        {
            base.Close();
            closePanelButton.ButtonDeRegisterAll();
            retryButton.ButtonDeRegisterAll();
            backButton.ButtonDeRegisterAll();
        }
        private void OnClosePanelClick()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
            UIController.GetInstance.ScreenEvent(ScreenType.NoInternet, UIScreenEvent.Close);
        }
        private void OnRetryClick()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
            UIController.GetInstance.ScreenEvent(ScreenType.NoInternet, UIScreenEvent.Close);
            NetworkController.ExecuteRetry();
        }
        private void OnBackClick()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
            UIController.GetInstance.ScreenEvent(ScreenType.NoInternet, UIScreenEvent.Close);
        }
    }
}
