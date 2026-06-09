using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
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
        [Header("References")]
        [SerializeField] private LevelDatabaseSO levelDatabase;
        [SerializeField] private SplineSystem splineSystem;
        [SerializeField] private Transform target;
        [SerializeField] private Transform visualChild;
        [SerializeField] private Animator characterAnimator;

        [Header("Map")]
        [SerializeField] private List<MapLevelSpawnData> mapLevels = new List<MapLevelSpawnData>();
        [SerializeField] private Transform levelsParent;

        [Header("LevelNumbers Text Style")]
        [SerializeField] private TextMeshPro[] levelNumbersTexts;
        [SerializeField] private Color underlayColor;
        [SerializeField] private float underlayOffsetX;
        [SerializeField] private float underlayOffsetY;
        [SerializeField] private float underlayThickness; //Dilate
        [SerializeField] private float underlaySoftness;


        [Header("Water")]
        [SerializeField] private Transform[] waterTransforms;
        [SerializeField] private float waterSpeed = 0.5f;

        [Header("Camera Scroll")]
        [SerializeField] private float scrollSpeed = 5f;

        [Header("Coming Soon")]
        [SerializeField] private TextMeshPro comingSoonTxt;
        [SerializeField] private float comingSoonOffsetY;
        [SerializeField] private float comingSoonOffsetFromLastLevel;
        #endregion

        #region Private Variables
        private CancellationTokenSource moveCTS;
        private float currentSplinePercent = 0f;
        private bool isLevelsInit = false;
        private bool isInLevelTransition = false;
        private int selectedLevelIndex = -1;

        // === Camera Scroll Variables ===
        private bool isDragging = false;
        private float lastPointerY;
        private float currentScrollY;
        private const float cameraStartDistance = 4.3f;
        private const float cameraEndDistance = 28.7f;

        private float spriteHeight;
        private float spriteThreshold;
        #endregion

        #region Properties
        public SplineSystem SplineSystem => splineSystem;
        public List<MapLevelSpawnData> MapLevels => mapLevels;
        public Transform LevelsParent => levelsParent;
        public event Action OnMapButtonsEnabled;
        #endregion

        #region Unity Methods
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                if (levelsParent != null)
                    levelNumbersTexts = levelsParent.GetComponentsInChildren<TextMeshPro>(true);
            }
        }
