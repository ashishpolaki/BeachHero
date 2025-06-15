using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "InGameCurrencyStoreItemSO", menuName = "Scriptable Objects/Store/InGameCurrencyStoreItemSO")]
    public class InGameCurrencyStoreItemSO : StoreItemSO
    {
        [SerializeField] private int quantity;

        public int Quantity => quantity;
    }
}
