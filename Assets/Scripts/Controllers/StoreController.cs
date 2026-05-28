using System;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace BeachHero
{
    public enum PurchaseItemType
    {
        None,
        StoreProduct,
        BoatSkin
    }
    public class StoreController : MonoBehaviour, IDetailedStoreListener
    {
        #region INspector Variables
        [SerializeField] private StoreDatabaseSO storeDatabase;
        [SerializeField] private BoatSkinDatabaseSO boatSkinDatabase;
        #endregion

        #region Private Variables
        private IStoreController m_StoreController; // The Unity Purchasing system.
        private PurchaseItemType currentPurchaseItemType;
        private int currentIndex;
        private int gameCurrencyBalance;

        private string defaultPrice = "$0.01"; // Default price for products that do not have a real money cost set.
        #endregion

        #region Actions
        public event Action<bool> OnStoreItemPurchaseAction;
        public event Action OnBoatPurchaseFail;
        public event Action OnGameCurrencyBalanceChange;
        #endregion

        #region Properties
        public int GameCurrencyBalance
        {
            get => gameCurrencyBalance;
            private set
            {
                gameCurrencyBalance = value;
                SaveSystem.SaveInt(StringUtils.GAME_CURRENCY_BALANCE, gameCurrencyBalance);
                OnGameCurrencyBalanceChange?.Invoke();
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
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add Real Money Products from Store Database
            foreach (var product in storeDatabase.RealMoneyProducts)
            {
                if (!string.IsNullOrEmpty(product.Id))
                {
                    builder.AddProduct(product.Id, product.Type);
                }
            }

            // Add Boat Skins that are for sale with real money
            foreach (var boatSkin in boatSkinDatabase.BoatSkins)
            {
                if (boatSkin.IsRealMoney && !boatSkin.IsDefaultBoat && !string.IsNullOrEmpty(boatSkin.ID))
                {
                    builder.AddProduct(boatSkin.ID, boatSkin.ProductType);
                }
            }
            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_StoreController = controller;
            for (int t = 0; t < m_StoreController.products.all.Length; t++)
            {
                var item = m_StoreController.products.all[t];

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
                            storeProduct.realMoneyCost = item.metadata.localizedPrice.ToString("N0");
                        }
                    }
                }
            }
            DebugUtils.Log("Store initialized with products: ");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            DebugUtils.LogError("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            DebugUtils.LogError("OnInitializeFailed InitializationFailureReason:" + error + " message: " + message);
        }

        private void InitBalances()
        {
            gameCurrencyBalance = SaveSystem.LoadInt(StringUtils.GAME_CURRENCY_BALANCE, IntUtils.DEFAULT_GAME_CURRENCY_BALANCE);
        }
        #endregion

        #region Purchase with Game Currency
        public void BuyStoreItemWithGameCurrency(int index)
        {
            currentIndex = index;
            var storeItem = GetGameCurrencyProduct(currentIndex);
            if (storeItem != null && GameCurrencyBalance >= storeItem.gameCurrencyCost)
            {
                StoreItemBought();
                DeductGameCurrencyBalance(storeItem.gameCurrencyCost);
            }
            else
            {
                HandleInSufficientGameCurrency();
            }
        }

        private void StoreItemBought()
        {
            var storeItem = GetGameCurrencyProduct(currentIndex);
            //Show Purchase Dialog
            if (storeItem != null)
            {
                for (int i = 0; i < storeItem.rewards.Length; i++)
                {
                    var reward = storeItem.rewards[i];

                    switch (reward.itemType)
                    {
                        case StoreItemType.Magnet:
                            GameController.GetInstance.PowerupController.UpdateMagnetBalance(reward.quantity);
                            break;

                        case StoreItemType.SpeedBoost:
                            GameController.GetInstance.PowerupController.UpdateSpeedBoostBalance(reward.quantity);
                            break;

                        case StoreItemType.GameCurrency:
                            GameCurrencyBalance += reward.quantity;
                            break;

                        case StoreItemType.NoAds:
                            AdController.GetInstance.PurchasedNoADsPack();
                            break;
                    }
                }
                OnStoreItemPurchaseAction?.Invoke(true);
            }
        }

        //Boats/Boat Skins
        public void BuyBoatWithGameCurrency(int index)
        {
            SkinController skinController = GameController.GetInstance.SkinController;
            BoatSkinSO boatSkin = skinController.GetBoatSkinByIndex(index);
            if (GameCurrencyBalance >= boatSkin.InGameCurrencyCost)
            {
                skinController.UnlockBoatSkin(index);
                DeductGameCurrencyBalance(boatSkin.InGameCurrencyCost);
            }
            else
            {
                HandleInSufficientGameCurrency();
            }
        }

        public void BuyBoatColorWithGameCurrency(int boatIndex, int colorIndex)
        {
            SkinController skinController = GameController.GetInstance.SkinController;
            BoatSkinSO boatSkin = skinController.GetBoatSkinByIndex(boatIndex);
            if (GameCurrencyBalance >= boatSkin.SkinColors[colorIndex].inGameCurrencyCost)
            {
                skinController.UnlockBoatSkinColor(boatIndex, colorIndex);
                DeductGameCurrencyBalance(boatSkin.SkinColors[colorIndex].inGameCurrencyCost);
            }
            else
            {
                HandleInSufficientGameCurrency();
            }
        }
        #endregion

        #region Game Currency Balance Management
        public void IncrementGameCurrencyBalance(int amount)
        {
            GameCurrencyBalance += amount;
            DebugUtils.Log($"Game currency balance increased by {amount}. New balance: {GameCurrencyBalance}");
        }
        public void DeductGameCurrencyBalance(int cost)
        {
            if (GameCurrencyBalance >= cost)
            {
                GameCurrencyBalance -= cost;
                DebugUtils.Log($"Game currency balance decreased by {cost}. New balance: {GameCurrencyBalance}");
            }
            else
            {
                DebugUtils.LogError("Not enough game currency to deduct.");
            }
        }
        private void HandleInSufficientGameCurrency()
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
                m_StoreController.InitiatePurchase(productID);
            }
            else
            {
                m_StoreController.InitiatePurchase(GetRealMoneyProductID(currentIndex));
            }
        }

        public void RetryPurchase()
        {
            if (currentPurchaseItemType == PurchaseItemType.BoatSkin)
            {
                string productID = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentIndex).ID;
                m_StoreController.InitiatePurchase(productID);
            }
            else
            {
                m_StoreController.InitiatePurchase(GetRealMoneyProductID(currentIndex));
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            if (currentPurchaseItemType == PurchaseItemType.StoreProduct)
            {
                StoreItemBought();
            }
            else if (currentPurchaseItemType == PurchaseItemType.BoatSkin)
            {
                GameController.GetInstance.SkinController.UnlockBoatSkin(currentIndex);
            }
            DebugUtils.Log($"Processing purchase for Store Product: {purchaseEvent.purchasedProduct.definition.id}");
            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed. 
            return PurchaseProcessingResult.Complete;
        }
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            if (currentPurchaseItemType == PurchaseItemType.StoreProduct)
            {
                OnStoreItemPurchaseAction?.Invoke(false);
            }
            else
            {
                OnBoatPurchaseFail?.Invoke();
            }
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
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            if (currentPurchaseItemType == PurchaseItemType.StoreProduct)
            {
                OnStoreItemPurchaseAction?.Invoke(false);
            }
            else
            {
                OnBoatPurchaseFail?.Invoke();
            }
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
        #endregion

        #region Get Game Currency Helpers
        public GameCurrencyProduct GetGameCurrencyProduct(int index)
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
