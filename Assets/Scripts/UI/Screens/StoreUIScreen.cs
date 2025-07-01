using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeachHero
{
    public class StoreUIScreen : BaseScreen
    {
        [SerializeField] private Button homeButton;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject purchasePanel;
        [SerializeField] private TextMeshProUGUI purchaseDescriptionText;
        private int currentPurchaseIndex;

        public StoreProductUI[] storeProducts;

        public override void Open(ScreenTabType screenTabType)
        {
            base.Open(screenTabType);
            InitializeIAPItems();
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
            GameController.GetInstance.StoreController.OnPurchaseSuccess += OnPurchaseSuccess;
            homeButton.onClick.AddListener(OpenHome);
            for (int i = 0; i < storeProducts.Length; i++)
            {
                // Game Currency button
                if (storeProducts[i].gameCurrencyPurchaseButton != null)
                {
                    int index = i;
                    storeProducts[index].gameCurrencyPurchaseButton.onClick.AddListener(() => GameCurrencyPurchaseButton(storeProducts[index].index));
                }
                // Real Money button
                if (storeProducts[i].realMoneyPurchaseButton != null)
                {
                    int index = i;
                    storeProducts[index].realMoneyPurchaseButton.onClick.AddListener(() => RealMoneyPurchaseButton(storeProducts[index].index));
                }
            }
        }

        private void RemoveListener()
        {
            GameController.GetInstance.StoreController.OnPurchaseSuccess -= OnPurchaseSuccess;
            homeButton.onClick.RemoveAllListeners();
            for (int i = 0; i < storeProducts.Length; i++)
            {
                if (storeProducts[i].gameCurrencyPurchaseButton != null)
                {
                    storeProducts[i].gameCurrencyPurchaseButton.onClick.RemoveAllListeners();
                }
                if (storeProducts[i].realMoneyPurchaseButton != null)
                {
                    storeProducts[i].realMoneyPurchaseButton.onClick.RemoveAllListeners();
                }
            }
        }

        private void OnPurchaseSuccess(bool _val)
        {
            purchasePanel.SetActive(true);
            purchaseDescriptionText.text = _val ? StringUtils.PRODUCT_PURCHASED_SUCCESS : StringUtils.PRODUCT_PURCHASE_FAILED;
        }

        private void GameCurrencyPurchaseButton(int index)
        {
            currentPurchaseIndex = index;
            GameController.GetInstance.StoreController.PurchaseWithGameCurrency(currentPurchaseIndex);
        }

        private void RealMoneyPurchaseButton(int index)
        {
            currentPurchaseIndex = index;
            GameController.GetInstance.StoreController.PurchaseWithRealMoney(currentPurchaseIndex);
        }
    }
}
