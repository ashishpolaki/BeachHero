using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SkinStore
{
    public class SkinStoreController : MonoBehaviour
    {
        private static SkinStoreController instance;
        public static SkinStoreController Instance => instance;

        [SerializeField] SkinStoreDatabase database;
        public SkinStoreDatabase Database => instance.database;

        private Dictionary<TabData, List<SkinStoreProductContainer>> products;

        public int TabsCount => Database.Tabs.Length;
        public int CoinsForAdsAmount => Database.CoinsForAds;
        public CurrencyType CoinsForAdsCurrency => Database.CurrencyForAds;

        public TabData SelectedTabData { get; private set; }

        private UISkinStore storeUI;

        public ISkinsProvider SkinsProvider { get; private set; }

        public void Init(ISkinsProvider skinsProvider)
        {
            instance = this;

            SkinsProvider = skinsProvider;

            Dictionary<TabData, List<SkinStoreProductData>> rawProducts = Database.Init();
            products = new Dictionary<TabData, List<SkinStoreProductContainer>>();

            foreach (TabData tab in rawProducts.Keys)
            {
                List<SkinStoreProductData> productsList = rawProducts[tab];

                List<SkinStoreProductContainer> containersInsideTab = new List<SkinStoreProductContainer>();
                for(int i = 0; i < productsList.Count; i++)
                {
                    SkinStoreProductData product = productsList[i];

                    ISkinData skinData = null;
                    if (!product.IsDummy)
                    {
                        skinData = SkinsProvider.GetSkinData(product.SkinId);
                    }

                    var container = new SkinStoreProductContainer(product, skinData);

                    containersInsideTab.Add(container);
                }

                products.Add(tab, containersInsideTab);
            }

            storeUI = UIController.GetPage<UISkinStore>();
            storeUI.InitTabs(OnTabClicked);

            InitDefaultProducts();

            SelectedTabData = Database.Tabs[0];
        }

        private void InitDefaultProducts()
        {
            var visitedTypes = new List<SkinTab>();

            foreach (var tab in products.Keys)
            {
                if (visitedTypes.Contains(tab.Type))
                    continue;
                visitedTypes.Add(tab.Type);

                var page = products[tab];

                if (page.Count > 0)
                {
                    var defaultContainer = page[0];
                    SkinsProvider.UnlockSkin(defaultContainer.SkinData);
                }
            }
        }

        public TabData GetTab(int tabId)
        {
            return Database.Tabs[tabId];
        }

        public List<SkinStoreProductContainer> GetProducts(TabData tab)
        {
            return products[tab];
        }

        public int PagesCount(TabData tab)
        {
            return products[tab].Count;
        }

        private void OnTabClicked(TabData data)
        {
            SelectedTabData = data;

            storeUI.SetSelectedTab(data);
        }

        private void UnlockAndSelect(SkinStoreProductContainer container, bool select = true)
        {
            SkinsProvider.UnlockSkin(container.SkinData);
            if (select) SelectProduct(container);
        }

        public bool SelectProduct(SkinStoreProductContainer container)
        {
            if (!container.IsUnlocked)
                return false;

            SkinsProvider.SelectSkin(container.SkinData);

            storeUI.InitStoreUI();

            return true;
        }

        public bool BuyProduct(SkinStoreProductContainer container, bool select = true, bool free = false)
        {
            if (container.IsUnlocked)
                return SelectProduct(container);

            if (free)
            {
                UnlockAndSelect(container, select);
                return true;
            }
            else if (container.ProductData.PurchType == SkinStoreProductData.PurchaseType.InGameCurrency && CurrenciesController.HasAmount(container.ProductData.Currency, container.ProductData.Cost))
            {
                CurrenciesController.Substract(container.ProductData.Currency, container.ProductData.Cost);
                UnlockAndSelect(container, select);
                return true;
            }
            // note | this type can't return true or false because result is not defined during execution of this code
            // right now result of this method is not used, but otherwise this logic needs to be improved
            else if(container.ProductData.PurchType == SkinStoreProductData.PurchaseType.RewardedVideo)
            {
                AdsManager.ShowRewardBasedVideo((success) =>
                {
                    if(success)
                    {
                        container.ProductData.RewardedVideoWatchedAmount++;

                        if(container.ProductData.RewardedVideoWatchedAmount >= container.ProductData.Cost)
                        {
                            UnlockAndSelect(container, select);
                        }

                        storeUI.InitStoreUI();
                    }
                });
            }

            return false;
        }

        public SkinStoreProductData GetRandomLockedProduct()
        {
            SkinStoreProductData lockedProduct = null;

            Database.Tabs.FindRandomOrder(tab =>
            {
                var product = products[tab].FindRandomOrder(product =>
                {
                    return !product.IsUnlocked && !product.ProductData.IsDummy;
                });

                if (product != null)
                {
                    lockedProduct = product.ProductData;
                    return true;
                }

                return false;
            });

            return lockedProduct;
        }

        public SkinStoreProductData GetRandomUnlockedProduct(SkinTab tab)
        {
            return products[GetTab((int)tab)].FindRandomOrder(product =>
            {
                return product.IsUnlocked && !product.ProductData.IsDummy;
            }).ProductData;
        }

        public SkinStoreProductData GetRandomProduct(SkinTab tab)
        {
            return products[GetTab((int)tab)].GetRandomItem().ProductData;
        }

        public SkinStoreProductContainer GetSelectedProductContainer()
        {
            List<SkinStoreProductContainer> selectedTabContainers = products[SelectedTabData];

            for(int i = 0; i < selectedTabContainers.Count; i++)
            {
                SkinStoreProductContainer container = selectedTabContainers[i];

                if(SkinsProvider.IsSkinSelected(container.SkinData)) return container;
            }

            return null;
        }
    }
}