using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class InSufficientGameCurrencyTab : BaseScreenTab
    {
        [SerializeField] private Button goToStoreBtn;
        public override void Open()
        {
            base.Open();
            goToStoreBtn.ButtonRegister(OnGoToStoreButtonClicked);
        }
        public override void Close()
        {
            base.Close();
            goToStoreBtn.ButtonDeRegister();
        }
        private void OnGoToStoreButtonClicked()
        {
            //OpenStore
            UIController.GetInstance.ScreenEvent(ScreenType.Store, UIScreenEvent.Push, ScreenTabType.None);
        }
    }
}
