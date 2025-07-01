using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Unity.Services.Core;
using System;

namespace BeachHero
{
    public class StoreController : MonoBehaviour, IDetailedStoreListener
    {
        #region INspector Variables
        [SerializeField] private StoreDatabaseSO storeDatabase;
        #endregion

        #region Private Variables
        private IStoreController m_StoreController; // The Unity Purchasing system.
        private int currentIndex;
        private int gameCurrencyBalance;
   
        private string defaultPrice = "$0.01"; // Default price for products that do not have a real money cost set.
        #endregion

        #region Actions
        public event Action<bool> OnPurchaseSuccess;
        public event Action OnGameCurrencyBalanceChange;
        #endregion

        #region Properties
        public int GameCurrencyBalance
        {
            get => gameCurrencyBalance;
            private set
            {
                gameCurrencyBalance = value;
                SaveController.SaveInt(StringUtils.GAME_CURRENCY_BALANCE, gameCurrencyBalance);
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
            foreach (var product in storeDatabase.StoreProducts)
            {
                if (!string.IsNullOrEmpty(product.Id))
                {
                    builder.AddProduct(product.Id, product.Type);
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
                    StoreProduct storeProduct = GetStoreProduct(item.definition.id);
                    if (storeProduct != null)
                    {
                        storeProduct.realMoneyCost = $"{item.metadata.localizedPriceString}";
                    }
                }
            }
            DebugUtils.Log("Store initialized with products: ");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            DebugUtils.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            DebugUtils.Log("OnInitializeFailed InitializationFailureReason:" + error + " message: " + message);
        }

        private void InitBalances()
        {
            gameCurrencyBalance = SaveController.LoadInt(StringUtils.GAME_CURRENCY_BALANCE, IntUtils.DEFAULT_GAME_CURRENCY_BALANCE);
        }
        #endregion

        #region Purchase
        public void PurchaseWithRealMoney(int index)
        {
            currentIndex = index;
            m_StoreController.InitiatePurchase(GetProductID(currentIndex));
        }

        public void PurchaseWithGameCurrency(int index)
        {
            currentIndex = index;
            StoreProduct storeItem = GetStoreProduct(currentIndex);
            if (storeItem != null && GameCurrencyBalance >= storeItem.gameCurrencyCost)
            {
                GameCurrencyBalance -= storeItem.gameCurrencyCost;
                ItemBought();
            }
            else
            {
                DebugUtils.Log("Not enough game currency to purchase this item.");
                OnPurchaseSuccess?.Invoke(false);
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            ItemBought();
            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed. 
            return PurchaseProcessingResult.Complete;
        }
        private void ItemBought()
        {
            var storeItem = GetStoreProduct(currentIndex);
            //Show Purchase Dialog
            if (storeItem != null)
            {
                for (int i = 0; i < storeItem.contents.Length; i++)
                {
                    if (storeItem.contents[i].itemType == StoreItemType.Magnet)
                    {
                        GameController.GetInstance.PowerupController.UpdateMagnetBalance(storeItem.contents[i].quantity);   
                    }
                    if (storeItem.contents[i].itemType == StoreItemType.GameCurrency)
                    {
                        GameCurrencyBalance += storeItem.contents[i].quantity;
                    }
                    if (storeItem.contents[i].itemType == StoreItemType.SpeedBoost)
                    {
                        GameController.GetInstance.PowerupController.UpdateSpeedBoostBalance(storeItem.contents[i].quantity);
                    }
                    if (storeItem.contents[i].itemType == StoreItemType.NoAds)
                    {
                        AdController.GetInstance.PurchasedNoADsPack();
                    }
                }
                OnPurchaseSuccess?.Invoke(true);
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            OnPurchaseSuccess?.Invoke(false);
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
            //        //        () => { Debug.Log("Ok was pressed"); });
            //#endif
            //            }
            //            return;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseSuccess?.Invoke(false);
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
            //        //        () => { Debug.Log("Ok was pressed"); });
            //#endif
            //            }
            //            return;
        }
        #endregion

        #region GetStore_Products
        public StoreProduct GetStoreProduct(int index)
        {
            foreach (var product in storeDatabase.StoreProducts)
            {
                if (product.index == index)
                {
                    return product;
                }
            }
            return null;
        }
        public StoreProduct GetStoreProduct(string id)
        {
            foreach (var product in storeDatabase.StoreProducts)
            {
                if (string.Equals(product.Id, id, System.StringComparison.OrdinalIgnoreCase))
                {
                    return product;
                }
            }
            return null;
        }
        public string GetProductID(int index)
        {
            foreach (var product in storeDatabase.StoreProducts)
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
