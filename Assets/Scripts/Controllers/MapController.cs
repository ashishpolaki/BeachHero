using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeachHero
{
    public class MapController : MonoBehaviour
    {
        public static MapController GetInstance { get; private set; }

        #region Constants 
        private static readonly Vector2 DefaultTextureScale = new Vector2(1, 1);
        private static readonly Vector2 ReferenceMapScale = new Vector2(4.5f, 4f);

        private static readonly float ZoomOutThick = 0.55f;
        private static readonly float ZoomDuration = 1.5f;
        private static readonly float ReferenceOrthoSize = 31f;
        #endregion

        #region Inspector Variables
        [SerializeField] private LevelDatabaseSO levelDatabase;
        [SerializeField] private Collider2D confineCollider;

        [Header("Map")]
        [SerializeField] private Transform mapBG;
        [SerializeField] private MapData[] mapDatas;

        [Header("Boat")]
        [SerializeField] private Transform boatTransform;
        [SerializeField] private float boatDuration = 1f;
        [SerializeField] private float boatOffsetDistance = 0.5f;
        [SerializeField] private Ease boatEase = Ease.OutCubic;

        [Header("Map Switch Tween Data")]
        [SerializeField] private float mapSwitchInScale = 0.8f;
        [SerializeField] private float mapSwitchOutScale = 0.8f;
        [SerializeField] private float mapSwitchScaleDuration = 0.2f;
        [SerializeField] private float mapSwitchScaleOvershoot = 4f;
        [SerializeField] private Ease mapSwitchScaleEase = Ease.InBack;

        [SerializeField] private float mapSwitchMoveDuration = 0.5f;
        [SerializeField] private Ease mapSwitchMoveEase = Ease.Linear;
        #endregion

        #region Private Variables
        private Tween boatTween;
        private int mapNumber = 0;
        private bool isNewMapUnlocked = false;
        #endregion

        #region Properties
        public bool IsNewMapUnlocked => isNewMapUnlocked;
        public int MapNumber
        {
            get => mapNumber;
            private set
            {
                mapNumber = value;
                SaveSystem.SaveInt(StringUtils.MAP_NUMER, mapNumber);
            }
        }
        public int TotalMaps => mapDatas.Length;
        #endregion

        #region Events
        public event Action OnMapButtonsEnabled;
        public event Action OnShowPowerupSelection;
        public event Action OnMapUnlocked;
        #endregion

        #region Unity Methods
        public void Awake()
        {
            if (GetInstance == null)
            {
                GetInstance = this;
            }
            InitializeMapVisuals();
            CameraController.GetInstance.SetCollider(confineCollider, GameCameraType.Map);
        }
        private void OnDestroy()
        {
            if (GetInstance == this)
            {
                GetInstance = null;
            }
        }
        #endregion

        #region Initialization
        private void InitializeMapVisuals()
        {
            // Initialize Map Data
            foreach (var mapData in mapDatas)
            {
                mapData.mapObject.SetActive(false);
            }

            //Set Map Visuals
            int savedMapNumber = SaveSystem.LoadInt(StringUtils.MAP_NUMER, IntUtils.DEFAULT_MAP_NUMBER);
            var levelNumber = GameController.GetInstance.CurrentLevelIndex + 1;

            for (int i = 0; i < mapDatas.Length; i++)
            {
                if (levelNumber >= mapDatas[i].startLevelNumber && levelNumber <= mapDatas[i].endLevelNumber)
                {
                    if (savedMapNumber != mapDatas[i].mapNumber)
                    {
                        savedMapNumber = mapDatas[i].mapNumber;
                    }
                    break;
                }
            }
            SwitchMap(-1, savedMapNumber);
        }
        #endregion

        #region Map State Checking
        public void CheckForMapUpdate()
        {
            int savedMapNumber = SaveSystem.LoadInt(StringUtils.MAP_NUMER, IntUtils.DEFAULT_MAP_NUMBER);
            var levelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            for (int i = 0; i < mapDatas.Length; i++)
            {
                if (levelNumber >= mapDatas[i].startLevelNumber && levelNumber <= mapDatas[i].endLevelNumber)
                {
                    if (savedMapNumber != mapDatas[i].mapNumber)
                    {
                        isNewMapUnlocked = true;
                    }
                    MapNumber = mapDatas[i].mapNumber;
                    break;
                }
            }

            if (isNewMapUnlocked)
            {
                SwitchMap(savedMapNumber, MapNumber);
            }
        }
        #endregion

        #region Boat Movement
        public void PlaceBoatAtCurrentLevel()
        {
            if (isNewMapUnlocked)
            {
                UnlockNewMap();
            }
            int levelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            PositionBoatAtLevel(levelNumber);
            OnMapButtonsEnabled?.Invoke();
        }

        public void PlaceBoatAtPreviousLevel()
        {
            int previousLevelNumber = GameController.GetInstance.CurrentLevelIndex;
            PositionBoatAtLevel(previousLevelNumber);
        }

        private void PositionBoatAtLevel(int levelNumber)
        {
            if (mapDatas[mapNumber - 1].IsLevelExists(levelNumber))
            {
                Transform levelVisual = mapDatas[mapNumber - 1].GetLevelVisual(levelNumber).transform;
                mapDatas[mapNumber - 1].CalculateOffsetDirection(levelVisual, out Vector3 offsetDir);
                boatTransform.SetPositionAndRotation(
                    levelVisual.position + offsetDir * boatOffsetDistance,
                    levelVisual.rotation);
            }
        }

        public void SetBoatObjectActive(bool enable)
        {
            boatTransform.gameObject.SetActive(enable);
        }

        public void AnimateBoatToCurrentLevel()
        {
            if (isNewMapUnlocked)
            {
                PlaceBoatAtCurrentLevel();
                return;
            }

            int currentLevelIndex = GameController.GetInstance.CurrentLevelIndex;
            int previousLevelIndex = currentLevelIndex - 1;
            previousLevelIndex = mapDatas[mapNumber - 1].GetCurrentLevelIndex(previousLevelIndex + 1);
            currentLevelIndex = mapDatas[mapNumber - 1].GetCurrentLevelIndex(currentLevelIndex + 1);

            if (currentLevelIndex < mapDatas[mapNumber - 1].points.Count)
            {
                BezierPoint bp0 = mapDatas[mapNumber - 1].points[previousLevelIndex];
                BezierPoint bp1 = mapDatas[mapNumber - 1].points[currentLevelIndex];

                Vector3 p0 = bp0.anchorPoint;
                Vector3 p1 = p0 + bp0.outTangent;
                Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
                Vector3 p3 = bp1.anchorPoint;

                float time = 0;
                boatTween.Kill();
                boatTween = DOTween.To(
                    () => time, x =>
                    {
                        time = x;
                        Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, time);
                        Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, time).normalized;
                        boatTransform.up = Vector3.Lerp(boatTransform.up, forward, Time.deltaTime * 10f);
                        mapDatas[mapNumber - 1].CalculateOffsetDirectionFromCross(boatTransform.up, out Vector3 boatOffsetDirection);
                        boatTransform.position = pos + boatOffsetDirection * boatOffsetDistance;
                    },
                    1,
                    boatDuration).SetEase(boatEase).OnComplete(() =>
                    {
                        OnShowPowerupSelection?.Invoke();
                    });
            }
        }
        #endregion

        #region Map
        public void UpdatePathLine()
        {
            var pathLine = mapDatas[mapNumber - 1].pathLine;
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, ZoomOutThick, ZoomDuration);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, ZoomOutThick, ZoomDuration);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, DefaultTextureScale, ZoomDuration);
        }

        public string GetMapName(int mapNumber)
        {
            for (int i = 0; i < mapDatas.Length; i++)
            {
                if (mapDatas[i].mapNumber == mapNumber)
                {
                    return mapDatas[i].name;
                }
            }
            return string.Empty;
        }
        public string GetMapDescription(int mapNumber)
        {
            for (int i = 0; i < mapDatas.Length; i++)
            {
                if (mapDatas[i].mapNumber == mapNumber)
                {
                    return mapDatas[i].description;
                }
            }
            return string.Empty;
        }
        private void UnlockNewMap()
        {
            isNewMapUnlocked = false;
            OnMapUnlocked?.Invoke();
        }

        public void SwitchMap(int previousMap, int currentMap, bool isPlayAnim = false)
        {
            UpdateMapBGScale();
            int mapMultiplier = currentMap > previousMap ? -1 : 1;
            if (isPlayAnim)
            {
                //Previous Map Animation Out
                if (previousMap > 0)
                {
                    var previousMapData = mapDatas[previousMap - 1];
                    previousMapData.mapObject.transform.DOScale(mapSwitchInScale, mapSwitchScaleDuration)
                        .SetEase(mapSwitchScaleEase, mapSwitchScaleOvershoot)
                        .OnComplete(() =>
                        {
                            previousMapData.mapObject.transform.DOMoveX(
                                previousMapData.mapObject.transform.position.x + (mapMultiplier * 50), mapSwitchMoveDuration)
                            .SetEase(mapSwitchMoveEase);
                        });
                }

                var currentMapData = mapDatas[currentMap - 1];
                currentMapData.mapObject.transform.DOScale(mapSwitchInScale, mapSwitchScaleDuration)
                     .SetEase(mapSwitchScaleEase, mapSwitchScaleOvershoot)
                        .OnComplete(() =>
                        {
                            currentMapData.mapObject.SetActive(true);
                            currentMapData.LevelSetup(levelDatabase);
                            currentMapData.mapObject.transform.position = new Vector3(
                                                                      -mapMultiplier * 50,
                                currentMapData.mapObject.transform.position.y,
                                currentMapData.mapObject.transform.position.z);

                            currentMapData.mapObject.transform.DOMoveX(
                                currentMapData.mapObject.transform.position.x + (mapMultiplier * 50), mapSwitchMoveDuration)
                            .SetEase(mapSwitchMoveEase).OnComplete(() =>
                            {
                                currentMapData.mapObject.transform.DOScale(1f, mapSwitchScaleDuration)
                                .SetEase(mapSwitchScaleEase, mapSwitchScaleOvershoot);
                            });
                        });
            }
            else
            {
                //If startLevelIndex is -1, it means no levels are set for this map
                if (mapDatas[currentMap - 1].startLevelNumber != -1)
                {
                    //PRevious
                    if (previousMap > 0)
                    {
                        mapDatas[previousMap - 1].mapObject.SetActive(false);
                    }

                    //Current
                    mapDatas[currentMap - 1].mapObject.SetActive(true);
                    mapDatas[currentMap - 1].LevelSetup(levelDatabase);
                }
            }
        }
        public void UpdateMapBGScale()
        {
            CameraController.GetInstance.SetOrthoSize(ScreenResolutionUtils.GetOrthographicSize(ReferenceOrthoSize), GameCameraType.Map);
            if (mapBG != null)
            {
                var scale = ScreenResolutionUtils.GetObjectScale(ReferenceMapScale, ReferenceOrthoSize);
                mapBG.localScale = new Vector3(scale.x, scale.y, 1f);
            }
        }
        #endregion

        #region Handle Click with UI and mouse click

        //private void OnEnable()
        //{
        //    if (InputManager.GetInstance != null)
        //    {
        //          InputManager.GetInstance.OnEscapePressed += ZoomOut;
        //          InputManager.GetInstance.OnMouseClickDown += HandleClick;
        //    }
        //}
        //private void OnDisable()
        //{
        //  if (InputManager.GetInstance != null)
        //  {
        //         InputManager.GetInstance.OnEscapePressed -= ZoomOut;
        //       InputManager.GetInstance.OnMouseClickDown -= HandleClick;
        //  }
        //}
        //  private Vector2 pendingClickPosition;
        //  private bool shouldCheckClick;
        //private void Update()
        //{
        //    if (shouldCheckClick)
        //    {
        //        shouldCheckClick = false;
        //        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        //        {
        //            // Click was on UI
        //            return;
        //        }

        //        Vector2 mousePos = Camera.main.ScreenToWorldPoint(pendingClickPosition);
        //        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f);

        //        if (hit.collider != null && hit.collider.TryGetComponent<LevelVisual>(out var levelVisual))
        //        {
        //            UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Push);
        //        }
        //    }
        //}
        //private void HandleClick(Vector2 screenPosition)
        //{
        //    pendingClickPosition = screenPosition;
        //    shouldCheckClick = true;
        //}
        #endregion


