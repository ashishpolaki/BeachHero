using UnityEngine;

namespace BeachHero
{
    public class Obstacle : MonoBehaviour, IObstacle
    {
        [SerializeField] private ObstacleType obstacleType;

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
            GameController.GetInstance.OnLevelFailed();
        }

        public virtual void UpdateState()
        {
        }
    }
    public interface IObstacle
    {
        public ObstacleType ObstacleType { get; set; }
        public abstract void Hit();
        public abstract void UpdateState();
    }
    public enum SpawnItemType
    {
        None,
        Collectable,
        MovingObstacle,
        StaticObstacle,
        WaterHoleObstacle,
        SavedCharacter,
    }
}
