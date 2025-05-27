using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

namespace BeachHero
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera gameViewCamera;
        [SerializeField] private CinemachineCamera playerNearTargetCamera;
        [SerializeField] private CinemachineCamera playerFarTargetCamera;

        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeMagnitude = 0.2f;
        [SerializeField] private float shakeFrequency = 25f;
        [SerializeField] private float cameraBlendDuration = 0.1f;

        private Vector3 originalPos;
        private Transform mainCameraTransform;

        private void Awake()
        {
            // Cache the main camera transform
            mainCameraTransform = Camera.main.transform;
            originalPos = mainCameraTransform.position;
            GameController.GetInstance.CacheCameraController(this);
            Init();
        }

        public void OnLevelPass(Transform playerTarget)
        {
            //Vector3 shoulderOffset = playerTarget.TransformPoint(playerTargetCameraSettings.Position);
            //mainCameraTransform.position = shoulderOffset;
            //mainCameraTransform.LookAt(playerTarget.position); 
            //mainCameraTransform.rotation *= Quaternion.Euler(playerTargetCameraSettings.Rotation);
            StartCoroutine(CameraBlend(playerTarget));
        }
        IEnumerator CameraBlend(Transform playerTarget)
        {
            playerFarTargetCamera.Priority = 1; // Set far camera to active
            playerFarTargetCamera.Follow = playerTarget;
            gameViewCamera.Priority = 0;
            yield return new WaitForSeconds(cameraBlendDuration);
            playerNearTargetCamera.Priority = 2;
            playerNearTargetCamera.Follow = playerTarget;
        }

        public void Init()
        {
            gameViewCamera.Priority = 1;
            playerNearTargetCamera.Priority = 0;
            playerFarTargetCamera.Priority = 0;
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
