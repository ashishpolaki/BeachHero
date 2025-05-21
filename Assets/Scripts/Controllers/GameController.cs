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

        private void SpawnLevel()
        {
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
        public void OnLevelCompleted()
        {
            isGameStarted = false;
            currentLevelIndex++;
            SaveController.SaveInt(StringUtils.LEVELNUMBER , currentLevelIndex);
        }
        public void RetryLevel()
        {
            SpawnLevel();
        }
        public void OnPowerUpPickedUp(PowerupType powerupType)
        {
            powerupController.OnPowerupCollected(powerupType);
        }
        public void OnPowerupActivated(PowerupType powerupType)
        {
            levelController.OnActivatePowerup(powerupType);
            powerupController.OnPowerupActivated(powerupType);
        }
        public void OnCharacterDrowned()
        {
            isGameStarted = false;
            levelController.OnCharacterDrown();
            UIController.GetInstance.ScreenEvent(ScreenType.GameLose, UIScreenEvent.Open);
        }
        public void OnCharacterPickUp()
        {
            levelController.OnCharacterPickUp();
        }
    }
}
