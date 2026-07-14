using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Scriptable Objects/LevelDatabase")]
    public class LevelDatabaseSO : ScriptableObject
    {
        [SerializeField] private LevelSO[] levelsList;
        [SerializeField] private SpawnItem[] spawnItemsList;
        [SerializeField] private List<LevelData> levelDatas;

        #region Properties
        public List<LevelData> LevelDatas => levelDatas;

        public LevelSO[] LevelsList
        {
            get { return levelsList; }
            private set { levelsList = value; }
        }

        public SpawnItem[] SpawnItemsList
        {
            get { return spawnItemsList; }
            private set { spawnItemsList = value; }
        }

        public int TotalLevelsCount
        {
            get { return levelsList.Length; }
        }
        #endregion

#if UNITY_EDITOR
        [ContextMenu("Rename Levels")]
        public void ResetLevelsDataContext()
        {
            if (levelsList == null || levelsList.Length == 0)
                return;

            // Store temporary names for each asset.
            Dictionary<Object, string> tempNames = new();

            // ---------- PASS 1 : Rename to unique temporary names ----------
            foreach (var level in levelsList)
            {
                if (level == null)
                    continue;

                string path = AssetDatabase.GetAssetPath(level);

                if (string.IsNullOrEmpty(path))
                    continue;

                string tempName = "__TMP_" + System.Guid.NewGuid().ToString("N");

                string error = AssetDatabase.RenameAsset(path, tempName);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Failed to rename '{path}' to temp name.\n{error}");
                    continue;
                }

                tempNames[level] = tempName;
            }

            AssetDatabase.SaveAssets();

            // ---------- PASS 2 : Rename to final names ----------
            for (int i = 0; i < levelsList.Length; i++)
            {
                var level = levelsList[i];

                if (level == null)
                    continue;

                string path = AssetDatabase.GetAssetPath(level);

                if (string.IsNullOrEmpty(path))
                    continue;

                string finalName = $"Level_{i + 1}";

                level.name = finalName;
                EditorUtility.SetDirty(level);

                string error = AssetDatabase.RenameAsset(path, finalName);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Failed to rename '{path}' to '{finalName}'.\n{error}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        public void ClearLevelsData()
        {
            for (int i = 0; i < levelDatas.Count; i++)
            {
                levelDatas[i].LevelNumber = i + 1;
                levelDatas[i].SetState(LevelVisualState.Locked);
                levelDatas[i].StarsEarned = 0;
                SaveSystem.SaveInt($"{StringUtils.STARS_EARNED_PREFIX}{i}", 0);
            }
        }
        private void OnValidate()
        {
            for (int i = 0; i < levelDatas.Count; i++)
            {
                if (levelDatas[i].LevelNumber != i + 1)
                {
                    levelDatas[i].LevelNumber = i + 1;
                }
            }
        }
#endif

        #region Methods
        public void Init()
        {
            int currentLevelIndex = GameController.GetInstance.HighestCompletedLevelIndex;
            for (int i = 0; i < levelDatas.Count; i++)
            {
                if (i < currentLevelIndex)
                {
                    levelDatas[i].SetState(LevelVisualState.Completed);
                }
                else if (i == currentLevelIndex)
                {
                    //If that is a last level, check more
                    if (levelDatas[i].StarsEarned <= 0)
                    {
                        levelDatas[i].SetState(LevelVisualState.Current);
                    }
                    else if (levelDatas[i].StarsEarned > 0)
                    {
                        levelDatas[i].SetState(LevelVisualState.Completed);
                    }
                }
                else
                {
                    levelDatas[i].SetState(LevelVisualState.Locked);
                }
                levelDatas[i].StarsEarned = SaveSystem.LoadInt($"{StringUtils.STARS_EARNED_PREFIX}{i}", 0);
            }
        }

        public LevelSO GetLevelByIndex(int index)
        {
            return levelsList[index % levelsList.Length];
        }
        public LevelData GetLevelDataByIndex(int index)
        {
            return levelDatas[index % levelDatas.Count];
        }
        public void SetStarsForLevel(int levelIndex, int medals)
        {
            if (levelIndex >= 0 && levelIndex < levelDatas.Count)
            {
                if (medals > levelDatas[levelIndex].StarsEarned)
                {
                    levelDatas[levelIndex].StarsEarned = medals;
                    SaveSystem.SaveInt($"{StringUtils.STARS_EARNED_PREFIX}{levelIndex}", medals);
                }
            }

            //Set TotalMedals as well
            int totalmedals = 0;
            for (int i = 0; i < levelDatas.Count; i++)
            {
                medals += levelDatas[i].StarsEarned;
            }
            SaveSystem.SaveInt(StringUtils.TOTAL_STARS, totalmedals);
        }
        #endregion
    }

    [System.Serializable]
    public struct SpawnItem
    {
        public SpawnItemType SpawnItemType;
        public GameObject[] Prefab;
    }
    public enum SpawnItemType
    {
        None,
        Collectable,
        MovingObstacle,
        StaticObstacle,
        WhirlpoolObstacle,
        DrownCharacter,
    }
}
