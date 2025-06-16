using UnityEngine;

namespace BeachHero
{
    public class SkinController : MonoBehaviour
    {
        [SerializeField] private BoatSkinDatabaseSO boatSkinsDatabase;

        public BoatSkinDatabaseSO BoatSkinsDatabase
        {
            get => boatSkinsDatabase;
        }
    }
}
