using UnityEngine;

namespace Watermelon
{
    public class IAPSettings : ScriptableObject
    {
        [SerializeField, Hide] IAPItem[] storeItems;
        public IAPItem[] StoreItems => storeItems;
    }
}