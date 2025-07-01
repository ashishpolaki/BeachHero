using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "StoreDatabaseSO", menuName = "Scriptable Objects/Store/StoreDatabaseSO")]
    public class StoreDatabaseSO : ScriptableObject
    {
        public StoreProduct[] StoreProducts;
    }
}
