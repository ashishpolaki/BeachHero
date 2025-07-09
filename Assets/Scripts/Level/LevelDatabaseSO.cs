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

        public List<LevelData> LevelDatas => levelDatas;

        [SerializeField] private List<LevelData> levelDatas;

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

        public LevelSO GetLevelByIndex(int index)
        {
            return levelsList[index % levelsList.Length];
        }

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

        public void Init()
        {
            int curentLevel = SaveSystem.LoadInt(StringUtils.LEVELNUMBER, IntUtils.DEFAULT_LEVEL);
            int currentLevelIndex = curentLevel - 1;
            int i = 0;
            while (i < currentLevelIndex)
            {
                levelDatas[i++].MarkComplete();
            }
            if (i < levelDatas.Count)
                levelDatas[i].MarkCurrentLevel();
        }

    }

    [System.Serializable]
    public struct SpawnItem
    {
        public SpawnItemType SpawnItemType;
        public GameObject[] Prefab;
    }
}
