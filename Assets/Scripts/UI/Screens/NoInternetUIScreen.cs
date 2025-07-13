using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class NoInternetUIScreen : BaseScreen
    {
        [SerializeField] private Button closePanelButton;
        [SerializeField] private Button retryButton;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            closePanelButton.ButtonRegister(OnClosePanelClick);
            retryButton.ButtonRegister(OnRetryClick);
        }
        public override void Close()
        {
            base.Close();
            closePanelButton.ButtonDeRegister();
            retryButton.ButtonDeRegister();
        }
        private void OnClosePanelClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.NoInternet, UIScreenEvent.Close);
        }
        private void OnRetryClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.NoInternet, UIScreenEvent.Close);
            if (AdController.GetInstance.IsRewardedADLoaded())
            {
                AdController.GetInstance.ShowRewardedAd();
            }
            else
            {
                AdController.GetInstance.RequestRewardedAD();
            }
        }
    }
}
