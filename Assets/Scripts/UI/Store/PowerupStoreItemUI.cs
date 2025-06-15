using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BeachHero
{
    public class PowerupStoreItemUI : StoreItemUI
    {
        [SerializeField] private TextMeshProUGUI itemQuantityText;
        [SerializeField] private TextMeshProUGUI inGameCurrencyText;
        [SerializeField] private Button inGameCurrencyBuyButton;

        public override void Initialize(StoreItemSO storeItemSO)
        {
            base.Initialize(storeItemSO);
            if (storeItemSO is PowerupStoreItemSO powerupStoreItem)
            {
                inGameCurrencyText.text = powerupStoreItem.InGameCurrencyCost.ToString();
                itemQuantityText.text = $"Quantity : {powerupStoreItem.Quantity}";
            }
        }
        private void OnDestroy()
        {
            inGameCurrencyBuyButton.onClick.RemoveListener(OnInGameCurrencyBuy);
        }
        public override void AddListeners()
        {
            base.AddListeners();
            inGameCurrencyBuyButton.onClick.AddListener(OnInGameCurrencyBuy);
        }
        public override void RemoveListeners()
        {
            base.RemoveListeners();
            inGameCurrencyBuyButton.onClick.RemoveListener(OnInGameCurrencyBuy);
        }
        private void OnInGameCurrencyBuy()
        {
            // Add logic to handle the purchase of the item
        }
    }
}
