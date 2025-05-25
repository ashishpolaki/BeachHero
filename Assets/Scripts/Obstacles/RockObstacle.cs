using UnityEngine;

namespace BeachHero
{
    public class RockObstacle : StaticObstacle
    {
        public override void Hit()
        {
            base.Hit();
            Camera.main.GetComponent<CameraShake>().StartShake();
        }
    }
}
