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

    //[Serializable]
    //public class BoatSaveData
    //{
    //    public int boatIndex;
    //    public bool isUnlocked;
    //    public int selectedColorIndex;
    //    public List<int> unlockedColorIndices = new List<int>();

    //    public BoatSaveData() { }

    //    public BoatSaveData(int boatIndex, bool isUnlocked, int selectedColorIndex = 0)
    //    {
    //        this.boatIndex = boatIndex;
    //        this.isUnlocked = isUnlocked;
    //        this.selectedColorIndex = selectedColorIndex;
    //        this.unlockedColorIndices = new List<int> { 0 }; // Default color (0) is unlocked
    //    }
    //}

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

        // Customization & Boats

        //Tutorials & Engagement
        public bool isWelcomeMessageShown;
        public bool isRateUsShown;
        #endregion

        #region Factory & Serialization
        public static GameData CreateDefault()
        {
            var data = new GameData
            {
                highestCompletedLevel = IntUtils.DEFAULT_LEVEL,
                totalScore = 0,
                coins = IntUtils.DEFAULT_COINS_BALANCE,
                speedBoostBalance = IntUtils.DEFAULT_SPEEDBOOST_BALANCE,
                shieldBalance = IntUtils.DEFAULT_SHIELD_BALANCE,
                isWelcomeMessageShown = false,
                isRateUsShown = false,

                levelProgress = new List<LevelSaveData>(),
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
            coins = Mathf.Max(coins, cloudData.coins);

            // Level Progress: Keep highest stars & score per level, ensuring highestCompletedLevel accounts for all level entries
            if (cloudData.levelProgress != null)
            {
                for (int i = 0; i < cloudData.levelProgress.Count; i++)
                {
                    var cloudLevel = cloudData.levelProgress[i];
                    if (cloudLevel != null)
                    {
                        MergeLevelProgress(cloudLevel.levelIndex, cloudLevel.starsEarned, cloudLevel.highScore);
                    }
                }
            }

            // Total Score will be total of all level scores, so recalculate after merging levels
            totalScore = 0;
            foreach (var levelData in levelProgress)
            {
                if (levelData != null)
                {
                    totalScore += levelData.highScore;
                }
            }
        }
        public void MergeLevelProgress(int level, int stars, int score)
        {
            LevelSaveData existing = GetLevelData(level);
            if (existing != null)
            {
                existing.starsEarned = Mathf.Max(existing.starsEarned, stars);
                existing.highScore = Mathf.Max(existing.highScore, score);
            }
            else
            {
                levelProgress.Add(new LevelSaveData(level, stars, score));
            }
        }
        public LevelSaveData GetLevelData(int level)
        {
            for (int i = 0; i < levelProgress.Count; i++)
            {
                if (levelProgress[i].levelIndex == level)
                {
                    return levelProgress[i];
                }
            }
            return null;
        }
        #endregion
    }
}