#endif
        private void Awake()
        {
            if (GetInstance == null)
            {
                GetInstance = this;
            }
            InitializeWater();
            ApplyLevelNumberTextStyle();
        }
        private void OnEnable()
        {
            InputManager.GetInstance.OnMouseClickDown += HandleMapClick;
            InputManager.GetInstance.OnMouseClickUp += HandleClickUp;
        }
        private void OnDisable()
        {
            if (InputManager.GetInstance != null)
            {
                InputManager.GetInstance.OnMouseClickDown -= HandleMapClick;
                InputManager.GetInstance.OnMouseClickUp -= HandleClickUp;

            }
            moveCTS?.Cancel();
        }
        private void OnDestroy()
        {
            if (GetInstance == this)
            {
                GetInstance = null;
            }
        }
        public void UpdateState()
        {
            if (isDragging)
            {
                Vector2 pos = InputManager.MousePosition;
                float currentY = pos.y;
                float delta = currentY - lastPointerY;
                Vector3 firstPos = mapLevels[0].levelVisual.transform.position;
                Vector3 lastPos = mapLevels[MapLevels.Count - 1].levelVisual.transform.position;
                Vector3 forward = CameraController.GetInstance.GetCameraForward(GameCameraType.Map);
                float minY = firstPos.y - forward.y * cameraStartDistance;
                float maxY = lastPos.y - forward.y * (cameraEndDistance - comingSoonOffsetFromLastLevel);

                // ===== APPLY DRAG =====
                currentScrollY -= delta * 0.01f * scrollSpeed;
                currentScrollY = Mathf.Clamp(currentScrollY, minY, maxY);
                lastPointerY = currentY;
                MoveCamera();
            }

            UpdateWaterLoop();
        }
        #endregion

        #region Water Methods
        private void InitializeWater()
        {
            if (waterTransforms == null || waterTransforms.Length == 0)
                return;

            var sprite = waterTransforms[0].GetComponent<SpriteRenderer>();
            spriteHeight = sprite.bounds.size.y;
            spriteThreshold = spriteHeight * 2f;
        }

        private void UpdateWaterLoop()
        {
            if (waterTransforms == null || waterTransforms.Length == 0)
                return;

            float lowestY = float.MaxValue;
            float highestY = float.MinValue;
            int lowestIndex = 0;
            int highestIndex = 0;
            float moveDelta = waterSpeed * Time.deltaTime;
            float camY = CameraController.GetInstance.GetCameraPosition(GameCameraType.Map).y;

            for (int i = 0; i < waterTransforms.Length; i++)
            {
                Transform t = waterTransforms[i];
                // Move first
                Vector3 pos = t.position;
                pos.y -= moveDelta;
                t.position = pos;
                float y = pos.y;

                if (y < lowestY)
                {
                    lowestY = y;
                    lowestIndex = i;
                }

                if (y > highestY)
                {
                    highestY = y;
                    highestIndex = i;
                }
            }

            //  Recycle bottom -> top
            if (lowestY < camY - spriteThreshold)
            {
                Transform lowest = waterTransforms[lowestIndex];
                Transform highest = waterTransforms[highestIndex];
                Vector3 pos = lowest.position;
                pos.y = highest.position.y + spriteHeight;
                lowest.position = pos;
            }

            //  Recycle top -> bottom
            if (highestY > camY + spriteThreshold)
            {
                Transform lowest = waterTransforms[lowestIndex];
                Transform highest = waterTransforms[highestIndex];
                Vector3 pos = highest.position;
                pos.y = lowest.position.y - spriteHeight;
                highest.position = pos;
            }
        }
        #endregion

        #region Camera Methods

        private void MoveCamera()
        {
            Vector3 pos = CameraController.GetInstance.GetCameraPosition(GameCameraType.Map);
            pos.y = currentScrollY;
            CameraController.GetInstance.SetCameraPosition(pos);
        }

        void CenterCameraToCurrentLevel()
        {
            int index = GameController.GetInstance.CurrentLevelIndex;

            if (index < 0 || index >= mapLevels.Count)
                return;

            Vector3 levelPos = mapLevels[index].levelVisual.transform.position;
            Vector3 forward = CameraController.GetInstance.GetCameraForward(GameCameraType.Map);
            Vector3 firstPos = mapLevels[0].levelVisual.transform.position;
            Vector3 lastPos = mapLevels[MapLevels.Count - 1].levelVisual.transform.position;
            float minY = firstPos.y - forward.y * cameraStartDistance;
            float maxY = lastPos.y - forward.y * (cameraEndDistance - comingSoonOffsetFromLastLevel);
            currentScrollY = levelPos.y - 7f;
            currentScrollY = Mathf.Clamp(currentScrollY, minY, maxY);
            MoveCamera();
        }

        #endregion

        #region Input Methods
        private void HandleClickUp(Vector2 mousePos)
        {
            if (GameController.GetInstance.GameState != GameState.Map)
            {
                return;
            }
            isDragging = false;
        }

        public void HandleMapClick(Vector2 mousePos)
        {
            if (GameController.GetInstance.GameState == GameState.Map)
            {
                var ray = Camera.main.ScreenPointToRay(InputManager.MousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Map")))
                {
                    var levelVisual = hit.collider.GetComponent<LevelVisual>();
                    if (levelVisual != null)
                    {
                        Action action = () =>
                        {
                            // Prevent clicking another level while in transition
                            if (!isInLevelTransition)
                            {
                                isInLevelTransition = true;
                                selectedLevelIndex = levelVisual.LevelNumber - 1;
                                // if the selected level is not the current level, set it as current.
                                if (selectedLevelIndex != GameController.GetInstance.CurrentLevelIndex)
                                {
                                    GameController.GetInstance.SetLevel(selectedLevelIndex);
                                }
                                StartGame();
                            }
                        };
                        levelVisual.PressAnimation(() => levelVisual.ReleaseAnimation(action));
                    }
                }
                else
                {
                    isDragging = true;
                    lastPointerY = InputManager.MousePosition.y;
                }
            }
        }
        #endregion

        #region Movement
        public void UpdateCharacterTransform(float percent,bool forward = true)
        {
            target.position = splineSystem.GetPoint(percent);
            target.rotation = splineSystem.GetForwardRotation(percent, forward);
            visualChild.localRotation = splineSystem.GetTwistRotation(percent);
        }

        private async UniTask MoveAlongSplineAsync(float start, float end)
        {
            moveCTS?.Cancel();
            moveCTS = new CancellationTokenSource();
            var token = moveCTS.Token;

            float totalDistance = splineSystem.CalculateDistance(start, end);
            float coveredDistance = 0f;

            float maxSpeed = 3f; // tune this only
            bool isIdle = false;

            characterAnimator.CrossFade("Run", 0.01f);
            try
            {
                while (coveredDistance < totalDistance)
                {
                    token.ThrowIfCancellationRequested();

                    float remainingDistance = totalDistance - coveredDistance;

                    float t = coveredDistance / totalDistance;

                    float speedFactor = Mathf.Sin(t * Mathf.PI);

                    float minSpeed = 0.5f; // important
                    float currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedFactor);

                    float deltaDistance = currentSpeed * Time.deltaTime;

                    if (coveredDistance + deltaDistance > totalDistance)
                        deltaDistance = totalDistance - coveredDistance;

                    coveredDistance += deltaDistance;

                    float percent = coveredDistance / totalDistance;
                    currentSplinePercent = Mathf.Lerp(start, end, percent);

                    UpdateCharacterTransform(currentSplinePercent);

                    float idleTriggerDistance = totalDistance * 0.05f;
                    if (remainingDistance <= idleTriggerDistance && !isIdle)
                    {
                        characterAnimator.CrossFade("Idle", 0.2f);
                        isIdle = true;
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                currentSplinePercent = end;
                UpdateCharacterTransform(end);
                StartGame();
            }
            catch (OperationCanceledException)
            {
                // safe cancel
            }
        }

        public async void MoveToLevelAsync(float start, float end)
        {
           await MoveAlongSplineAsync(start, end);
        }
        #endregion
        #region Reset

        public void ResetData()
        {
            isInLevelTransition = false;
            moveCTS?.Cancel();
            characterAnimator.CrossFade("Idle", 0.1f);
        }
        #endregion

        #region Initialization
        public void InitializeMapVisuals()
        {
            CenterCameraToCurrentLevel();
            if (isLevelsInit)
            {
                return;
            }
            isLevelsInit = true;
            SetupLevels();
            SetComingSoonText();
        }
        public void SetupLevels()
        {
            // Set level visuals to face camera
            Vector3 camRot = CameraController.GetInstance.GetCameraRotation(GameCameraType.Map);
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
        private void SetComingSoonText()
        {
            if (comingSoonTxt != null && mapLevels.Count > 0)
            {
                float Y = (mapLevels[^1].levelVisual.transform.position.y + comingSoonOffsetY);
                comingSoonTxt.transform.position = new Vector3(comingSoonTxt.transform.position.x, Y, 0);
            }
        }
        private void ApplyLevelNumberTextStyle()
        {
            if (levelNumbersTexts == null || levelNumbersTexts.Length == 0)
                return;

            Material sharedMat = new Material(levelNumbersTexts[0].fontMaterial);
            sharedMat.EnableKeyword("UNDERLAY_ON");
            sharedMat.SetColor("_UnderlayColor", underlayColor);
            sharedMat.SetFloat("_UnderlayOffsetX", underlayOffsetX);
            sharedMat.SetFloat("_UnderlayOffsetY", underlayOffsetY);
            sharedMat.SetFloat("_UnderlaySoftness", underlaySoftness);
            sharedMat.SetFloat("_UnderlayDilate", underlayThickness);
            for (int i = 0; i < levelNumbersTexts.Length; i++)
            {
                var text = levelNumbersTexts[i];
                if (text == null) continue;
                text.fontMaterial = sharedMat;
            }
        }
        #endregion

        #region Level Progression

        private async void StartGame()
        {
            isInLevelTransition = false;
            currentSplinePercent = mapLevels[selectedLevelIndex].splinePercent;
            await UIController.GetInstance.LoadingUI.ShowLoadingScreen();
            GameController.GetInstance.StartGameplay();
            await UIController.GetInstance.LoadingUI.DisableLoadingScreen();
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
                isInLevelTransition = true;
                selectedLevelIndex = GameController.GetInstance.CurrentLevelIndex;
                MoveToLevelAsync(currentSplinePercent, mapLevels[selectedLevelIndex].splinePercent);
            }
        }
        #endregion
    }
}
