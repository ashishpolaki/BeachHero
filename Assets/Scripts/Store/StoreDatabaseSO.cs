using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "StoreDatabaseSO", menuName = "Scriptable Objects/Store/StoreDatabaseSO")]
    public class StoreDatabaseSO : ScriptableObject
    {
        [SerializeField] private StoreItemSO[] storeItems;

        public StoreItemSO[] StoreItems
        {
            get => storeItems;
        }
    }
}
