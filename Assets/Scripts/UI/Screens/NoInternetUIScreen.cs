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
            closePanelButton.onClick.AddListener(OnClosePanelClick);
            retryButton.onClick.AddListener(OnClosePanelClick);
        }
        public override void Close()
        {
            base.Close();
            closePanelButton.onClick.RemoveListener(OnClosePanelClick);
            retryButton.onClick.RemoveListener(OnClosePanelClick);
        }
        private void OnClosePanelClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.NoInternet, UIScreenEvent.Close);
        }
        private void OnRetryClick()
        {

        }
    }
}
