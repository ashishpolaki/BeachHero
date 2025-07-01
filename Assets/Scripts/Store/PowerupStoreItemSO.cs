using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "PowerupStoreItemSO", menuName = "Scriptable Objects/Store/Powerup")]
    public class PowerupStoreItemSO : StoreItemSO
    {
        [SerializeField] private PowerupType powerupType;
        [SerializeField] private int quantity;
        [SerializeField] private int inGameCurrencyCost;

        public PowerupType PowerupType => powerupType;
        public int Quantity => quantity;
        public int InGameCurrencyCost => inGameCurrencyCost;
    }
}
