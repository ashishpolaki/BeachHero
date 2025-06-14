using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class StoreUIScreen : BaseScreen
    {
        [SerializeField] private StoreItemUI storeItemPrefab;
        [SerializeField] private Transform content;
        private List<StoreItemUI> storeItemsList = new List<StoreItemUI>();
        private bool isInitialized = false;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);

            if (!isInitialized)
            {
                InitializeStoreItems();
                isInitialized = true;
            }
        }

        public override void Close()
        {
            base.Close();
        }

        private void InitializeStoreItems()
        {
            //var storeItemsDatabase = GameController.GetInstance.StoreManager.StoreItemsDatabase;
            //foreach (var item in storeItemsDatabase)
            //{
            //    StoreItemUI storeItemUI = Instantiate(storeItemPrefab, content);
            //    storeItemUI.Initialize(item.Id); // Assuming item has an Id property
            //    storeItemsList.Add(storeItemUI);
            //}
        }
    }
}
