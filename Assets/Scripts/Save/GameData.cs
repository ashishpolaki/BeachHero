using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public class LevelSaveData
    {
        public int levelIndex;
        public int starsEarned;
        public int highScore;

        public LevelSaveData() { }

        public LevelSaveData(int levelIndex, int starsEarned, int highScore)
        {
            this.levelIndex = levelIndex;
            this.starsEarned = starsEarned;
            this.highScore = highScore;
        }
    }

    [Serializable]
    public class BoatSaveData
    {
        public int boatIndex;
        public bool isUnlocked;
        public int selectedColorIndex;
        public List<int> unlockedColorIndices = new List<int>();

        public BoatSaveData() { }

        public BoatSaveData(int boatIndex, bool isUnlocked, int selectedColorIndex = 0)
        {
            this.boatIndex = boatIndex;
            this.isUnlocked = isUnlocked;
            this.selectedColorIndex = selectedColorIndex;
            this.unlockedColorIndices = new List<int> { 0 }; // Default color (0) is unlocked
        }
    }

    [Serializable]
    public class GameData
    {
        #region Variables
        public int highestCompletedLevel;
        public int totalScore = 0;
        public List<LevelSaveData> levelProgress = new List<LevelSaveData>();

        // Powerups & Currency
        public int coins;
        public int speedBoostBalance;
        public int shieldBalance;
        public bool isSpeedBoostUnlock;
        public bool isShieldUnlock;

        // Tutorials & Engagement
        public bool isWelcomeMessageShown;
        public bool isRateUsShown;
        public bool noAdsPurchased;

        // Customization & Boats
        public bool isBoatNotificationShown;
        public int currentSelectedBoatIndex = 0;
        public List<BoatSaveData> boats = new List<BoatSaveData>();
        #endregion

        #region Factory & Serialization
        public static GameData CreateDefault()
        {
            var defaultBoat = new BoatSaveData(IntUtils.DEFAULT_BOAT_INDEX, true, IntUtils.DEFAULT_BOAT_COLOR_INDEX);

            var data = new GameData
            {
                highestCompletedLevel = IntUtils.DEFAULT_LEVEL,
                totalScore = 0,
                coins = IntUtils.DEFAULT_COINS_BALANCE,
                speedBoostBalance = IntUtils.DEFAULT_SPEEDBOOST_BALANCE,
                shieldBalance = IntUtils.DEFAULT_SHIELD_BALANCE,
                isWelcomeMessageShown = false,
                isRateUsShown = false,
                isSpeedBoostUnlock = false,
                isShieldUnlock = false,
                noAdsPurchased = false,

                levelProgress = new List<LevelSaveData>(),

                isBoatNotificationShown = false,
                currentSelectedBoatIndex = IntUtils.DEFAULT_BOAT_INDEX,
                boats = new List<BoatSaveData> { defaultBoat }
            };

            return data;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static GameData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return CreateDefault();
            }

            try
            {
                GameData data = JsonUtility.FromJson<GameData>(json);
                return data ?? CreateDefault();
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"[GameData] Failed to parse JSON: {ex.Message}");
                return CreateDefault();
            }
        }
        #endregion

        #region Level Helper Methods
        public LevelSaveData GetLevelData(int levelIndex)
        {
            if (levelProgress == null)
            {
                levelProgress = new List<LevelSaveData>();
                return null;
            }

            for (int i = 0; i < levelProgress.Count; i++)
            {
                if (levelProgress[i] != null && levelProgress[i].levelIndex == levelIndex)
                {
                    return levelProgress[i];
                }
            }
            return null;
        }

        public void SetLevelProgress(int levelIndex, int stars, int score)
        {
            if (levelProgress == null)
            {
                levelProgress = new List<LevelSaveData>();
            }

            LevelSaveData existing = GetLevelData(levelIndex);
            if (existing != null)
            {
                existing.starsEarned = Mathf.Max(existing.starsEarned, stars);
                existing.highScore = Mathf.Max(existing.highScore, score);
            }
            else
            {
                levelProgress.Add(new LevelSaveData(levelIndex, stars, score));
            }
        }
        #endregion

        #region Boat Helper Methods
        public void SetBoatNotificationShown(bool shown)
        {
            isBoatNotificationShown = shown;
        }

        public BoatSaveData GetBoatData(int boatIndex)
        {
            if (boats == null)
            {
                boats = new List<BoatSaveData>();
            }

            for (int i = 0; i < boats.Count; i++)
            {
                if (boats[i] != null && boats[i].boatIndex == boatIndex)
                {
                    return boats[i];
                }
            }
            return null;
        }

        public bool IsBoatUnlocked(int boatIndex)
        {
            if (boatIndex == IntUtils.DEFAULT_BOAT_INDEX) return true;
            BoatSaveData boat = GetBoatData(boatIndex);
            return boat != null && boat.isUnlocked;
        }

        public bool IsBoatColorUnlocked(int boatIndex, int colorIndex)
        {
            if (colorIndex == 0) return true; // Default color is always unlocked
            BoatSaveData boat = GetBoatData(boatIndex);
            if (boat == null || !boat.isUnlocked) return false;
            return boat.unlockedColorIndices != null && boat.unlockedColorIndices.Contains(colorIndex);
        }

        public void UnlockBoat(int boatIndex)
        {
            BoatSaveData boat = GetBoatData(boatIndex);
            if (boat == null)
            {
                boat = new BoatSaveData(boatIndex, true, 0);
                boats.Add(boat);
            }
            else
            {
                boat.isUnlocked = true;
                if (boat.unlockedColorIndices == null)
                {
                    boat.unlockedColorIndices = new List<int>();
                }
                if (!boat.unlockedColorIndices.Contains(0))
                {
                    boat.unlockedColorIndices.Add(0);
                }
            }
        }

        public void UnlockBoatColor(int boatIndex, int colorIndex)
        {
            BoatSaveData boat = GetBoatData(boatIndex);
            if (boat == null)
            {
                boat = new BoatSaveData(boatIndex, true, colorIndex);
                boats.Add(boat);
            }

            boat.isUnlocked = true;
            if (boat.unlockedColorIndices == null)
            {
                boat.unlockedColorIndices = new List<int>();
            }
            if (!boat.unlockedColorIndices.Contains(colorIndex))
            {
                boat.unlockedColorIndices.Add(colorIndex);
            }
        }

        public void SetSelectedBoat(int boatIndex, int colorIndex = 0)
        {
            currentSelectedBoatIndex = boatIndex;
            BoatSaveData boat = GetBoatData(boatIndex);
            if (boat != null)
            {
                boat.selectedColorIndex = colorIndex;
            }
            else
            {
                boat = new BoatSaveData(boatIndex, true, colorIndex);
                boats.Add(boat);
            }
        }

        public int GetSelectedBoatColorIndex(int boatIndex)
        {
            BoatSaveData boat = GetBoatData(boatIndex);
            return boat != null ? boat.selectedColorIndex : 0;
        }
        #endregion

        #region Smart Merge (Offline <-> Cloud)
        /// <summary>
        /// Merges offline local progress with cloud progress.
        /// Takes the highest level, highest scores, union of unlocked boats/colors, and keeps local coins/powerups.
        /// </summary>
        public void MergeWithCloudData(GameData cloudData)
        {
            if (cloudData == null) return;

            // Progression:
            highestCompletedLevel = Mathf.Max(highestCompletedLevel, cloudData.highestCompletedLevel);
            isWelcomeMessageShown = isWelcomeMessageShown || cloudData.isWelcomeMessageShown;
            isRateUsShown = isRateUsShown || cloudData.isRateUsShown;
            noAdsPurchased = noAdsPurchased || cloudData.noAdsPurchased;
            coins = Mathf.Max(coins, cloudData.coins);

            // Level Progress: Keep highest stars & score per level, ensuring highestCompletedLevel accounts for all level entries
            if (cloudData.levelProgress != null)
            {
                for (int i = 0; i < cloudData.levelProgress.Count; i++)
                {
                    var cloudLevel = cloudData.levelProgress[i];
                    if (cloudLevel != null)
                    {
                        SetLevelProgress(cloudLevel.levelIndex, cloudLevel.starsEarned, cloudLevel.highScore);
                    }
                }
            }

            // Total Score will be total of all level scores, so recalculate after merging levels
            totalScore = 0;
            if (levelProgress != null)
            {
                foreach (var levelData in levelProgress)
                {
                    if (levelData != null)
                    {
                        totalScore += levelData.highScore;
                    }
                }
            }

            // Boats & Colors: Merge union of all unlocks
            if (cloudData.boats != null)
            {
                foreach (var cloudBoat in cloudData.boats)
                {
                    if (cloudBoat != null)
                    {
                        if (cloudBoat.isUnlocked)
                        {
                            UnlockBoat(cloudBoat.boatIndex);
                        }

                        if (cloudBoat.unlockedColorIndices != null)
                        {
                            foreach (var colorIndex in cloudBoat.unlockedColorIndices)
                            {
                                UnlockBoatColor(cloudBoat.boatIndex, colorIndex);
                            }
                        }
                    }
                }
            }

            // Powerups
            speedBoostBalance = Mathf.Max(speedBoostBalance, cloudData.speedBoostBalance);
            shieldBalance = Mathf.Max(shieldBalance, cloudData.shieldBalance);
            isSpeedBoostUnlock = isSpeedBoostUnlock || cloudData.isSpeedBoostUnlock;
            isShieldUnlock = isShieldUnlock || cloudData.isShieldUnlock;
        }
        #endregion
    }
}
