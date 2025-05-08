using UnityEngine;

namespace BeachHero
{
    public class Obstacle : MonoBehaviour, IObstacle
    {
        [SerializeField] private SpawnItemType spawnItemType;
        [SerializeField] private ObstacleType obstacleType;

        public SpawnItemType SpawnItemType
        {
            get
            {
                return spawnItemType;
            }
            set
            {
                spawnItemType = value;
            }
        }
        public ObstacleType ObstacleType
        {
            get
            {
                return obstacleType;
            }
            set
            {
                obstacleType = value;
            }
        }

        public virtual void Hit()
        {

        }

        public virtual void Init(Vector3 position)
        {

        }
    }
    public interface IObstacle
    {
        public ObstacleType ObstacleType { get; set; }
        public abstract void Hit();
    }
    public enum SpawnItemType
    {
        None,
        Collectable,
        MovingObstacle,
        StaticObstacle,
        SavedCharacter,
    }
}
