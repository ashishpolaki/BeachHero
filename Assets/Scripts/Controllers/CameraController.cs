using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BeachHero
{
    public class CameraController : SingleTon<CameraController>
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameCameraConfig[] cameraConfigs;
        [SerializeField] private int activePriority = 1;
        [SerializeField] private int inactivePriority = 0;
        [SerializeField] private float playerFarToNearBlendDelay = 0.1f;

        [Space(10), Header("Shake Settings")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeAmplitude = 1f;

        private GameCameraType currentCameraType = GameCameraType.None;
        private GameCameraType previousCameraType = GameCameraType.None;
        private Dictionary<GameCameraType, GameCameraConfig> cameraDictionary = new Dictionary<GameCameraType, GameCameraConfig>();

        #region Initialization
        public void Init()
        {
            SetActiveCamera(GameCameraType.GameView);
            foreach (var config in cameraConfigs)
            {
                if (!cameraDictionary.ContainsKey(config.cameraType))
                {
                    cameraDictionary.Add(config.cameraType, config);
                }
            }
        }
        #endregion

        #region Player Camera Blend Switch
        public void OnPlayerBlendCamera(Transform playerTarget)
        {
            SetCameraFollow(playerTarget, GameCameraType.PlayerFar);
            SetCameraFollow(playerTarget, GameCameraType.PlayerNear);
            SetActiveCamera(GameCameraType.PlayerFar);
            StartCoroutine(PlayerCameraBlend());
        }
        IEnumerator PlayerCameraBlend()
        {
            yield return new WaitForSeconds(playerFarToNearBlendDelay);
            SetActiveCamera(GameCameraType.PlayerNear);
        }
        #endregion

        #region Camera Modifications
        public void SetActiveCamera(GameCameraType type)
        {
            if (currentCameraType == type)
            {
                return;
            }

            previousCameraType = currentCameraType;
            currentCameraType = type;

            if (cameraDictionary.TryGetValue(type, out GameCameraConfig currentConfig))
            {
                currentConfig.camera.Priority = activePriority;
                mainCamera.cullingMask = currentConfig.cullingMask;
            }
            if (cameraDictionary.TryGetValue(previousCameraType, out GameCameraConfig previousConfig))
            {
                previousConfig.camera.Priority = inactivePriority;
            }
        }
        public void SetCameraPosition(Vector3 pos, bool setZ = true)
        {
            if (cameraDictionary.TryGetValue(currentCameraType, out GameCameraConfig currentConfig))
            {
                Transform t = currentConfig.camera.transform;
                Vector3 current = t.position;
                t.position = new Vector3(pos.x, pos.y, setZ ? pos.z : current.z);
            }
        }
        public void SetOrthoSize(float size, GameCameraType gameCameraType)
        {
            if (cameraDictionary.TryGetValue(gameCameraType, out GameCameraConfig currentConfig))
            {
              currentConfig.camera.Lens.OrthographicSize = size;
            }
        }
        public void SetCameraFollow(Transform target, GameCameraType cameraType)
        {
            if (cameraDictionary.TryGetValue(cameraType, out GameCameraConfig currentConfig))
            {
                currentConfig.camera.Follow = target;
            }
        }
        public void SetCollider(Collider2D collider, GameCameraType gameCameraType)
        {
            if (cameraDictionary.TryGetValue(gameCameraType, out GameCameraConfig currentConfig))
            {
                var confiner = currentConfig.camera.GetComponent<CinemachineConfiner2D>();
                if (confiner != null)
                {
                    currentConfig.camera.GetComponent<CinemachineConfiner2D>().BoundingShape2D = collider;
                }
            }
        }

        public async void ShakeActiveCamera()
        {
            if (cameraDictionary.TryGetValue(currentCameraType, out GameCameraConfig currentConfig))
            {
                var shakeCamera = currentConfig.camera.GetComponent<CinemachineBasicMultiChannelPerlin>();
                if (shakeCamera != null)
                {
                    shakeCamera.AmplitudeGain = shakeAmplitude;
                    await Task.Delay((int)(shakeDuration * 1000));
                    shakeCamera.AmplitudeGain = 0f;
                }
            }
        }
        #endregion
    }
    public enum GameCameraType
    {
        PlayerFar,
        PlayerNear,
        Map,
        GameView,
        None
    }

    [System.Serializable]
    public struct GameCameraConfig
    {
        public GameCameraType cameraType;
        public CinemachineCamera camera;
        public LayerMask cullingMask;
    }
}
