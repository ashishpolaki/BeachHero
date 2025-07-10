using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class AdNotLoadedUIScreen : BaseScreen
    {
        [SerializeField] private Button closePanelButton;
        [SerializeField] private Button retryButton;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            closePanelButton.onClick.AddListener(OnClosePanelClick);
            retryButton.onClick.AddListener(OnRetryClick);
        }

        public override void Close()
        {
            base.Close();
            closePanelButton.onClick.RemoveListener(OnClosePanelClick);
            retryButton.onClick.RemoveListener(OnRetryClick);
        }

        private void OnClosePanelClick()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.AdNotLoaded, UIScreenEvent.Close);
        }

        private void OnRetryClick()
        {
        }
    }
}
