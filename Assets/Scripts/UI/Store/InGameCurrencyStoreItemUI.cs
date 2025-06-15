using UnityEngine;
using TMPro;

namespace BeachHero
{
    public class InGameCurrencyStoreItemUI : StoreItemUI
    {
        [SerializeField] private TextMeshProUGUI itemQuantityText;

        public override void Initialize(StoreItemSO storeItemSO)
        {
            base.Initialize(storeItemSO);
            if (storeItemSO is InGameCurrencyStoreItemSO inGameCurrencyStoreItem)
            {
                itemQuantityText.text = $"Quantity : {inGameCurrencyStoreItem.Quantity}";
            }
        }
    }
}
