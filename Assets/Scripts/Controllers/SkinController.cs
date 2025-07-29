using System;
using UnityEngine;

namespace BeachHero
{
    public class SkinController : MonoBehaviour
    {
        [SerializeField] private BoatSkinDatabaseSO boatSkinsDatabase;

        public event Action<int> OnSkinPurchased;
        public event Action<int,int> OnSkinColorPurchased;

        public BoatSkinDatabaseSO BoatSkinsDatabase
        {
            get => boatSkinsDatabase;
        }

        #region Get Methods
        public bool IsBoatSkinUnlocked(int boatIndex)
        {
            BoatSkinSO boatSkin = GetBoatSkinByIndex(boatIndex);
            if (boatSkin.IsDefaultBoat)
            {
                return true; // Default boats are always unlocked
            }
            if (SaveSystem.LoadBool(StringUtils.BOAT_SKIN_UNLOCKED + boatIndex, false))
            {
                return true;
            }
            return false;
        }

        public bool IsBoatSkinColorUnlocked(int boatIndex, int colorIndex)
        {
            BoatSkinSO boatSkin = GetBoatSkinByIndex(boatIndex);
            if (boatSkin.SkinColors[colorIndex].isDefault)
            {
                return true;
            }
            if (SaveSystem.LoadBool($"{StringUtils.BOAT_SKIN_COLOR_UNLOCK}{boatIndex}_{colorIndex}", false))
            {
                return true;
            }
            return false;
        }

        public BoatSkinSO GetBoatSkinByIndex(int index)
        {
            //foreach
            foreach (var skin in boatSkinsDatabase.BoatSkins)
            {
                if (skin.Index == index)
                    return skin;
            }

            DebugUtils.LogError("BoatSkinsDatabase is null or index is out of range.");
            return null;
        }
        public BoatSkinSO GetBoatSkinByID(string id)
        {
            foreach (var skin in boatSkinsDatabase.BoatSkins)
            {
                if (skin.ID == id)
                    return skin;
            }
            DebugUtils.LogError($"BoatSkin with ID {id} not found in the database.");
            return null;
        }
        public float GetSelectedBoatSpeed()
        {
            int currentBoatIndex = GetSavedBoatIndex();
            return GetBoatSkinByIndex(currentBoatIndex).Speed;
        }
        public GameObject GetSelectedBoatPrefab()
        {
            int currentBoatIndex = GetSavedBoatIndex();
            return GetBoatSkinByIndex(currentBoatIndex).BoatPrefab;
        }
        public int GetSavedBoatIndex()
        {
            return SaveSystem.LoadInt(StringUtils.CURRENT_BOAT_INDEX, IntUtils.DEFAULT_BOAT_INDEX);
        }
        public int GetSavedBoatColorIndex(int boatIndex)
        {
            return SaveSystem.LoadInt(StringUtils.CURRENT_BOAT_COLOR_INDEX + boatIndex, IntUtils.DEFAULT_BOAT_COLOR_INDEX);
        }
        #endregion

        #region Set Methods
        public void SetSavedBoatIndex(int boatIndex, int colorIndex = 0)
        {
            SaveSystem.SaveInt(StringUtils.CURRENT_BOAT_INDEX, boatIndex);
            SaveSystem.SaveInt(StringUtils.CURRENT_BOAT_COLOR_INDEX + boatIndex, colorIndex);
        }
        public void UnlockBoatSkin(int index)
        {
            SaveSystem.SaveBool(StringUtils.BOAT_SKIN_UNLOCKED + index, true);
            SetSavedBoatIndex(index, 0); // Default color index is 0
            OnSkinPurchased?.Invoke(index);
        }
        public void UnlockBoatSkinColor(int boatindex,int colorIndex)
        {
            SaveSystem.SaveBool($"{StringUtils.BOAT_SKIN_COLOR_UNLOCK}{boatindex}_{colorIndex}", true);
            SetSavedBoatIndex(boatindex, colorIndex);
            OnSkinColorPurchased?.Invoke(boatindex, colorIndex);
        }
        #endregion
    }
}
