using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public class CameraShake : MonoBehaviour
    {
        public float duration = 1f;
        public float magnitude = 0.2f;
        public float frequency = 25f;

        private Vector3 originalPos;

        void Start()
        {
            originalPos = transform.localPosition;
        }

        public void StartShake()
        {
            StopAllCoroutines(); // Stop any ongoing shake to prevent overlap
            StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / duration;

                float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

                float x = (Mathf.PerlinNoise(Time.time * frequency, 0.0f) - 0.5f) * 2.0f * magnitude * damper;
                float y = (Mathf.PerlinNoise(0.0f, Time.time * frequency) - 0.5f) * 2.0f * magnitude * damper;

                transform.localPosition = originalPos + new Vector3(x, y, 0);

                yield return null; // Wait for the next frame
            }

            // Reset position after shaking
            transform.localPosition = originalPos;
        }
    }
}
