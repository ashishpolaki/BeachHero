using UnityEngine;

namespace BeachHero
{
    public class StoreController : MonoBehaviour
    {
        [SerializeField] private StoreDatabaseSO storeDatabase;

        public StoreDatabaseSO StoreDatabase
        {
            get => storeDatabase;
        }
    }
}
