using UnityEngine;

namespace BeachHero
{
    [CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
    public class LevelSO : ScriptableObject
    {
        [SerializeField] private float levelTime;
        [SerializeField] private StartPointData startPoint;
        [SerializeField] private ObstacleData obstacles;
        [SerializeField] private DrownCharacterData[] drownCharacters;
        [SerializeField] private CollectableData[] collectables;

        #region Properties
        
        public float LevelTime => levelTime;

        public StartPointData StartPointData => startPoint;

        public ObstacleData Obstacle => obstacles;

        public DrownCharacterData[] DrownCharacters => drownCharacters;

        public CollectableData[] Collectables => collectables;
        #endregion
    }

    [System.Serializable]
    public struct CollectableData
    {
        public CollectableType type;
        public Vector3 position;
        [Range(1, 10)] public int count;
    }

    public enum CollectableType
    {
        None,
        GameCurrency,
        Gem,
        Magnet,
        SpeedBoost,
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
        Rock,
        Barrel,
        MantaRay,
    }

    public enum MovingObstacleMovementType
    {
        None,
        Custom,
        FigureEight,
        Circular,
    }

    [System.Serializable]
    public struct DrownCharacterData
    {
        [SerializeField] private Vector3 position;
        [Range(0, 1f)]
        [SerializeField] private float waitTimePercentage;

        public Vector3 Position => position;

        public float WaitTimePercentage => waitTimePercentage;
    }

    [System.Serializable]
    public struct ObstacleData
    {
        [SerializeField] private StaticObstacleData[] staticObstacles;
        [SerializeField] private MovingObstacleData[] movingObstacles;
        [SerializeField] private WaterHoleObstacleData[] waterHoleObstacles;

        public StaticObstacleData[] StaticObstacles => staticObstacles;

        public MovingObstacleData[] MovingObstacles => movingObstacles;

        public WaterHoleObstacleData[] WaterHoleObstacles => waterHoleObstacles;
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

        public BezierKeyframe[] bezierKeyframes;
        public float resolution;
        public float movementSpeed;

        [Space]
        public bool loopedMovement;
        public bool inverseDirection;
    }

    [System.Serializable]
    public struct WaterHoleObstacleData
    {
        public Vector3 position;
        public Vector2 shaderPosition; // Position used for the shader effect
        public float scale;
    }

    [System.Serializable]
    public struct BezierKeyframe
    {
        public Vector3 position; // Keyframe position (world position)
        public Vector3 inTangentLocal; // Incoming tangent (local position relative to position)
        public Vector3 outTangentLocal; // Outgoing tangent (local position relative to position)

        /// <summary>
        /// Property to calculate the world position of the inTangent
        /// </summary>
        public Vector3 InTangentWorld => position + inTangentLocal;

        /// <summary>
        ///  Property to calculate the world position of the outTangent
        /// </summary>
        public Vector3 OutTangentWorld => position + outTangentLocal;
    }
}
