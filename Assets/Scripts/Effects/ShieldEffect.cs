using UnityEngine;

namespace BeachHero
{
    public class ShieldEffect : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Vector3 currentScale = new Vector3(2.38f, 2.38f, 2.38f);
        [SerializeField] private float stretch = 0.25f;
        [SerializeField] private float scaleSpeed = 10f;

        public void PlaySpawnAnimation()
        {
            transform.localScale = currentScale;
        }

        public void Stop()
        {
            transform.localScale = Vector3.zero;
        }

        public void UpdateScale(Vector3 direction)
        {
            Vector3 targetScale = new Vector3(
                currentScale.x + Mathf.Abs(direction.x) * stretch,
                currentScale.y,
                currentScale.z);

            // Smooth 
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * scaleSpeed
            );
        }
    }
}
