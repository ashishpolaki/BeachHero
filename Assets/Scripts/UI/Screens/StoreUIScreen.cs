using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    [System.Serializable]
    public struct RewardAdItemUI
    {
        public StoreItemType itemType;
        public int quantity;
        public TextMeshProUGUI quantityText;
        public Button watchAdButton;
    }
    public class StoreUIScreen : BaseScreen
    {
        [SerializeField] private Button homeButton;
        [SerializeField] private Transform content;
        private int currentPurchaseIndex;

        public StoreProductUI[] storeProducts;
        public RewardAdItemUI[] rewardAdItems;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            InitializeIAPItems();
            InitializeRewardedADItems();
            AddListener();
        }
        public override void Close()
        {
            base.Close();
            RemoveListener();
        }

        private void OpenHome()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
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
            for (int i = 0; i < storeProducts.Length; i++)
            {
                StoreProduct product = GameController.GetInstance.StoreController.GetStoreProduct(storeProducts[i].index);
                if (product != null)
                {
                    int productIndex = i;
                    if (product.isRealMoney)
                    {
                        storeProducts[productIndex].realMoneyPriceText.text = product.realMoneyCost;
                    }
                    if (product.isGameCurrency)
                    {
                        storeProducts[productIndex].gameCurrencyPriceText.text = product.gameCurrencyCost.ToString();
                    }

                    // Set Quantity Text for Product Contents
                    if (storeProducts[productIndex].contentUis.Length > 0)
                    {
                        for (int j = 0; j < storeProducts[productIndex].contentUis.Length; j++)
                        {
                            int contentUIIndex = j;
                            if (storeProducts[productIndex].contentUis[contentUIIndex].itemType == product.contents[contentUIIndex].itemType)
                            {
                                if (storeProducts[productIndex].contentUis[contentUIIndex].quantityText != null)
                                {
                                    storeProducts[productIndex].contentUis[contentUIIndex].quantityText.text = product.contents[contentUIIndex].quantity.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddListener()
        {
            GameController.GetInstance.StoreController.OnStoreItemPurchaseAction += OnPurchaseSuccess;
            homeButton.ButtonRegister(OpenHome);
            //Store Products
            for (int i = 0; i < storeProducts.Length; i++)
            {
                // Game Currency button
                if (storeProducts[i].gameCurrencyPurchaseButton != null)
                {
                    int index = i;
                    storeProducts[index].gameCurrencyPurchaseButton.ButtonRegister(() => GameCurrencyPurchaseButton(storeProducts[index].index));
                }
                // Real Money button
                if (storeProducts[i].realMoneyPurchaseButton != null)
                {
                    int index = i;
                    storeProducts[index].realMoneyPurchaseButton.ButtonRegister(() => RealMoneyPurchaseButton(storeProducts[index].index));
                }
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
            homeButton.ButtonDeRegister(OpenHome);
            //Store Products
            for (int i = 0; i < storeProducts.Length; i++)
            {
                if (storeProducts[i].gameCurrencyPurchaseButton != null)
                {
                    storeProducts[i].gameCurrencyPurchaseButton.ButtonDeRegisterAll();
                }
                if (storeProducts[i].realMoneyPurchaseButton != null)
                {
                    storeProducts[i].realMoneyPurchaseButton.ButtonDeRegisterAll();
                }
            }
            // Rewarded AD Items
            for (int i = 0; i < rewardAdItems.Length; i++)
            {
                if (rewardAdItems[i].watchAdButton != null)
                {
                    rewardAdItems[i].watchAdButton.ButtonDeRegisterAll();
                }
            }
        }

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
                else if(rewardAdItems[index].itemType == StoreItemType.SpeedBoost)
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
