using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Scriptable Objects/LevelDatabase")]
    public class LevelDatabaseSO : ScriptableObject
    {
        [SerializeField] private LevelSO[] levelsList;
        [SerializeField] private SpawnItem[] spawnItemsList;

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
    }
   
    [System.Serializable]
    public struct  SpawnItem 
    {
        public SpawnItemType SpawnItemType;
        public GameObject[] Prefab;
    }
}
