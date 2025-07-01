using UnityEngine;
using UnityEngine.Purchasing;

namespace BeachHero
{
    public interface IStoreItem
    {
        public ProductType Type { get; }
        public string Name { get; }
        public Sprite Icon { get; }
        public int Price { get; }
    }
}
