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
            pathRenderer.SetPositions(PointsList);
        }
    }
}
