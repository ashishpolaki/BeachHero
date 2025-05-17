using UnityEngine;

namespace BeachHero
{
    public class GameController : SingleTon<GameController>
    {
        [SerializeField] private LevelController levelController;
        [SerializeField] private LevelDatabaseSO levelDatabaseSO;
        [SerializeField] private PoolController poolManager;

        [Tooltip("The Index Starts from 0")]
        private int currentLevelIndex;
        private bool isGameStarted = false;

        #region Properties
        public int CurrentLevelIndex => currentLevelIndex;
        public PoolController PoolManager => poolManager;
        public LevelController LevelController => levelController;
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
            SpawnLevel();
        }
        #endregion

        private void SpawnLevel()
        {
            currentLevelIndex = 0;
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(currentLevelIndex));
        }
        public void Play()
        {
            isGameStarted = true;
            levelController.GameStart();
            UIController.GetInstance.ScreenEvent(ScreenType.Gameplay, UIScreenEvent.Open);
        }
        public void RetryLevel()
        {
            SpawnLevel();
        }
        public void OnMagnetPowerUpActivate()
        {
            levelController.ActivateCoinMagnetPowerup();
        }

        public void OnSpeedPowerUpActivate()
        {
            levelController.ActivateSpeedPowerup();
        }
        public void OnCharacterDrowned()
        {
            isGameStarted = false;
            levelController.OnCharacterDrown();
            UIController.GetInstance.ScreenEvent(ScreenType.GameLose, UIScreenEvent.Open);
        }
    }
}
