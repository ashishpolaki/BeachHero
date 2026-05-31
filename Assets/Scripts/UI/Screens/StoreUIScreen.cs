using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class StoreUIScreen : BaseScreen
    {
        [SerializeField] private Button homeButton;
        [SerializeField] private Transform content;
        private int currentPurchaseIndex;

        public RealMoneyProductUI[] realMoneyProductsList;
        public GameCurrencyProductUI[] gameCurrencyProductsList;
        public RewardAdItemUI[] rewardAdItems;

        #region BaseScreen Overrides
        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            InitializeIAPItems();
            InitializeRewardedADItems();
            InitializeGameCurrencyProducts();
            AddListener();
        }
        public override void Close()
        {
            base.Close();
            RemoveListener();
        }
        #endregion

        #region Initialize
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
                GameCurrencyProduct product = GameController.GetInstance.StoreController.GetGameCurrencyProduct(gameCurrencyProductsList[i].index);
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
                if (rewardAdItems[index].itemType == StoreItemType.GameCurrency)
                {
                    GameController.GetInstance.StoreController.IncrementGameCurrencyBalance(rewardAdItems[index].quantity);
                }
                else if (rewardAdItems[index].itemType == StoreItemType.Magnet)
                {
                    GameController.GetInstance.PowerupController.UpdateMagnetBalance(rewardAdItems[index].quantity);
                }
                else if (rewardAdItems[index].itemType == StoreItemType.SpeedBoost)
                {
                    GameController.GetInstance.PowerupController.UpdateSpeedBoostBalance(rewardAdItems[index].quantity);
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

        private void GameCurrencyPurchaseButton(int index)
        {
            currentPurchaseIndex = index;
            GameController.GetInstance.StoreController.BuyStoreItemWithGameCurrency(currentPurchaseIndex);
        }

        private void RealMoneyPurchaseButton(int index)
        {
            currentPurchaseIndex = index;
            GameController.GetInstance.StoreController.PurchaseWithRealMoney(currentPurchaseIndex, PurchaseItemType.StoreProduct);
        }
    }
}
