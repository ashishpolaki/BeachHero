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

        private string itemId;

        public virtual void Initialize(StoreItemSO storeItemSO)
        {
            itemId = storeItemSO.Id;
            itemImage.sprite = storeItemSO.Icon;
            itemNameText.text = storeItemSO.Name;
            realMoneyText.text = $"Rs.{storeItemSO.Price}"; 
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
         //   GameController.GetInstance.StoreController.Purchase(itemId);
        }
    }
}
