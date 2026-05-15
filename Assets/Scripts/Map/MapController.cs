using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BeachHero
{
    [Serializable]
    public class MapLevelSpawnData
    {
        public int levelNumber;

        [Header("Spline Position")]
#if UNITY_EDITOR
        public int segmentIndex;
        [Range(0f, 1f)]
        public float t;
#endif
        public float splinePercent;

        [Header("Transform")]
        public Vector3 scale = Vector3.one;
        public Vector3 rotation;

        public LevelVisual levelVisual;
    }
    public class MapController : MonoBehaviour
    {
        public static MapController GetInstance { get; private set; }

        #region Inspector Variables
        [SerializeField] private LevelDatabaseSO levelDatabase;
        [SerializeField] private List<SplinePoint> pathPoints = new List<SplinePoint>();
        [SerializeField] private List<MapLevelSpawnData> mapLevels = new List<MapLevelSpawnData>();
        [SerializeField] private Transform levelsParent;
        [SerializeField] private Transform target;
        [SerializeField] private Transform visualChild;
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private float moveSpeed = 0.5f;

        [Range(5, 100)]
        public int resolution = 20;
        #endregion

        #region Private Variables
        private CancellationTokenSource moveCTS;
        private float currentSplinePercent = 0f;
        private bool isLevelsInit = false;
        private int selectedLevelIndex = -1;
        #endregion

        #region Properties
        public List<SplinePoint> PathPoints => pathPoints;
        public List<MapLevelSpawnData> MapLevels => mapLevels;
        public Transform LevelsParent => levelsParent;
        #endregion

        public event Action OnMapButtonsEnabled;

        #region Spline Methods
        public Vector3 GetTangent(float percent)
        {
            percent = Mathf.Clamp01(percent);

            if (pathPoints == null || pathPoints.Count < 4)
                return Vector3.forward;

            List<Vector3> pts = GetPositions();
            return CatmullSplineUtils.GetTangentOnSpline(pts, percent);
        }

        public Quaternion GetForwardRotation(float percent, bool isForward = true)
        {
            float safePercent = Mathf.Clamp01(percent);
            safePercent = Mathf.Min(safePercent, 0.98f);

            Vector3 dir = GetTangent(percent);

            if (dir == Vector3.zero)
                return Quaternion.identity;

            if (!isForward)
                dir = -dir;

            return Quaternion.LookRotation(dir, Vector3.back);
        }

        public Quaternion GetTwistRotation(float percent)
        {
            percent = Mathf.Clamp01(percent);

            int count = pathPoints.Count;
            if (count < 2)
                return Quaternion.identity;

            float scaled = percent * (count - 1);
            int i = Mathf.FloorToInt(scaled);
            float t = scaled - i;

            i = Mathf.Clamp(i, 0, count - 2);

            Quaternion a = pathPoints[i].rotation;
            Quaternion b = pathPoints[i + 1].rotation;

            if (Quaternion.Dot(a, b) < 0f)
            {
                b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
            }

            Quaternion rot = Quaternion.Slerp(a, b, t);
            return rot;
        }

        public Vector3 GetPoint(float percent)
        {
            percent = Mathf.Clamp01(percent);

            if (pathPoints == null || pathPoints.Count < 4)
                return transform.position;

            List<Vector3> pts = GetPositions();
            return CatmullSplineUtils.GetPointOnSpline(pts, percent);
        }

        public List<Vector3> GetPositions()
        {
            List<Vector3> pts = new List<Vector3>();
            for (int i = 0; i < pathPoints.Count; i++)
                pts.Add(pathPoints[i].position);
            return pts;
        }

        public void UpdateCharacterTransform(float percent, bool isForward = true)
        {
            target.position = GetPoint(percent);
            target.rotation = GetForwardRotation(percent, isForward);
            visualChild.localRotation = GetTwistRotation(percent);
        }
        #endregion

        #region Unity Methods
        public void Awake()
        {
            if (GetInstance == null)
            {
                GetInstance = this;
            }
        }
        private void OnEnable()
        {
            InputManager.GetInstance.OnMouseClickDown += HandleMapClick;
        }
        private void OnDisable()
        {
            if (InputManager.GetInstance != null)
                InputManager.GetInstance.OnMouseClickDown -= HandleMapClick;
            moveCTS?.Cancel();
        }

        private void OnDestroy()
        {
            if (GetInstance == this)
            {
                GetInstance = null;
            }
        }
        #endregion

        #region Input
        public void HandleMapClick(Vector2 mousePos)
        {
            var ray = Camera.main.ScreenPointToRay(InputManager.MousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Map")))
            {
                var levelVisual = hit.collider.GetComponent<LevelVisual>();
                if (levelVisual != null)
                {
                    selectedLevelIndex = levelVisual.LevelNumber - 1;
                    if (selectedLevelIndex == GameController.GetInstance.CurrentLevelIndex)
                    {
                        StartGame();
                        return;
                    }
                    GameController.GetInstance.SetLevel(selectedLevelIndex);
                    MoveToLevelAsync(currentSplinePercent, mapLevels[selectedLevelIndex].splinePercent);
                }
            }
        }
        #endregion


        #region Movement
        private float CalculateSplineDistance(float startPercent, float endPercent)
        {
            var pts = PathPoints;
            if (pts.Count < 4) return 0f;
            float distance = 0f;
            int steps = resolution * (pts.Count - 3); // same density as draw
            float prevT = startPercent;
            Vector3 prev = GetPoint(prevT);
            for (int i = 1; i <= steps; i++)
            {
                float lerpT = i / (float)steps;
                float t = Mathf.Lerp(startPercent, endPercent, lerpT);
                Vector3 p = GetPoint(t);
                distance += Vector3.Distance(prev, p);
                prev = p;
            }
            return distance;
        }

        private async UniTask MoveAlongSplineAsync(float start, float end, float duration)
        {
            moveCTS?.Cancel();
            moveCTS = new CancellationTokenSource();

            var token = moveCTS.Token;

            float time = 0f;
            bool idleTriggered = false;
            bool isForward = end > start;
            characterAnimator.CrossFade("Run", 0.1f);
            try
            {
                while (time < duration)
                {
                    token.ThrowIfCancellationRequested();

                    time += Time.deltaTime;

                    float t = time / duration;
                    t = Mathf.SmoothStep(0f, 1f, t);

                    currentSplinePercent = Mathf.Lerp(start, end, t);

                    UpdateCharacterTransform(currentSplinePercent, isForward);

                    if (!idleTriggered && t >= 0.9f)
                    {
                        characterAnimator.CrossFade("Idle", 0.3f);
                        idleTriggered = true;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                currentSplinePercent = end;
                UpdateCharacterTransform(end, isForward);
                StartGame();
            }
            catch (OperationCanceledException)
            {
                // movement cancelled (safe)
            }
        }

        public async void MoveToLevelAsync(float start, float end)
        {
            float distance = CalculateSplineDistance(start, end);
            float baseSpeed = 1f;     // normal speed
            float maxSpeed = 5f;

            float speed = Mathf.Lerp(baseSpeed, maxSpeed, distance / 10f);
            speed = Mathf.Clamp(speed, baseSpeed, maxSpeed);
            float duration = distance / speed;
            await MoveAlongSplineAsync(start, end, duration);
        }
        #endregion

        #region Initialization
        public void InitializeMapVisuals()
        {
            if (isLevelsInit)
            {
                return;
            }
            isLevelsInit = true;
            // Set level visuals to face camera
            Vector3 camRot = CameraController.GetInstance.GetCurrentCameraEulerAngles();
            for (int i = 0; i < mapLevels.Count; i++)
            {
                var levelVisual = mapLevels[i].levelVisual;
                if (levelVisual == null)
                    continue;

                var t = levelVisual.transform;
                Vector3 euler = t.eulerAngles;
                euler.x = camRot.x;
                t.eulerAngles = euler;

                levelVisual.Setup(levelDatabase.LevelDatas[i]);
            }
        }
        #endregion


        private async void StartGame()
        {
            await UIController.GetInstance.FadeUI.FadeInASync();
            GameController.GetInstance.StartGameplay();
            await UIController.GetInstance.FadeUI.FadeOutASync();
            GameController.GetInstance.LevelController.PlaySpawnAnimations();
        }

        public void OnLevelComplete(int medals)
        {
            int currentLevelIndex = GameController.GetInstance.CurrentLevelIndex;
            mapLevels[currentLevelIndex].levelVisual.OnLevelComplete(medals);
            //Next level should be unlocked
            if (currentLevelIndex + 1 < mapLevels.Count)
            {
                if (mapLevels[currentLevelIndex + 1].levelVisual.State == LevelVisualState.Locked)
                {
                    mapLevels[currentLevelIndex + 1].levelVisual.SetAsCurrentLevel();
                }
            }
        }

        public void SyncCharacterToLevel()
        {
            int currentLevelIndex = GameController.GetInstance.CurrentLevelIndex;
            if (currentLevelIndex >= 0 && currentLevelIndex < mapLevels.Count)
            {
                selectedLevelIndex = GameController.GetInstance.LoadCurrentLevelNumber() - 1;
                currentSplinePercent = mapLevels[selectedLevelIndex].splinePercent;
                UpdateCharacterTransform(currentSplinePercent);
            }
        }

        public void AnimateToLevel()
        {
            if (selectedLevelIndex + 1 != GameController.GetInstance.CurrentLevelIndex)
            {
                SyncCharacterToLevel();
            }
            else
            {
                selectedLevelIndex = GameController.GetInstance.CurrentLevelIndex;
                MoveToLevelAsync(currentSplinePercent, mapLevels[selectedLevelIndex].splinePercent);
            }
        }
    }
}
