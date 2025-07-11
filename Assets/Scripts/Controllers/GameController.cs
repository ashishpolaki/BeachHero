using UnityEngine;

namespace BeachHero
{
    public enum GameState
    {
        NotStarted,
        Playing,
        Paused,
        LevelPassed,
        LevelFailed,
    }
    public class GameController : SingleTon<GameController>
    {
        [SerializeField] private LevelDatabaseSO levelDatabaseSO;
        [SerializeField] private LevelController levelController;
        [SerializeField] private PoolController poolManager;
        [SerializeField] private PowerupController powerupController;
        [SerializeField] private TutorialController tutorialController;
        [SerializeField] private StoreController storeController;
        [SerializeField] private SkinController skinController;

        [Tooltip("The Index Starts from 0")]
        private int currentLevelIndex;

        private GameState gameState = GameState.NotStarted;
        private CameraController cameraController;

        #region Properties
        public GameState GameState => gameState;
        public int CurrentLevelIndex => currentLevelIndex;
        public PoolController PoolManager => poolManager;
        public LevelController LevelController => levelController;
        public PowerupController PowerupController => powerupController;
        public TutorialController TutorialController => tutorialController;
        public StoreController StoreController => storeController;
        public SkinController SkinController => skinController;
        public CameraController CameraController => cameraController;
        #endregion

        #region Unity Methods
        private void Start()
        {
            Application.targetFrameRate = 30;
            AudioController.GetInstance.Init();
            AdController.GetInstance.Init();
            powerupController.Init();
            storeController.Init();
            SpawnLevel();
        }
        private void Update()
        {
            if (GameState == GameState.Playing || GameState == GameState.LevelPassed)
            {
                levelController.UpdateState();
            }
        }
        private void OnDestroy()
        {
            poolManager.Reset();
        }
        #endregion

        #region Initialization
        public void CacheCameraController(CameraController _cameraController)
        {
            cameraController = _cameraController;
            if (_cameraController == null)
            {
                DebugUtils.LogError("CameraController is null");
            }
        }
        private void InitializeLevel()
        {
            SetGameState(GameState.NotStarted);
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(currentLevelIndex));
            cameraController.Init();
            levelDatabaseSO.Init();
        }
        private void SpawnLevel()
        {
            currentLevelIndex = SaveSystem.LoadInt(StringUtils.LEVELNUMBER, IntUtils.DEFAULT_LEVEL) - 1;
            InitializeLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
        }
        #endregion

        #region Game Flow
        public void Play()
        {
            SetGameState(GameState.Playing);
            bool isFTUE = tutorialController.IsFTUE(currentLevelIndex + 1);
            ScreenTabType screenTabType = isFTUE ? ScreenTabType.Tutorial : ScreenTabType.None;
            levelController.GameStart(isFTUE);
            UIController.GetInstance.ScreenEvent(ScreenType.Gameplay, UIScreenEvent.Open, screenTabType);
            ActivatePowerups();
        }
       
        public void RetryLevel()
        {
            InitializeLevel();
        }
        public void NextLevel()
        {
            InitializeLevel();
        }
        public void SkipLevel()
        {
            IncrementLevel();
            InitializeLevel();
        }
        private void IncrementLevel()
        {
            currentLevelIndex++;
            SaveSystem.SaveInt(StringUtils.LEVELNUMBER, currentLevelIndex + 1);
        }
        public void OnLevelPass()
        {
            IncrementLevel();
            gameState = GameState.LevelPassed;
            levelController.OnLevelCompleted(true);
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Open, ScreenTabType.LevelPass);
        }
        public void OnLevelFailed()
        {
            if (GameState == GameState.LevelPassed)
            {
                return; // If the level is already passed, do not allow to fail again.
            }
            gameState = GameState.LevelFailed;
            AudioController.GetInstance.PlaySound(AudioType.Gamelose);
            levelController.OnLevelCompleted(false);
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Open, ScreenTabType.LevelFail);
        }
        #endregion

        #region Collect
        public void OnCharacterPickUp()
        {
            levelController.OnCharacterPickUp();
        }
        public void OnGameCurrencyPickup()
        {
            levelController.OnGameCurrencyCollect();
        }
        #endregion

        #region Powerup
        private void ActivatePowerups()
        {
            if (powerupController.CurrentActivePowerupList.Count <= 0)
            {
                return;
            }
            foreach (PowerupType powerupType in powerupController.CurrentActivePowerupList)
            {
                levelController.OnActivatePowerup(powerupType);
            }
            powerupController.ActivateSelectedPowerups();
        }
        #endregion

        #region Camera
        public void OnLevelPassedCameraEffect()
        {
            if (GameState == GameState.LevelPassed)
            {
                cameraController.OnLevelPass(levelController.PlayerTransform);
            }
        }
        #endregion

        #region Utilities
        public void SetGameState(GameState state)
        {
            gameState = state;
        }
        #endregion
    }
}
