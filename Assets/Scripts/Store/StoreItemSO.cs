using UnityEngine;
using UnityEngine.Purchasing;

namespace BeachHero
{
    public class StoreItemSO : ScriptableObject, IStoreItem
    {
        [SerializeField] protected ProductType productType;
        [SerializeField] protected string productId;
        [SerializeField] protected string productName;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected int price;
        [SerializeField] protected StoreItemUI uiPrefab;

        public ProductType Type => productType;
        public string Name => productName;
        public string Id => productId;
        public Sprite Icon => icon;
        public int Price => price;
        public StoreItemUI UIPrefab => uiPrefab;
    }
}
