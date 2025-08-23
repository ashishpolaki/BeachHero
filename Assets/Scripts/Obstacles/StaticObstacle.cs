using UnityEngine;

namespace BeachHero
{
    public class StaticObstacle : Obstacle
    {
        [SerializeField] private bool isShakeEffectEnabled = false;

        public virtual void Init(Vector3 position)
        {
            transform.position = position;
        }
        public override void Hit()
        {
            base.Hit();
            if (isShakeEffectEnabled)
            {
                CameraController.GetInstance.ShakeActiveCamera();
            }
        }
    }
}
