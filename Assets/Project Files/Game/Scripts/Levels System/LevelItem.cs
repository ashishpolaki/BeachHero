using UnityEngine;

namespace Watermelon
{


    [System.Serializable]
    public class LevelItem
    {
        [SerializeField] Item item;
        public Item Item => item;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] Category category;
        [SerializeField] Vector3 spawnPosition;

        public LevelItem()
        {

        }

        public LevelItem(Item item, GameObject prefab)
        {
            this.item = item;
            this.prefab = prefab;
        }

        public LevelItem(Item item)
        {
            this.item = item;
        }

        //Used in level editor
        public enum Category
        {
            StartPoint = 0,
            Character = 1,
            MovingObstacle = 2,
            Item = 3
        }
    }
}