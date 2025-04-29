using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Scriptable Objects/LevelDatabase")]
    public class LevelDatabaseSO : ScriptableObject
    {
        [SerializeField] private LevelSO[] levelsList;

        public LevelSO[] LevelsList
        {
            get { return levelsList; }
            private set { levelsList = value; }
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
}
