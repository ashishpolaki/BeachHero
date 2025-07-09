using UnityEngine;

namespace BeachHero
{
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
        private bool isGameStarted = false;
        private bool isLevelPass = false;
        private CameraController cameraController;

        #region Properties
        public int CurrentLevelIndex => currentLevelIndex;
        public PoolController PoolManager => poolManager;
        public LevelController LevelController => levelController;
        public PowerupController PowerupController => powerupController;
        public TutorialController TutorialController => tutorialController;
        public StoreController StoreController => storeController;
        public SkinController SkinController => skinController;
        public CameraController CameraController
        {
            get
            {
                return cameraController;
            }
        }
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
            Application.targetFrameRate = 30;
            AudioController.GetInstance.Init();
            AdController.GetInstance.Init();
            powerupController.Init();
            storeController.Init();
            levelDatabaseSO.Init();
            SpawnLevel();
        }
        #endregion

        #region Cache Component
        public void CacheCameraController(CameraController _cameraController)
        {
            cameraController = _cameraController;
            if (_cameraController == null)
            {
                DebugUtils.LogError("CameraController is null");
            }
        }
        #endregion

        private void SpawnLevel()
        {
            isGameStarted = false;
            isLevelPass = false;
            currentLevelIndex = SaveSystem.LoadInt(StringUtils.LEVELNUMBER, IntUtils.DEFAULT_LEVEL) - 1;
            UIController.GetInstance.ScreenEvent(ScreenType.MainMenu, UIScreenEvent.Open);
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(currentLevelIndex));
            CameraController.Init();
        }

        public void Play()
        {
            isGameStarted = true;
            bool isFTUE = tutorialController.IsFTUE(currentLevelIndex + 1);
            ScreenTabType screenTabType = isFTUE ? ScreenTabType.Tutorial : ScreenTabType.None;
            levelController.GameStart(isFTUE);
            UIController.GetInstance.ScreenEvent(ScreenType.Gameplay, UIScreenEvent.Open, screenTabType);
            ActivatePowerups();
        }

        public void OnCharacterPickUp()
        {
            levelController.OnCharacterPickUp();
        }

        #region Level 
        public void RetryLevel()
        {
            SpawnLevel();
        }

        public void NextLevel()
        {
            isGameStarted = false;
            isLevelPass = false;
            currentLevelIndex = SaveSystem.LoadInt(StringUtils.LEVELNUMBER, IntUtils.DEFAULT_LEVEL) - 1;
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(currentLevelIndex));
            CameraController.Init();
        }
        public void OnLevelPass()
        {
            currentLevelIndex++;
            SaveSystem.SaveInt(StringUtils.LEVELNUMBER, currentLevelIndex + 1);
            isLevelPass = true;
            levelController.OnLevelCompleted(true);
            UIController.GetInstance.ScreenEvent(ScreenType.GameWin, UIScreenEvent.Open);
        }
        public void OnLevelFailed()
        {
            if (isLevelPass)
            {
                return; // If the level is already passed, do not allow to fail again.
            }
            isLevelPass = false;
            AudioController.GetInstance.PlaySound(AudioType.Gamelose);
            levelController.OnLevelCompleted(false);
            UIController.GetInstance.ScreenEvent(ScreenType.GameLose, UIScreenEvent.Open);
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
            if (isLevelPass)
            {
                cameraController.OnLevelPass(levelController.PlayerTransform);
            }
        }
        #endregion
    }
}
