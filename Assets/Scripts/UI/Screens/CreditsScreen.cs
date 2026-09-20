using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class CreditsScreen : BaseScreen
    {
        [SerializeField] private Button closePanelButton;
        [SerializeField] private Button backButton;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            closePanelButton.ButtonRegister(ClosePanel);
            backButton.ButtonRegister(OnBack);
        }

        public override void Close()
        {
            base.Close();
            closePanelButton.ButtonDeRegister(ClosePanel);
            backButton.ButtonDeRegister(OnBack);
        }

        private void OnBack()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
            UIController.GetInstance.ScreenEvent(ScreenType.Credits, UIScreenEvent.Close);
        }

        private void ClosePanel()
        {
            AudioController.GetInstance.PlaySound(AudioType.Swoosh);
            UIController.GetInstance.ScreenEvent(ScreenType.Credits, UIScreenEvent.Close);
        }
    }
}
