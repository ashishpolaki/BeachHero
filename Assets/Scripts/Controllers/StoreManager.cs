using System;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace BeachHero
{
    public enum PurchaseItemType
    {
        None,
        StoreProduct,
        BoatSkin
    }
    public class StoreManager : MonoBehaviour
    {
        #region INspector Variables
        [SerializeField] private StoreDatabaseSO storeDatabase;
        [SerializeField] private BoatSkinDatabaseSO boatSkinDatabase;
        #endregion

        #region Private Variables
        private StoreController m_StoreController; // The Unity Purchasing system.
        private PurchaseItemType currentPurchaseItemType;
        private int currentIndex;
        private int gameCurrencyBalance;

        private string defaultPrice = "$0.01"; // Default price for products that do not have a real money cost set.
        #endregion

        #region Actions
        public event Action<bool> OnStoreItemPurchaseAction;
        public event Action OnBoatPurchaseFail;
        public event Action OnCoinsBalanceChange;
        #endregion

        #region Properties
        public int CoinsBalance
        {
            get => gameCurrencyBalance;
            private set
            {
                gameCurrencyBalance = value;
                SaveSystem.SaveInt(StringUtils.GAME_CURRENCY_BALANCE, gameCurrencyBalance);
                OnCoinsBalanceChange?.Invoke();
            }
        }
        #endregion

        #region Initialisation
        public void Init()
        {
            InitializeServices();
            InitBalances();
        }
        private async void InitializeServices()
        {
            await UnityServices.InitializeAsync();

            // Get StoreController
            m_StoreController = UnityIAPServices.StoreController();

            //Store
            m_StoreController.OnStoreConnected += OnStoreConnected;
            m_StoreController.OnStoreDisconnected += OnStoreDisconnected;

            //Products
            m_StoreController.OnProductsFetched += OnProductsFetched;
            m_StoreController.OnProductsFetchFailed += OnProductsFetchFailed;

            //Purchases
            m_StoreController.OnPurchasePending += OnPurchasePending;
            m_StoreController.OnPurchaseFailed += OnPurchaseFailed;
            m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
            m_StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            m_StoreController.OnPurchaseDeferred += OnPurchaseDeferred;
            m_StoreController.OnPurchaseConfirmed += OnPUrchaseConfirm;

            // Add Real Money Products from Store Database
            var catalogProvider = new CatalogProvider();
            foreach (var product in storeDatabase.RealMoneyProducts)
            {
                if (!string.IsNullOrEmpty(product.Id))
                {
                    catalogProvider.AddProduct(product.Id, product.Type);
                }
            }

            // Add Boat Skins that are for sale with real money
            foreach (var boatSkin in boatSkinDatabase.BoatSkins)
            {
                if (boatSkin.IsRealMoney && !boatSkin.IsDefaultBoat && !string.IsNullOrEmpty(boatSkin.ID))
                {
                    catalogProvider.AddProduct(boatSkin.ID, boatSkin.ProductType);
                }
            }
            await m_StoreController.Connect();
            // Fetch products from store
            catalogProvider.FetchProducts(list => m_StoreController.FetchProducts(list));
        }
        private void InitBalances()
        {
            gameCurrencyBalance = SaveSystem.LoadInt(StringUtils.GAME_CURRENCY_BALANCE, IntUtils.DEFAULT_GAME_CURRENCY_BALANCE);
        }
        #endregion

        #region Handle StoreController Methods
        //Store
        private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
        {
            DebugUtils.LogError("StoreConnectionFailureDescriptionReason:" + failure.Message);
        }
        private void OnStoreConnected()
        {
            DebugUtils.Log("Connected to Store:");
        }

        //Products
        private void OnProductsFetched(List<Product> products)
        {
            // Fetch purchases for successfully retrieved products
            m_StoreController.FetchPurchases();

            for (int t = 0; t < products.Count; t++)
            {
                var item = products[t];

                if (!string.Equals(defaultPrice, item.metadata.localizedPriceString))
                {
                    //Boat Skin
                    if (item.definition.id.Contains("boat", StringComparison.OrdinalIgnoreCase))
                    {
                        BoatSkinSO boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByID(item.definition.id);
                        if (boatSkin != null)
                        {
                            boatSkin.SetRealMoneyCost(item.metadata.localizedPriceString);
                        }
                    }
                    else
                    {
                        //Store Product
                        RealMoneyProduct storeProduct = GetRealMoneyProduct(item.definition.id);
                        if (storeProduct != null)
                        {
                            storeProduct.realMoneyCost = item.metadata.localizedPriceString;
                        }
                    }
                }
            }
            DebugUtils.Log("Store initialized with products: ");
        }
        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            DebugUtils.LogError("OnProductsFetchFailed FailureReason: " + failure.FailureReason);
        }

        //Purchase methods
        private void OnPurchasePending(PendingOrder order)
        {
            if (currentPurchaseItemType == PurchaseItemType.StoreProduct)
            {
                StoreItemBought();
            }
            else if (currentPurchaseItemType == PurchaseItemType.BoatSkin)
            {
                GameController.GetInstance.SkinController.UnlockBoatSkin(currentIndex);
            }
            DebugUtils.Log($"Processing purchase for Store Product: {order.Info.PurchasedProductInfo}");
        }
        private void OnPurchaseDeferred(DeferredOrder deferredOrder)
        {
            DebugUtils.Log($"OnPurchaseDeferred: {deferredOrder.Info.PurchasedProductInfo}");
        }
        private void OnPUrchaseConfirm(Order confirmedOrder)
        {
            DebugUtils.Log($"OnPUrchaseConfirm: {confirmedOrder.Info.PurchasedProductInfo}");
        }
        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            if (currentPurchaseItemType == PurchaseItemType.StoreProduct)
            {
                OnStoreItemPurchaseAction?.Invoke(false);
            }
            else
            {
                OnBoatPurchaseFail?.Invoke();
            }
            DebugUtils.LogError($"OnPurchaseFailed for Store Product: {failedOrder.FailureReason}");
            //            if (Application.internetReachability != NetworkReachability.NotReachable)
            //            {

            //                PlayerPrefs.SetInt("PURCHASE_FAILED_COUNT", PlayerPrefs.GetInt("PURCHASE_FAILED_COUNT", 0) + 1);

            //                //GameAnalytics.PurchasedFailedCount(PlayerPrefs.GetInt("PURCHASE_FAILED_COUNT", 0));

            //#if UNITY_ANDROID
            //                //Show Purchase failed popup or native message
            //                //  ShopDialog.instance.PurchaseGems(false, currentIndex);

            //#elif UNITY_IOS
            //        //MobileNativePopups.OpenAlertDialog(
            //        //        "PURCHASE FAIL", failureReason.ToString(),
            //        //        "OK",
            //        //        () => { DebugUtils.Log("Ok was pressed"); });
            //#endif
            //            }
            //            return;
        }
        private void OnPurchasesFetched(Orders orders)
        {
            DebugUtils.Log($"OnPurchasesFetched for Store Product");
        }
        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            DebugUtils.LogError($"OnPurchasesFetchFailed for Store Product: {failure.FailureReason}");
        }
        //public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        //{
        //    if (currentPurchaseItemType == PurchaseItemType.StoreProduct)
        //    {
        //        StoreItemBought();
        //    }
        //    else if (currentPurchaseItemType == PurchaseItemType.BoatSkin)
        //    {
        //        GameController.GetInstance.SkinController.UnlockBoatSkin(currentIndex);
        //    }
        //    DebugUtils.Log($"Processing purchase for Store Product: {purchaseEvent.purchasedProduct.definition.id}");
        //    // Return a flag indicating whether this product has completely been received, or if the application needs 
        //    // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        //    // saving purchased products to the cloud, and when that save is delayed. 
        //    return PurchaseProcessingResult.Complete;
        //}

        //public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        //{
        //    if (currentPurchaseItemType == PurchaseItemType.StoreProduct)
        //    {
        //        OnStoreItemPurchaseAction?.Invoke(false);
        //    }
        //    else
        //    {
        //        OnBoatPurchaseFail?.Invoke();
        //    }
        //}
        #endregion

        #region Purchase with Coins
        public void BuyStoreItemWithCoins(int index)
        {
            currentIndex = index;
            var storeItem = GetCoinsProduct(currentIndex);
            if (storeItem != null && CoinsBalance >= storeItem.gameCurrencyCost)
            {
                StoreItemBoughtWithCoins();
                DeductCoinsBalance(storeItem.gameCurrencyCost);
            }
            else
            {
                HandleInSufficientCoins();
            }
        }

        private void StoreItemBoughtWithCoins()
        {
            var storeItem = GetCoinsProduct(currentIndex);
            //Show Purchase Dialog
            if (storeItem != null)
            {
                for (int i = 0; i < storeItem.rewards.Length; i++)
                {
                    var reward = storeItem.rewards[i];

                    switch (reward.itemType)
                    {
                        case StoreItemType.Shield:
                            GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.Shield, reward.quantity);
                            break;

                        case StoreItemType.SpeedBoost:
                            GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.SpeedBoost, reward.quantity);
                            break;

                        case StoreItemType.Coins:
                            CoinsBalance += reward.quantity;
                            break;

                        case StoreItemType.NoAds:
                            AdController.GetInstance.PurchasedNoADsPack();
                            break;
                    }
                }
                OnStoreItemPurchaseAction?.Invoke(true);
            }
        }

        #region Boats

        //Boats/Boat Skins
        public void BuyBoatWithCoins(int index)
        {
            SkinController skinController = GameController.GetInstance.SkinController;
            BoatSkinSO boatSkin = skinController.GetBoatSkinByIndex(index);
            if (CoinsBalance >= boatSkin.InGameCurrencyCost)
            {
                skinController.UnlockBoatSkin(index);
                DeductCoinsBalance(boatSkin.InGameCurrencyCost);
            }
            else
            {
                HandleInSufficientCoins();
            }
        }

        public void BuyBoatColorWithCoins(int boatIndex, int colorIndex)
        {
            SkinController skinController = GameController.GetInstance.SkinController;
            BoatSkinSO boatSkin = skinController.GetBoatSkinByIndex(boatIndex);
            if (CoinsBalance >= boatSkin.SkinColors[colorIndex].inGameCurrencyCost)
            {
                skinController.UnlockBoatSkinColor(boatIndex, colorIndex);
                DeductCoinsBalance(boatSkin.SkinColors[colorIndex].inGameCurrencyCost);
            }
            else
            {
                HandleInSufficientCoins();
            }
        }
        #endregion

        #endregion

        #region Coins Balance Management
        public void IncrementCoinsBalance(int amount)
        {
            CoinsBalance += amount;
            DebugUtils.Log($"Game currency balance increased by {amount}. New balance: {CoinsBalance}");
        }
        public void DeductCoinsBalance(int cost)
        {
            if (CoinsBalance >= cost)
            {
                CoinsBalance -= cost;
                DebugUtils.Log($"Game currency balance decreased by {cost}. New balance: {CoinsBalance}");
            }
            else
            {
                DebugUtils.LogError("Not enough game currency to deduct.");
            }
        }
        private void HandleInSufficientCoins()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push, ScreenTabType.InsufficientGameCurrency);
        }
        #endregion

        #region Purchase With Real Money
        public void PurchaseWithRealMoney(int index, PurchaseItemType purchaseItemType)
        {
            currentIndex = index;
            currentPurchaseItemType = purchaseItemType;

            if (currentPurchaseItemType == PurchaseItemType.BoatSkin)
            {
                string productID = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentIndex).ID;
                m_StoreController.PurchaseProduct(productID);
            }
            else
            {
                m_StoreController.PurchaseProduct(GetRealMoneyProductID(currentIndex));
            }
        }

        private void StoreItemBought()
        {
            var storeItem = GetRealMoneyProduct(currentIndex);
            //Show Purchase Dialog
            if (storeItem != null)
            {
                for (int i = 0; i < storeItem.rewards.Length; i++)
                {
                    var reward = storeItem.rewards[i];

                    switch (reward.itemType)
                    {
                        case StoreItemType.Shield:
                            GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.Shield, reward.quantity);
                            break;

                        case StoreItemType.SpeedBoost:
                            GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.SpeedBoost, reward.quantity);
                            break;

                        case StoreItemType.Coins:
                            CoinsBalance += reward.quantity;
                            break;

                        case StoreItemType.NoAds:
                            AdController.GetInstance.PurchasedNoADsPack();
                            break;
                    }
                }
                OnStoreItemPurchaseAction?.Invoke(true);
            }
        }

        public void RetryPurchase()
        {
            if (currentPurchaseItemType == PurchaseItemType.BoatSkin)
            {
                string productID = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentIndex).ID;
                m_StoreController.PurchaseProduct(productID);
            }
            else
            {
                m_StoreController.PurchaseProduct(GetRealMoneyProductID(currentIndex));
            }
        }

        #endregion

        #region Get Coins Helpers
        public GameCurrencyProduct GetCoinsProduct(int index)
        {
            foreach (var product in storeDatabase.GameCurrencyProducts)
            {
                if (product.index == index)
                {
                    return product;
                }
            }
            return null;
        }
        #endregion

        #region Get Real Money Helpers 
        public RealMoneyProduct GetRealMoneyProduct(int index)
        {
            foreach (var product in storeDatabase.RealMoneyProducts)
            {
                if (product.index == index)
                {
                    return product;
                }
            }
            return null;
        }
        public RealMoneyProduct GetRealMoneyProduct(string id)
        {
            foreach (var product in storeDatabase.RealMoneyProducts)
            {
                if (string.Equals(product.Id, id, System.StringComparison.OrdinalIgnoreCase))
                {
                    return product;
                }
            }
            return null;
        }
        public string GetRealMoneyProductID(int index)
        {
            foreach (var product in storeDatabase.RealMoneyProducts)
            {
                if (product.index == index)
                {
                    return product.Id;
                }
            }
            return string.Empty;
        }
        #endregion
    }
}
