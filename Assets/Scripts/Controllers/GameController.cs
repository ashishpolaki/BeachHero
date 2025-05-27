using System.Collections.Generic;
using UnityEngine;

namespace BeachHero
{
    public class GameController : SingleTon<GameController>
    {
        [SerializeField] private LevelController levelController;
        [SerializeField] private LevelDatabaseSO levelDatabaseSO;
        [SerializeField] private PoolController poolManager;
        [SerializeField] private SaveController saveController;
        [SerializeField] private PowerupController powerupController;

        [Tooltip("The Index Starts from 0")]
        private int currentLevelIndex;
        private bool isGameStarted = false;
        private CameraController cameraController;

        #region Properties
        public int CurrentLevelIndex => currentLevelIndex;
        public PoolController PoolManager => poolManager;
        public LevelController LevelController => levelController;
        public PowerupController PowerupController => powerupController;
        #endregion

        #region Unity Methods
        private void Update()
        {
            if (isGameStarted)
            {
                levelController.UpdateState();
            }
        }
        private void OnDestroy()
        {
            poolManager.Reset();
        }
        private void Start()
        {
            powerupController.LoadPowerups();
            SpawnLevel();
        }
        #endregion

        #region Cache Component
        public void CacheCameraController(CameraController cameraController)
        {
            this.cameraController = cameraController;
            if (cameraController != null)
            {
                CacheComponent(out this.cameraController);
            }
            else
            {
                Debug.LogError("CameraController is null");
            }
        }
        private bool CacheComponent<T>(out T component) where T : Component
        {
            Component unboxedComponent = gameObject.GetComponent(typeof(T));

            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;

                return true;
            }

            Debug.LogError(string.Format("GameController does not able to cache {0}", typeof(T)));

            component = null;

            return false;
        }
        #endregion

        private void SpawnLevel()
        {
            isGameStarted = false;
            currentLevelIndex = SaveController.LoadInt(StringUtils.LEVELNUMBER, 0);
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(currentLevelIndex));
        }
        public void Play(List<PowerupType> powerupTypes)
        {
            if (powerupTypes.Count > 0)
            {
                foreach (PowerupType powerupType in powerupTypes)
                {
                    powerupController.OnPowerupActivated(powerupType);
                    levelController.OnActivatePowerup(powerupType);
                }
            }
            isGameStarted = true;
            levelController.GameStart();
            UIController.GetInstance.ScreenEvent(ScreenType.Gameplay, UIScreenEvent.Open);
        }
        public void OnCharacterPickUp()
        {
            levelController.OnCharacterPickUp();
        }
        public void RetryLevel()
        {
            SpawnLevel();
        }

        #region Level Pass/Fail
        public void OnLevelPass()
        {
            currentLevelIndex++;
            SaveController.SaveInt(StringUtils.LEVELNUMBER, currentLevelIndex);
            levelController.OnLevelCompleted(true);
            UIController.GetInstance.ScreenEvent(ScreenType.GameWin, UIScreenEvent.Open);
        }
        public void OnLevelFailed()
        {
            levelController.OnLevelCompleted(false);
            UIController.GetInstance.ScreenEvent(ScreenType.GameLose, UIScreenEvent.Open);
        }

        #endregion

        #region Powerup
        public void OnPowerUpPickedUp(PowerupType powerupType)
        {
            powerupController.OnPowerupCollected(powerupType);
        }
        public void OnPowerupActivated(PowerupType powerupType)
        {
            levelController.OnActivatePowerup(powerupType);
            powerupController.OnPowerupActivated(powerupType);
        }
        #endregion

        #region Camera
        public void OnCameraShakeEffect()
        {
            cameraController.StartShake();
        }
        #endregion

    }
}
