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
        //Get boatskins with index
        public BoatSkinSO GetBoatSkinByIndex(int index)
        {
            //foreach
            foreach (var item in boatSkinsDatabase.BoatSkins)
            {
                if (item.Index == index)
                    return item;
            }

            Debug.LogError("BoatSkinsDatabase is null or index is out of range.");
            return null;
        }

        public float GetSpeed()
        {
            int currentBoatIndex = GetCurrentSelectedBoatIndex();
            return GetBoatSkinByIndex(currentBoatIndex).Speed;
        }

        public GameObject GetCurrentSelectedBoat()
        {
            int currentBoatIndex = GetCurrentSelectedBoatIndex();
            return GetBoatSkinByIndex(currentBoatIndex).BoatPrefab;
        }

        public int GetCurrentSelectedBoatIndex()
        {
            return SaveController.LoadInt(StringUtils.CURRENT_BOAT_INDEX, 1);
        }
    }
}
