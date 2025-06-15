using UnityEngine;
using UnityEngine.Purchasing;

namespace BeachHero
{
    public class StoreItemSO : ScriptableObject, IStoreItem
    {
        [SerializeField] protected ProductType productType;
        [SerializeField] protected int productId;
        [SerializeField] protected string productName;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected int price;
        [SerializeField] protected StoreItemUI uiPrefab;

        public ProductType ProductType => ProductType;
        public string ItemName => productName;
        public Sprite ItemIcon => icon;
        public int ItemPrice => price;
        public StoreItemUI UIPrefab => uiPrefab;
        public int ProductId => productId;
    }
}
