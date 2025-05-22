using UnityEngine;

namespace BeachHero
{
    public class BarrelObstacle : StaticObstacle
    {
        [SerializeField] private GameObject barrelGraphics;

        public override void Init(Vector3 position)
        {
            base.Init(position);
        }
    }
}
