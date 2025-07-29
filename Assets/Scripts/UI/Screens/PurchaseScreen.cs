using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class PurchaseScreen : BaseScreen
    {
        [SerializeField] private Button closePanelButton;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            closePanelButton.ButtonRegister(OnCloseButtonClicked);
        }
        public override void Close()
        {
            base.Close();
            closePanelButton.ButtonDeRegister();
        }
        private void OnCloseButtonClicked()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Close, ScreenTabType.None);
        }
    }
}
