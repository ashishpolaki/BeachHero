using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public enum GameState
    {
        NotStarted,
        Playing,
        Paused,
        LevelWin,
        LevelFail,
        Map
    }
    [System.Serializable]
    public struct LevelFailDelay
    {
        public LevelFailDelayType type;
        public float seconds;
    }
    public enum LevelFailDelayType
    {
        None,
        Short,
        Medium,
        Long
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
        [SerializeField] private LevelFailDelay[] levelFailDelays;

        [Tooltip("The Index Starts from 0")]
        private int currentLevelIndex;

        private GameState gameState = GameState.NotStarted;

        #region Properties
        public GameState GameState => gameState;
        public int CurrentLevelIndex => currentLevelIndex;
        public PoolController PoolManager => poolManager;
        public LevelController LevelController => levelController;
        public PowerupController PowerupController => powerupController;
        public TutorialController TutorialController => tutorialController;
        public StoreController StoreController => storeController;
        public SkinController SkinController => skinController;
        #endregion

        #region Unity Methods
        private void Update()
        {
            if (GameState == GameState.Playing || GameState == GameState.LevelWin)
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
        public void Init()
        {
            currentLevelIndex = SaveSystem.LoadInt(StringUtils.LEVELNUMBER, IntUtils.DEFAULT_LEVEL) - 1;
            powerupController.Init();
            storeController.Init();
        }
        public void SpawnLevel()
        {
            currentLevelIndex = SaveSystem.LoadInt(StringUtils.LEVELNUMBER, IntUtils.DEFAULT_LEVEL) - 1;
            InitializeLevel();
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
        }
        private void InitializeLevel()
        {
            SetGameState(GameState.NotStarted);
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(currentLevelIndex));
            CameraController.GetInstance.SetActiveCamera(GameCameraType.GameView);
            levelDatabaseSO.Init();
        }
        #endregion

        #region Game Flow
        public void Play()
        {
            SetGameState(GameState.Playing);
            CameraController.GetInstance.SetActiveCamera(GameCameraType.GameView);
            bool isFTUE = tutorialController.IsFTUE(currentLevelIndex + 1);
            ScreenTabType screenTabType = isFTUE ? ScreenTabType.FTUE : ScreenTabType.None;
            levelController.InitializePlayerData(isFTUE);
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
        public void OnLevelWin()
        {
            IncrementLevel();
            SetGameState(GameState.LevelWin);
            levelController.SetLevelCompletionResult(true);
        }
        public void OnLevelFailed(LevelFailDelayType levelFailDelayType)
        {
            // If the level is already passed, do not allow to fail again.
            if (GameState == GameState.LevelWin)
            {
                LevelWinFeedback();
                return;
            }
            StartCoroutine(IELevelFailed(levelFailDelayType));
        }
        IEnumerator IELevelFailed(LevelFailDelayType levelFailDelayType)
        {
            SetGameState(GameState.LevelFail);
            levelController.SetLevelCompletionResult(false);
            float delay = GetLevelFailDelayInSeconds(levelFailDelayType);
            yield return new WaitForSeconds(delay);
            LevelFailFeedback();
        }
        private void LevelFailFeedback()
        {
            AudioController.GetInstance.PlaySound(AudioType.Gamelose);
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Open, ScreenTabType.LevelFail);
        }
        private void LevelWinFeedback()
        {
            UIController.GetInstance.ScreenEvent(ScreenType.Results, UIScreenEvent.Open, ScreenTabType.LevelWin);
            AudioController.GetInstance.PlaySound(AudioType.Gamewin);
        }
        private float GetLevelFailDelayInSeconds(LevelFailDelayType type)
        {
            foreach (LevelFailDelay delay in levelFailDelays)
            {
                if (delay.type == type)
                {
                    return delay.seconds;
                }
            }
            return 0f;
        }
        #endregion

        #region Collect
        public void OnCharacterPickUp()
        {
            levelController.OnDrownCharacterPickUp();
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

        #region Utilities
        public void SetGameState(GameState state)
        {
            gameState = state;
        }
        #endregion
    }
}
