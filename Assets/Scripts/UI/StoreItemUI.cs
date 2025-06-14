using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BeachHero
{
    public class StoreItemUI : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemQuantityText;
        [SerializeField] private TextMeshProUGUI inGameCurrencyText;
        [SerializeField] private TextMeshProUGUI realMoneyText;
        [SerializeField] private Button inGameCurrencyBuyButton;
        [SerializeField] private Button realMoneyBuyButton;
        private int itemId;

        public void Initialize(int id)
        {
            itemId = id;
            itemNameText.text = "Item " + id; // Example item name
            inGameCurrencyBuyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        private void OnBuyButtonClicked()
        {
            Debug.Log("Buying item with ID: " + itemId);
            // Add logic to handle the purchase of the item
        }

    }
}
