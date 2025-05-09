using UnityEngine;

namespace BeachHero
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;
        public static GameController Instance { get => instance; }

        [SerializeField] private LevelController levelController;
        [SerializeField] private LevelDatabaseSO levelDatabaseSO;
        [SerializeField] private PoolManager poolManager;

        [Tooltip("The Index Starts from 0")]
        private int currentLevelIndex;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
            levelController.UpdateState();
        }

        private void OnDestroy()
        {
            poolManager.Reset();
        }

        private void Start()
        {
            currentLevelIndex = 0;
            levelController.StartState(levelDatabaseSO.GetLevelByIndex(currentLevelIndex));
        }

    }
}
