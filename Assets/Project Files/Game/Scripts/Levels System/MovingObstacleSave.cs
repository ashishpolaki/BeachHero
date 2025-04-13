using UnityEngine;

namespace Watermelon.BeachRescue
{
    [System.Serializable]
    public class MovingObstacleSave
    {
        public MovingObstacleType type;
        public float movementSpeed = 15f;

        [Space]
        public Vector3 linearMovementStartPosition;
        public Vector3 linearMovementFinishPosition;

        [Space]
        public Vector3 circlarMovementCenter;
        public float circlarMovementRadius = 1f;

        [Space]
        public bool loopedMovement = true;
        public bool inverseDirection;
    }
}