using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "InGameCurrencyStoreItemSO", menuName = "Scriptable Objects/Store/GameCurrency")]
    public class InGameCurrencyStoreItemSO : StoreItemSO
    {
        [SerializeField] private int quantity;

        public int Quantity => quantity;
    }
}