#if UNITY_EDITOR
        [Header("Editor")]
        public GameObject levelPrefab;
        public void GenerateLevelVisuals(int _mapIndex, List<BezierPoint> bezierPoints, Vector3[] linePoints)
        {
            //Destroy previous level visuals if any 
            var levelVisualChilds = mapDatas[_mapIndex].levelsParent.GetComponentsInChildren<LevelVisual>();
            foreach (var visual in levelVisualChilds)
            {
                Undo.DestroyObjectImmediate(visual.gameObject);
            }
            mapDatas[_mapIndex].levelVisuals.Clear();
            mapDatas[_mapIndex].points = bezierPoints;

            // Generate new level visuals based on bezier points
            SetPathLine(_mapIndex, linePoints);
            for (int i = 0; i < bezierPoints.Count; i++)
            {
                var levelObject = Instantiate(levelPrefab, mapDatas[_mapIndex].levelsParent);
                Undo.RegisterCreatedObjectUndo(levelObject, "Create Level Visual");
                mapDatas[_mapIndex].levelVisuals.Add(levelObject.GetComponent<LevelVisual>());
                mapDatas[_mapIndex].levelVisuals[i].SetPositions(bezierPoints[i].anchorPoint);
            }

            // Set up the level visuals
            for (var i = 0; i < mapDatas[_mapIndex].levelVisuals.Count - 1; i++)
            {
                var levelTransform = mapDatas[_mapIndex].levelVisuals[i].transform;
                BezierPoint bp0 = bezierPoints[i];
                BezierPoint bp1 = bezierPoints[i + 1];

                Vector3 p0 = bp0.anchorPoint;
                Vector3 p1 = p0 + bp0.outTangent;
                Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
                Vector3 p3 = bp1.anchorPoint;

                Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, 0.1f).normalized;
                levelTransform.up = forward;
            }
        }
        public void SetPathLine(int _mapIndex, Vector3[] linePoints)
        {
            mapDatas[_mapIndex].pathLine.positionCount = linePoints.Length;
            mapDatas[_mapIndex].pathLine.SetPositions(linePoints);
        }
#endif
    }
}
