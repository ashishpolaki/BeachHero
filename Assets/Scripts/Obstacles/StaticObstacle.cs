using UnityEngine;

namespace BeachHero
{
    public class StaticObstacle : Obstacle
    {
        public void Init(Vector3 position)
        {
            transform.position = position;
        }
    }
}
