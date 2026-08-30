using System;
using UnityEngine;

namespace BeachHero
{
    public class SkinController : MonoBehaviour
    {
        [SerializeField] private BoatSkinDatabaseSO boatSkinsDatabase;

        public event Action<int> OnSkinPurchased;
        public event Action<int, int> OnSkinColorPurchased;
        public event Action<bool> OnBoatCustomisationPanelOpen;
        public BoatSkinDatabaseSO BoatSkinsDatabase
        {
            get => boatSkinsDatabase;
        }

        #region Get Methods
        public bool IsBoatSkinUnlocked(int boatIndex)
        {
            BoatSkinSO boatSkin = GetBoatSkinByIndex(boatIndex);
            return SaveSystem.CurrentData != null && SaveSystem.CurrentData.IsBoatUnlocked(boatIndex);
        }


        public bool IsBoatSkinColorUnlocked(int boatIndex, int colorIndex)
        {
            if (colorIndex == 0)
            {
                return true; // Default color is always unlocked
            }

            return SaveSystem.CurrentData != null && SaveSystem.CurrentData.IsBoatColorUnlocked(boatIndex, colorIndex);
        }

        public BoatSkinSO GetBoatSkinByIndex(int index)
        {
            if (boatSkinsDatabase == null || boatSkinsDatabase.BoatSkins == null)
            {
                DebugUtils.LogError("BoatSkinsDatabase is null.");
                return null;
            }

            foreach (var skin in boatSkinsDatabase.BoatSkins)
            {
                if (skin != null && skin.Index == index)
                    return skin;
            }

            DebugUtils.LogError($"BoatSkin with index {index} not found in database.");
            return null;
        }

        public BoatSkinSO GetBoatSkinByID(string id)
        {
            if (boatSkinsDatabase == null || boatSkinsDatabase.BoatSkins == null)
            {
                DebugUtils.LogError("BoatSkinsDatabase is null.");
                return null;
            }

            foreach (var skin in boatSkinsDatabase.BoatSkins)
            {
                if (skin != null && skin.ID == id)
                    return skin;
            }

            DebugUtils.LogError($"BoatSkin with ID {id} not found in the database.");
            return null;
        }

        public float GetSelectedBoatSpeed(int currentBoatIndex)
        {
            var boat = GetBoatSkinByIndex(currentBoatIndex);
            return boat != null ? boat.Speed : 0f;
        }

        public float GetSelectedBoatBoostSpeed()
        {
            int currentBoatIndex = GetSavedBoatIndex();
            var boat = GetBoatSkinByIndex(currentBoatIndex);
            return boat != null ? boat.BoostSpeed : 0f;
        }

        public GameObject GetSelectedBoatPrefab(int currentBoatIndex)
        {
            var boat = GetBoatSkinByIndex(currentBoatIndex);
            return boat != null ? boat.BoatPrefab : null;
        }

        public int GetSavedBoatIndex()
        {
            return SaveSystem.CurrentData != null ? SaveSystem.CurrentData.currentSelectedBoatIndex : IntUtils.DEFAULT_BOAT_INDEX;
        }

        public int GetSavedBoatColorIndex(int boatIndex)
        {
            return SaveSystem.CurrentData != null ? SaveSystem.CurrentData.GetSelectedBoatColorIndex(boatIndex) : IntUtils.DEFAULT_BOAT_COLOR_INDEX;
        }

        public Color[] GetBoatPartColors(int currentBoatIndex, int currentColorIndex)
        {
            BoatSkinSO boatSkin = GetBoatSkinByIndex(currentBoatIndex);
            if (boatSkin != null && boatSkin.SkinColors != null && boatSkin.SkinColors.Length > currentColorIndex && currentColorIndex >= 0)
            {
                return boatSkin.SkinColors[currentColorIndex].ShaderColors;
            }
            DebugUtils.LogError($"No colors found for boat index {currentBoatIndex} and color index {currentColorIndex}.");
            return Array.Empty<Color>();
        }
        #endregion

        #region Set Methods
        public void SetSavedBoatIndex(int boatIndex, int colorIndex = 0)
        {
            if (SaveSystem.CurrentData != null)
            {
                SaveSystem.CurrentData.SetSelectedBoat(boatIndex, colorIndex);
                SaveSystem.SaveGameData();
                PlayGamesController.GetInstance.SaveDataInCloud();
            }
        }

        public void BoatCustomisationScreenActive(bool val)
        {
            OnBoatCustomisationPanelOpen?.Invoke(val);
        }

        public void UnlockBoatSkin(int index)
        {
            if (SaveSystem.CurrentData != null)
            {
                SaveSystem.CurrentData.UnlockBoat(index);
                SaveSystem.CurrentData.UnlockBoatColor(index, 0);
                SetSavedBoatIndex(index, 0); // Default color index is 0
            }
            OnSkinPurchased?.Invoke(index);
        }

        public void UnlockBoatSkinColor(int boatIndex, int colorIndex)
        {
            if (SaveSystem.CurrentData != null)
            {
                SaveSystem.CurrentData.UnlockBoatColor(boatIndex, colorIndex);
                SetSavedBoatIndex(boatIndex, colorIndex);
            }
            OnSkinColorPurchased?.Invoke(boatIndex, colorIndex);
        }
        #endregion
    }
}
