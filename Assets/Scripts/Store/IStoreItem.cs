using UnityEngine;
using UnityEngine.Purchasing;

namespace BeachHero
{
    public interface IStoreItem
    {
        public ProductType ProductType { get; }
        public string ItemName { get; }
        public Sprite ItemIcon { get; }
        public int ItemPrice { get; }
    }
}
