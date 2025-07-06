using System;
using UnityEngine;

namespace BeachHero
{
    public class SkinController : MonoBehaviour
    {
        [SerializeField] private BoatSkinDatabaseSO boatSkinsDatabase;

        public event Action<int> OnBoatSkinPurchased;
        public event Action OnPurchaseFail;

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

            DebugUtils.LogError("BoatSkinsDatabase is null or index is out of range.");
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
            return SaveSystem.LoadInt(StringUtils.CURRENT_BOAT_INDEX, 1);
        }

        private void OnBoatPurchaseFail()
        {
            DebugUtils.LogError("Not enough game currency to purchase this skin.");
            OnPurchaseFail.Invoke();
        }

        public void SkinUnlocked(int index)
        {
            SaveSystem.SaveBool(StringUtils.BOAT_SKIN_UNLOCKED + index, true);
            OnBoatSkinPurchased?.Invoke(index);
        }

        public void TryPurchaseWithGameCurrency(int index)
        {
            BoatSkinSO boatSkin = GetBoatSkinByIndex(index);
            if (GameController.GetInstance.StoreController.GameCurrencyBalance >= boatSkin.InGameCurrencyCost)
            {
                SkinUnlocked(index);
                GameController.GetInstance.StoreController.DeductGameCurrencyBalance(boatSkin.InGameCurrencyCost);
            }
            else
            {
                OnBoatPurchaseFail();
            }
        }

        public int GetBoatColorAdsCount(int boatIndex, int colorIndex)
        {
            int currentAds = SaveSystem.LoadInt($"{StringUtils.BOAT_SKIN_COLOR_UNLOCK}{boatIndex}_{colorIndex}", 0);
            return currentAds;
        }

        public void SetBoatColorAdsCount(int boatIndex, int colorIndex)
        {
            int count = GetBoatColorAdsCount(boatIndex, colorIndex) + 1;
            SaveSystem.SaveInt($"{StringUtils.BOAT_SKIN_COLOR_UNLOCK}{boatIndex}_{colorIndex}", count);
        }
    }
}
