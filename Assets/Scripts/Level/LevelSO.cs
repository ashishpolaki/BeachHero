using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
    public class LevelSO : ScriptableObject
    {
        [SerializeField] private StartPointData startPoint;
        [SerializeField] private ObstacleData obstacles;
        [SerializeField] private SavedCharacterData[] savedCharacters;

        #region Properties
        public StartPointData StartPointData => startPoint;
        public ObstacleData Obstacle => obstacles;
        public SavedCharacterData[] SavedCharacters => savedCharacters;
        #endregion
    }

    [System.Serializable]
    public struct StartPointData
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }
    public enum ObstacleType
    {
        None,
        Shark,
        Eel,
        WaterHole,
        Rock
    }
    public enum MovingObstacleMovementType
    {
        None,
        Linear,
        Circular
    }
    [System.Serializable]
    public struct SavedCharacterData
    {
        [SerializeField] private Vector3 position;
        [SerializeField] private float waitTimePercentage;
    }

    [System.Serializable]
    public struct ObstacleData
    {
        [SerializeField] private StaticObstacleData[] staticObstacles;
        [SerializeField] private MovingObstacleData[] movingObstacles;

        public StaticObstacleData[] StaticObstacles => staticObstacles;
        public MovingObstacleData[] MovingObstacles => movingObstacles;
    }

    [System.Serializable]
    public struct StaticObstacleData
    {
        public ObstacleType type;
        public Vector3 position;
        public Vector3 rotation;
    }

    [System.Serializable]
    public struct MovingObstacleData
    {
        public ObstacleType type;
        public MovingObstacleMovementType movementType;

        [Space]
        public Vector3 linearMovementStartPosition;
        public Vector3 linearMovementFinishPosition;

        [Space]
        public Vector3 circlarMovementCenter;
        public float circlarMovementRadius;

        [Space]
        public bool loopedMovement;
        public bool inverseDirection;
    }
}
