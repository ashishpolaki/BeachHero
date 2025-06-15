using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BeachHero
{
    public class StoreItemUI : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI realMoneyText;
        [SerializeField] private Button realMoneyBuyButton;

        private int itemId;

        public virtual void Initialize(StoreItemSO storeItemSO)
        {
            itemId = storeItemSO.ProductId;
            itemImage.sprite = storeItemSO.ItemIcon;
            itemNameText.text = storeItemSO.ItemName;
            realMoneyText.text = $"Rs.{storeItemSO.ItemPrice}"; 
        }
        private void OnEnable()
        {
            AddListeners();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        public virtual void AddListeners()
        {
            realMoneyBuyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        public virtual void RemoveListeners()
        {
            realMoneyBuyButton.onClick.RemoveListener(OnBuyButtonClicked);
        }

        private void OnBuyButtonClicked()
        {
        }

    }
}
