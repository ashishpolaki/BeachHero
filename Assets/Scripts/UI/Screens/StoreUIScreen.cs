using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class StoreUIScreen : BaseScreen
    {
        [SerializeField] private Button homeButton;
        [SerializeField] private Transform content;
        [SerializeField] private RectTransform storeBoardBg;
        [SerializeField] private GameObject noAdsPackObject;
        [SerializeField] private float extraBgHeight = 200f;
        private int currentPurchaseIndex;

        public RealMoneyProductUI[] realMoneyProductsList;
        public GameCurrencyProductUI[] gameCurrencyProductsList;
        public RewardAdItemUI[] rewardAdItems;

        #region BaseScreen Overrides
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            AdjustStoreBoardBgHeight();
            InitializeIAPItems();
            InitializeRewardedADItems();
            InitializeGameCurrencyProducts();
            AddListener();
            NoAdsPackPurchase();
        }
        public override void Close()
        {
            base.Close();
            GameController.GetInstance.SetPreviousGameState();
            RemoveListener();
        }
        #endregion

        #region Initialize
        private void AdjustStoreBoardBgHeight()
        {
            if (storeBoardBg != null && UIController.GetInstance != null && UIController.GetInstance.Canvas != null)
            {
                RectTransform canvasRect = UIController.GetInstance.Canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    float canvasHeight = canvasRect.rect.height;
                    float decreaseBgHeight = Screen.height - Screen.safeArea.height;
                    storeBoardBg.sizeDelta = new Vector2(storeBoardBg.sizeDelta.x, canvasHeight + extraBgHeight - decreaseBgHeight);
                }
            }
        }

        private void InitializeRewardedADItems()
        {
            for (int i = 0; i < rewardAdItems.Length; i++)
            {
                RewardAdItemUI itemUI = rewardAdItems[i];
                if (itemUI.quantityText != null)
                {
                    itemUI.quantityText.text = itemUI.quantity.ToString();
                }
            }
        }
        private void InitializeIAPItems()
        {
            for (int i = 0; i < realMoneyProductsList.Length; i++)
            {
                RealMoneyProduct product = GameController.GetInstance.StoreController.GetRealMoneyProduct(realMoneyProductsList[i].index);
                if (product != null)
                {
                    int productIndex = i;
                    realMoneyProductsList[productIndex].priceText.text = product.realMoneyCost;

                    // Set Quantity Text 
                    if (realMoneyProductsList[productIndex].storeRewardUIs.Length > 0)
                    {
                        for (int j = 0; j < realMoneyProductsList[productIndex].storeRewardUIs.Length; j++)
                        {
                            int contentUIIndex = j;
                            if (realMoneyProductsList[productIndex].storeRewardUIs[contentUIIndex].itemType == product.rewards[contentUIIndex].itemType)
                            {
                                if (realMoneyProductsList[productIndex].storeRewardUIs[contentUIIndex].quantityText != null)
                                {
                                    realMoneyProductsList[productIndex].storeRewardUIs[contentUIIndex].quantityText.text = product.rewards[contentUIIndex].quantity.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitializeGameCurrencyProducts()
        {
            for (int i = 0; i < gameCurrencyProductsList.Length; i++)
            {
                GameCurrencyProduct product = GameController.GetInstance.StoreController.GetCoinsProduct(gameCurrencyProductsList[i].index);
                if (product != null)
                {
                    int productIndex = i;
                    // Set Game Currency Price Text
                    gameCurrencyProductsList[productIndex].priceText.text = product.gameCurrencyCost.ToString();

                    if (gameCurrencyProductsList[productIndex].storeRewardUIs.Length > 0)
                    {
                        for (int j = 0; j < gameCurrencyProductsList[productIndex].storeRewardUIs.Length; j++)
                        {
                            int contentUIIndex = j;
                            if (gameCurrencyProductsList[productIndex].storeRewardUIs[contentUIIndex].itemType == product.rewards[contentUIIndex].itemType)
                            {
                                if (gameCurrencyProductsList[productIndex].storeRewardUIs[contentUIIndex].quantityText != null)
                                {
                                    gameCurrencyProductsList[productIndex].storeRewardUIs[contentUIIndex].quantityText.text = product.rewards[contentUIIndex].quantity.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Button Listeners
        private void AddListener()
        {
            GameController.GetInstance.StoreController.OnStoreItemPurchaseAction += OnPurchaseSuccess;
            GameController.GetInstance.StoreController.OnNoAdsPurchased += NoAdsPackPurchase;
            homeButton.ButtonRegister(OpenHomeButtonListener);
            //Real Money Products
            for (int i = 0; i < realMoneyProductsList.Length; i++)
            {
                if (realMoneyProductsList[i].purchaseButton != null)
                {
                    int index = i;
                    realMoneyProductsList[index].purchaseButton.ButtonRegister(() => RealMoneyPurchaseButton(realMoneyProductsList[index].index));
                }
            }

            // Game Currency Products
            for (int i = 0; i < gameCurrencyProductsList.Length; i++)
            {
                int index = i;
                gameCurrencyProductsList[index].purchaseButton.ButtonRegister(() => GameCurrencyPurchaseButton(gameCurrencyProductsList[index].index));
            }

            // Rewarded AD Items
            for (int i = 0; i < rewardAdItems.Length; i++)
            {
                if (rewardAdItems[i].watchAdButton != null)
                {
                    int index = i;
                    rewardAdItems[index].watchAdButton.ButtonRegister(() =>
                    {
                        HandleRewardAdButton(index);
                    });
                }
            }
        }

        private void RemoveListener()
        {
            GameController.GetInstance.StoreController.OnStoreItemPurchaseAction -= OnPurchaseSuccess;
            GameController.GetInstance.StoreController.OnNoAdsPurchased -= NoAdsPackPurchase;
            homeButton.ButtonDeRegister(OpenHomeButtonListener);
            //Real Money Products
            for (int i = 0; i < realMoneyProductsList.Length; i++)
            {
                if (realMoneyProductsList[i].purchaseButton != null)
                {
                    int index = i;
                    realMoneyProductsList[index].purchaseButton.ButtonDeRegisterAll();
                }
            }

            // Game Currency Products
            for (int i = 0; i < gameCurrencyProductsList.Length; i++)
            {
                int index = i;
                gameCurrencyProductsList[index].purchaseButton.ButtonDeRegisterAll();
            }

            // Rewarded AD Items
            for (int i = 0; i < rewardAdItems.Length; i++)
            {
                if (rewardAdItems[i].watchAdButton != null)
                {
                    int index = i;
                    rewardAdItems[index].watchAdButton.ButtonDeRegisterAll();
                }
            }
        }
        private void OpenHomeButtonListener()
        {
            Close();
        }
        #endregion

        private void HandleRewardAdButton(int index)
        {
            AdController.GetInstance.ShowRewardedAd((reward) =>
            {
                if (rewardAdItems[index].itemType == StoreItemType.Coins)
                {
                    GameController.GetInstance.StoreController.IncrementCoinsBalance(rewardAdItems[index].quantity);
                }
                else if (rewardAdItems[index].itemType == StoreItemType.Shield)
                {
                    GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.Shield, rewardAdItems[index].quantity);
                }
                else if (rewardAdItems[index].itemType == StoreItemType.SpeedBoost)
                {
                    GameController.GetInstance.PowerupController.UpdatePowerupBalance(PowerupType.SpeedBoost, rewardAdItems[index].quantity);
                }
            });
        }

        private void OnPurchaseSuccess(bool _val)
        {
            if (_val)
            {
                UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push, ScreenTabType.PurchasSuccess);
            }
            else
            {
                UIController.GetInstance.ScreenEvent(ScreenType.Purchase, UIScreenEvent.Push, ScreenTabType.PurchasFail);
            }
        }
        private void NoAdsPackPurchase()
        {
            if (AdController.GetInstance.NoAdsPurchased())
            {
                noAdsPackObject.SetActive(false);
            }
        }

        private void GameCurrencyPurchaseButton(int index)
        {
            currentPurchaseIndex = index;
            GameController.GetInstance.StoreController.BuyStoreItemWithCoins(currentPurchaseIndex);
        }

        private void RealMoneyPurchaseButton(int index)
        {
            currentPurchaseIndex = index;
            GameController.GetInstance.StoreController.PurchaseWithRealMoney(currentPurchaseIndex, PurchaseItemType.StoreProduct);
        }
    }
}
