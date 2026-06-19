using UnityEngine;

namespace BeachHero
{
    public enum ObstacleType
    {
        None,
        Shark,
        Eel,
        Whirlpool,
        Rock,
        Barrel,
        MantaRay,
        Iceberg,
        ShipWreck,
    }
    public class Obstacle : MonoBehaviour, IObstacle
    {
        [SerializeField] private ObstacleType obstacleType;
        private bool isHit = false;

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
        public bool IsHit
        {
            get { return isHit; }
            set
            {
                isHit = value;
            }
        }

        public virtual void Hit()
        {
            isHit = true;
            GameController.GetInstance.OnLevelFailed(LevelFailDelayType.Medium);
            HapticsManager.GetInstance.WarningHaptic();
        }
        public virtual void UpdateState()
        {
        }
        public virtual void ResetObstacle()
        {
            isHit = false;
        }
        public virtual void HitByDash(Vector3 hitDirection = default)
        {
            isHit = true;
        }
    }
    public interface IObstacle
    {
        public ObstacleType ObstacleType { get; set; }
        public bool IsHit { get; set; }
        public abstract void Hit();
        public abstract void UpdateState();
        public abstract void ResetObstacle();
        public abstract void HitByDash(Vector3 hitDirection);
    }
}
