using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Scriptable Objects/LevelDatabase")]
    public class LevelDatabaseSO : ScriptableObject
    {
        [SerializeField] private LevelSO[] levelsList;
        [SerializeField] private SpawnItems spawnItems;

        public LevelSO[] LevelsList
        {
            get { return levelsList; }
            private set { levelsList = value; }
        }
        public SpawnItems SpawnItems
        {
            get { return spawnItems; }
            private set { spawnItems = value; }
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
    public struct SpawnItems
    {
        [SerializeField] private GameObject[] staticObstacles;
        [SerializeField] private GameObject[] movingObstacles;
        [SerializeField] private GameObject[] collectables;
        [SerializeField] private GameObject[] savedCharacters;

        public GameObject[] StaticObstacles => staticObstacles;
        public GameObject[] MovingObstacles => movingObstacles;
        public GameObject[] Collectables => collectables;
        public GameObject[] SavedCharacters => savedCharacters;
    }
}
