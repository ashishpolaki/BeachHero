using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

namespace BeachHero
{
    public class CameraController : SingleTon<CameraController>
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameCameraConfig[] cameraConfigs;
        [SerializeField] private int activePriority = 1;
        [SerializeField] private int inactivePriority = 0;
        [SerializeField] private float playerFarToNearBlendDelay = 0.1f;

        private GameCameraType currentCameraType = GameCameraType.None;
        private GameCameraType previousCameraType = GameCameraType.None;

        #region Initialization
        public void Init()
        {
            SetActiveCamera(GameCameraType.GameView);
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

            foreach (var config in cameraConfigs)
            {
                if(currentCameraType == config.cameraType)
                {
                    config.camera.Priority = activePriority;
                    mainCamera.cullingMask = config.cullingMask;
                }
                else if (config.cameraType == previousCameraType)
                {
                    config.camera.Priority = inactivePriority;
                }
            }
            currentCameraType = type;
        }
        public void SetCameraPosition(Vector3 pos, bool setZ = true)
        {
            foreach (var config in cameraConfigs)
            {
                if (config.cameraType == currentCameraType)
                {
                    Transform t = config.camera.transform;
                    Vector3 current = t.position;
                    t.position = new Vector3(pos.x, pos.y, setZ ? pos.z : current.z);
                    break;
                }
            }
        }
        public void SetOrthoSize(float size,GameCameraType gameCameraType)
        {
            foreach (var config in cameraConfigs)
            {
                if (config.cameraType == gameCameraType)
                {
                    config.camera.Lens.OrthographicSize = size;
                    break;
                }
            }
        }
        public void SetCameraFollow(Transform target, GameCameraType cameraType)
        {
            foreach (var config in cameraConfigs)
            {
                if (config.cameraType == cameraType)
                {
                    config.camera.Follow = target;
                    break;
                }
            }
        }
        public void ShakeActiveCamera()
        {

        }
        #endregion

    }
    public enum GameCameraType
    {
        PlayerFar,
        PlayerNear,
        MapFar,
        MapNear,
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
