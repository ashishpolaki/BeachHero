using UnityEngine;
using System.Collections;

namespace BeachHero
{
    public class CameraController : MonoBehaviour
    {
        private Transform mainCameraTransform;

        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeMagnitude = 0.2f;
        [SerializeField] private float shakeFrequency = 25f;

        private Vector3 originalPos;

        private void Awake()
        {
            // Cache the main camera transform
            mainCameraTransform = Camera.main.transform;
            originalPos = mainCameraTransform.position;
            GameController.GetInstance.CacheCameraController(this);
        }

        public void StartShake()
        {
            StopAllCoroutines(); // Stop any ongoing shake to prevent overlap
            StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / shakeDuration;

                float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

                float x = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0.0f) - 0.5f) * 2.0f * shakeMagnitude * damper;
                float y = (Mathf.PerlinNoise(0.0f, Time.time * shakeFrequency) - 0.5f) * 2.0f * shakeMagnitude * damper;

                mainCameraTransform.localPosition = originalPos + new Vector3(x, y, 0);

                yield return null; // Wait for the next frame
            }

            // Reset position after shaking
            mainCameraTransform.localPosition = originalPos;
        }
    }
}
