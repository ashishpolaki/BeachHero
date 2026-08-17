using System;
using System.Collections.Generic;
using System.Linq;
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
    [Serializable]
    public class IAPPayData
    {
        public string Payload;
        public string Store;
        public string TransactionID;
    }
    [Serializable]
    public class IAPPayload
    {
        public string json;
        public string signature;
        public IAPPayloadData payloadData;
    }
    [Serializable]
    public class IAPPayloadData
    {
        public string orderId;
        public string packageName;
        public string productId;
        public long purchaseTime;
        public int purchaseState;
        public string purchaseToken;
        public int quantity;
        public bool acknowledged;
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
        private int coinBalance;
        private string defaultPrice = "$0.01"; // Default price for products that do not have a real money cost set.
        #endregion

        #region Actions
        public event Action<bool> OnStoreItemPurchaseAction;
        public event Action OnBoatPurchaseFail;
        public event Action OnCoinsBalanceChange;
        public event Action OnNoAdsPurchased;
        #endregion

        #region Properties
        public int CoinsBalance
        {
            get => SaveSystem.CurrentData != null ? SaveSystem.CurrentData.coins : IntUtils.DEFAULT_COINS_BALANCE;
            private set
            {
                coinBalance = value;
                SaveSystem.CurrentData.coins = coinBalance;
                SaveSystem.SaveGameData();
                PlayGamesController.GetInstance.SaveDataInCloud();
                OnCoinsBalanceChange?.Invoke();
            }
        }
        #endregion

        #region Initialisation
        public void Init()
        {
            InitializeServices();
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
            //foreach (var boatSkin in boatSkinDatabase.BoatSkins)
            //{
            //    if (boatSkin.IsRealMoney && !boatSkin.IsDefaultBoat && !string.IsNullOrEmpty(boatSkin.ID))
            //    {
            //        catalogProvider.AddProduct(boatSkin.ID, boatSkin.ProductType);
            //    }
            //}
            await m_StoreController.Connect();
            // Fetch products from store
            catalogProvider.FetchProducts(list => m_StoreController.FetchProducts(list));
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
                    //if (item.definition.id.Contains("boat", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    BoatSkinSO boatSkin = GameController.GetInstance.SkinController.GetBoatSkinByID(item.definition.id);
                    //    if (boatSkin != null)
                    //    {
                    //        boatSkin.SetRealMoneyCost(item.metadata.localizedPriceString);
                    //    }
                    //}
                    //else
                    //{
                    //Store Product
                    RealMoneyProduct storeProduct = GetRealMoneyProduct(item.definition.id);
                    if (storeProduct != null)
                    {
                        storeProduct.realMoneyCost = item.metadata.localizedPriceString;
                    }
                    //   }
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
                string receipt = order.Info.Receipt;
                if (string.IsNullOrEmpty(receipt))
                {
                    DebugUtils.LogError("Receipt is null or empty for Store Product purchase.");
                    return;
                }
                var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
                if (product == null)
                {
                    DebugUtils.LogError("Product is null for Store Product purchase.");
                    return;
                }
                m_StoreController.ConfirmPurchase(order);
            }
            //else if (currentPurchaseItemType == PurchaseItemType.BoatSkin)
            //{
            //    GameController.GetInstance.SkinController.UnlockBoatSkin(currentIndex);
            //}
        }
        private void OnPUrchaseConfirm(Order confirmedOrder)
        {
            int quantity = GetPurchaseQuantity(confirmedOrder);
            StoreItemBought(quantity);
        }
        private int GetPurchaseQuantity(Order confirmedOrder)
        {
            int quantity = 1; // Default quantity
            string receipt = confirmedOrder.Info.Receipt;
            if (!string.IsNullOrEmpty(receipt))
            {
                var paydata = JsonUtility.FromJson<IAPPayData>(receipt);
                if (paydata.Store != "fake")
                {
                    IAPPayload payload = JsonUtility.FromJson<IAPPayload>(paydata.Payload);
                    IAPPayloadData payloadData = JsonUtility.FromJson<IAPPayloadData>(payload.json);
                    quantity = payloadData.quantity;
                }
            }
            return quantity;
        }

        private void OnPurchaseDeferred(DeferredOrder deferredOrder)
        {
            DebugUtils.Log($"OnPurchaseDeferred: {deferredOrder.Info.PurchasedProductInfo}");
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
        }
        private void OnPurchasesFetched(Orders orders)
        {
            // Store the confirmed orders for later access
            foreach (var order in orders.ConfirmedOrders)
            {
                foreach (var product in order.CartOrdered.Items())
                {
                    var storeProduct = product.Product;
                    DebugUtils.Log($"Fetched purchase for Store : {storeProduct.definition.id}, Quantity: {product.Quantity}");
                    if (storeProduct.definition.id == "no_ads")
                    {
                        NoAdsPurchased();
                    }
                }
            }
        }
        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            DebugUtils.LogError($"OnPurchasesFetchFailed for Store Product: {failure.FailureReason}");
        }
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
            if (CoinsBalance >= boatSkin.CoinCost)
            {
                skinController.UnlockBoatSkin(index);
                DeductCoinsBalance(boatSkin.CoinCost);
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
            if (CoinsBalance >= boatSkin.SkinColors[colorIndex].coinCost)
            {
                skinController.UnlockBoatSkinColor(boatIndex, colorIndex);
                DeductCoinsBalance(boatSkin.SkinColors[colorIndex].coinCost);
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

            //if (currentPurchaseItemType == PurchaseItemType.BoatSkin)
            //{
            //    string productID = GameController.GetInstance.SkinController.GetBoatSkinByIndex(currentIndex).ID;
            //    m_StoreController.PurchaseProduct(productID);
            //}
            //else
            //{
            m_StoreController.PurchaseProduct(GetRealMoneyProductID(currentIndex));
            //}
        }

        private void StoreItemBought(int quantity)
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
                            GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.Shield, reward.quantity * quantity);
                            break;

                        case StoreItemType.SpeedBoost:
                            GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.SpeedBoost, reward.quantity * quantity);
                            break;

                        case StoreItemType.Coins:
                            CoinsBalance += (reward.quantity * quantity);
                            break;

                        case StoreItemType.NoAds:
                            NoAdsPurchased();
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

        public void NoAdsPurchased()
        {
            SaveSystem.SaveBool(StringUtils.NO_ADS_PURCHASED, true);
            OnNoAdsPurchased?.Invoke();
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
