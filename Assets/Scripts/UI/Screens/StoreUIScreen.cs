using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class StoreUIScreen : BaseScreen
    {
        [SerializeField] private Button homeButton;
        [SerializeField] private Transform content;
        private List<StoreItemUI> storeItemsList = new List<StoreItemUI>();
        private bool isInitialized = false;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            homeButton.onClick.AddListener(OpenHome);
            if (!isInitialized)
            {
                InitializeStoreItems();
                isInitialized = true;
            }
        }

        public override void Close()
        {
            base.Close();
            homeButton.onClick.RemoveAllListeners();
        }

        private void OpenHome()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
        }

        private void InitializeStoreItems()
        {
            foreach (var storeItem in GameController.GetInstance.StoreController.StoreDatabase.StoreItems)
            {
                StoreItemUI storeItemUI = Instantiate(storeItem.UIPrefab, content);
                storeItemUI.Initialize(storeItem);
                storeItemsList.Add(storeItemUI);
            }
        }
    }
}
