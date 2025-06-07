using UnityEngine;

namespace BeachHero
{
    public class Eel : MovingObstacle
    {
        [SerializeField] private LineRenderer pathRenderer;

        public override void Init(MovingObstacleData movingObstacleData)
        {
            base.Init(movingObstacleData);
            pathRenderer.positionCount = PointsList.Length;
            // Ensure the first point is at a height of 0.5f to fix rendering bug on water
            PointsList[0].y = 0.5f;
            pathRenderer.SetPositions(PointsList);
        }
    }
}
