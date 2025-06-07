using UnityEngine;

namespace BeachHero
{
    public class SharkObstacle : MovingObstacle
    {
        [SerializeField] private LineRenderer pathRenderer;

        public override void Init(MovingObstacleData movingObstacleData)
        {
            base.Init(movingObstacleData);
            pathRenderer.positionCount = PointsList.Length;
            PointsList[0].y = 0.5f;
            pathRenderer.SetPositions(PointsList);
        }
    }
}
