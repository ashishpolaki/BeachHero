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
        [SerializeField] private MedalCurrencyRequirements medalRequirements;

        #region Properties

        public float LevelTime => levelTime;

        public StartPointData StartPointData => startPoint;

        public ObstacleData Obstacle => obstacles;

        public DrownCharacterData[] DrownCharacters => drownCharacters;

        public CollectableData[] Collectables => collectables;

        public MedalCurrencyRequirements MedalsRequirements => medalRequirements;
        #endregion
    }

    [System.Serializable]
    public struct MedalCurrencyRequirements
    {
        public int requiredCurrencyForThreeMedals;
        public int requiredCurrencyForTwoMedals;
        public int requiredCurrencyForOneMedal;
    }

    [System.Serializable]
    public struct CollectableData
    {
        public CollectableType type;
        public Vector3 position;
        public Vector3 rotation;
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

    public enum MovingObstacleShape
    {
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
        [SerializeField] private WhirlpoolObstacleData[] whirlpoolObstacles;

        public StaticObstacleData[] StaticObstacles => staticObstacles;

        public MovingObstacleData[] MovingObstacles => movingObstacles;

        public WhirlpoolObstacleData[] WhirlpoolObstacles => whirlpoolObstacles;
    }

    [System.Serializable]
    public struct StaticObstacleData
    {
        public ObstacleType type;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    [System.Serializable]
    public struct MovingObstacleData
    {
        public ObstacleType type;
        public BezierKeyframe[] bezierKeyframes;
        public float resolution;
        public float movementSpeed;
        public float rotationSpeedMultiplier;

        [Space]
        public bool loopedMovement;
        public bool inverseDirection;
    }

    [System.Serializable]
    public struct WhirlpoolObstacleData
    {
        public Vector3 position;
        public Vector2 shaderPosition; // Position used for the shader effect
        public float scale;
    }

}
