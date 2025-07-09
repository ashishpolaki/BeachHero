using UnityEngine;

namespace BeachHero
{
    public class RockObstacle : StaticObstacle
    {
        public override void Hit()
        {
            base.Hit();
            GameController.GetInstance.CameraController.StartShake();
        }
    }
}
