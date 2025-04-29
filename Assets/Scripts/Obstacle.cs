using UnityEngine;

namespace BeachHero
{
    public class Obstacle : MonoBehaviour, IObstacle
    {
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

        [SerializeField] private ObstacleType obstacleType;

        public virtual void Hit()
        {

        }
    }
    public interface IObstacle
    {
        public ObstacleType ObstacleType { get; set; }
        public abstract void Hit();
    }
}
