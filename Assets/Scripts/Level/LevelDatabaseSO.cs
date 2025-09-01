using System;
using System.Collections.Generic;
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
        public void ClearLevelsData()
        {
            for (int i = 0; i < levelDatas.Count; i++)
            {
                levelDatas[i].IsCurrentLevel = false;
                levelDatas[i].IsCompleted = false;
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
            int currentLevelIndex = GameController.GetInstance.CurrentLevelIndex;
            for (int i = 0; i < levelDatas.Count; i++)
            {
                if (i < currentLevelIndex)
                {
                    levelDatas[i].MarkComplete();
                }
                else if (i == currentLevelIndex)
                {
                    levelDatas[i].MarkCurrentLevel();
                }
                else
                {
                    levelDatas[i].MarkIncomplete();
                }
            }
        }

        public LevelSO GetLevelByIndex(int index)
        {
            return levelsList[index % levelsList.Length];
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
        WaterHoleObstacle,
        DrownCharacter,
    }
}
