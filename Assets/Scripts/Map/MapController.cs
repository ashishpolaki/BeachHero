using UnityEngine;
using System;
using LitMotion;
using System.Collections.Generic;

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

        [Range(0f, 1f)]
        public float percent;

        [Range(5, 100)]
        public int resolution = 20;
        #endregion

        #region Properties
        public List<SplinePoint> PathPoints => pathPoints;
        public List<MapLevelSpawnData> MapLevels => mapLevels;
        public Transform LevelsParent => levelsParent;
        #endregion

        #region Spline Methods
        public Vector3 GetTangent(float percent)
        {
            percent = Mathf.Clamp01(percent);

            if (pathPoints == null || pathPoints.Count < 4)
                return Vector3.forward;

            List<Vector3> pts = GetPositions();
            return CatmullSplineUtils.GetTangentOnSpline(pts, percent);
        }
        public Quaternion GetForwardRotation(float percent)
        {
            float safePercent = Mathf.Clamp01(percent);
            safePercent = Mathf.Min(safePercent, 0.98f);

            Vector3 dir = GetTangent(safePercent);

            if (dir == Vector3.zero)
                return Quaternion.identity;

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

        public void UpdateTarget()
        {
            if (target != null && pathPoints != null && pathPoints.Count >= 4)
            {
                target.position = GetPoint(percent);

                target.rotation = GetForwardRotation(percent);

                visualChild.localRotation = GetTwistRotation(percent);
            }
        }
        #endregion

        #region Old Code

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
            var levelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            //for (int i = 0; i < mapDatas.Length; i++)
            //{
            //    if (levelNumber >= mapDatas[i].startLevelNumber && levelNumber <= mapDatas[i].endLevelNumber)
            //    {
            //        if (savedMapNumber != mapDatas[i].mapNumber)
            //        {
            //            savedMapNumber = mapDatas[i].mapNumber;
            //        }
            //        break;
            //    }
            //}
            //SwitchMap(-1, savedMapNumber);
        }
        #endregion

        #region Map State Checking
        public void CheckForMapUpdate()
        {
            int savedMapNumber = SaveSystem.LoadInt(StringUtils.MAP_NUMER, IntUtils.DEFAULT_MAP_NUMBER);
            var levelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            //for (int i = 0; i < mapDatas.Length; i++)
            //{
            //    if (levelNumber >= mapDatas[i].startLevelNumber && levelNumber <= mapDatas[i].endLevelNumber)
            //    {
            //        if (savedMapNumber != mapDatas[i].mapNumber)
            //        {
            //            isNewMapUnlocked = true;
            //        }
            //        MapNumber = mapDatas[i].mapNumber;
            //        break;
            //    }
            //}

        }
        #endregion

        #region Boat Movement
        public void PlaceBoatAtCurrentLevel()
        {
            UnlockNewMap();
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
            //if (mapDatas[mapNumber - 1].IsLevelExists(levelNumber))
            //{
            //    Transform levelVisual = mapDatas[mapNumber - 1].GetLevelVisual(levelNumber).transform;
            //    mapDatas[mapNumber - 1].CalculateOffsetDirection(levelVisual, out Vector3 offsetDir);
            //    boatTransform.SetPositionAndRotation(
            //        levelVisual.position + offsetDir * boatOffsetDistance,
            //        levelVisual.rotation);
            //}
        }

        public void SetBoatObjectActive(bool enable)
        {
            //boatTransform.gameObject.SetActive(enable);
        }

        public void AnimateBoatToCurrentLevel()
        {

            PlaceBoatAtCurrentLevel();

            int currentLevelIndex = GameController.GetInstance.CurrentLevelIndex;
            int previousLevelIndex = currentLevelIndex - 1;
            //previousLevelIndex = mapDatas[mapNumber - 1].GetCurrentLevelIndex(previousLevelIndex + 1);
            //currentLevelIndex = mapDatas[mapNumber - 1].GetCurrentLevelIndex(currentLevelIndex + 1);

            //if (currentLevelIndex < mapDatas[mapNumber - 1].points.Count)
            //{
            //    BezierPoint bp0 = mapDatas[mapNumber - 1].points[previousLevelIndex];
            //    BezierPoint bp1 = mapDatas[mapNumber - 1].points[currentLevelIndex];

            //    Vector3 p0 = bp0.anchorPoint;
            //    Vector3 p1 = p0 + bp0.outTangent;
            //    Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
            //    Vector3 p3 = bp1.anchorPoint;

            //    float time = 0;
            //    boatTweenHandle.Cancel();
            //    boatTweenHandle = TweenManager.SetFloat(time, 1f, boatDuration,
            //        x =>
            //        {
            //            time = x;
            //            Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, time);
            //            Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, time).normalized;
            //            boatTransform.up = Vector3.Lerp(boatTransform.up, forward, Time.deltaTime * 10f);
            //            mapDatas[mapNumber - 1].CalculateOffsetDirectionFromCross(boatTransform.up, out Vector3 boatOffsetDirection);
            //            boatTransform.position = pos + boatOffsetDirection * boatOffsetDistance;
            //        },
            //        boatEase, () => OnShowPowerupSelection?.Invoke());
            //}
        }
        #endregion

        #region Map
        public void UpdatePathLine()
        {
            // var pathLine = mapDatas[mapNumber - 1].pathLine;
            // TweenManager.SetFloat(pathLine.startWidth, ZoomOutThick, ZoomDuration, value => pathLine.startWidth = value);
            //   TweenManager.SetFloat(pathLine.endWidth, ZoomOutThick, ZoomDuration, value => pathLine.endWidth = value);
            // DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, ZoomOutThick, ZoomDuration);
            //  DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, ZoomOutThick, ZoomDuration);
            // DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, DefaultTextureScale, ZoomDuration);
        }

        public string GetMapName(int mapNumber)
        {
            //for (int i = 0; i < mapDatas.Length; i++)
            //{
            //    if (mapDatas[i].mapNumber == mapNumber)
            //    {
            //        return mapDatas[i].name;
            //    }
            //}
            return string.Empty;
        }
        public string GetMapDescription(int mapNumber)
        {
            //for (int i = 0; i < mapDatas.Length; i++)
            //{
            //    if (mapDatas[i].mapNumber == mapNumber)
            //    {
            //        return mapDatas[i].description;
            //    }
            //}
            return string.Empty;
        }
        private void UnlockNewMap()
        {
            OnMapUnlocked?.Invoke();
        }

        public void SwitchMap(int previousMap, int currentMap, bool isPlayAnim = false)
        {
            UpdateMapBGScale();
            int mapMultiplier = currentMap > previousMap ? -1 : 1;
            if (isPlayAnim)
            {
                //Previous Map 
                //if (previousMap > 0)
                //{
                //    mapDatas[previousMap - 1].mapObject.SetActive(false);
                //}
                //var currentMapData = mapDatas[currentMap - 1];
                //currentMapData.mapObject.SetActive(true);
                //currentMapData.LevelSetup(levelDatabase);
            }
            else
            {
                //If startLevelIndex is -1, it means no levels are set for this map
                //if (mapDatas[currentMap - 1].startLevelNumber != -1)
                //{
                //    //PRevious
                //    if (previousMap > 0)
                //    {
                //        mapDatas[previousMap - 1].mapObject.SetActive(false);
                //    }

                //    //Current
                //    mapDatas[currentMap - 1].mapObject.SetActive(true);
                //    mapDatas[currentMap - 1].LevelSetup(levelDatabase);
                //}
            }
        }
        public void UpdateMapBGScale()
        {
            //CameraController.GetInstance.SetOrthoSize(ScreenResolutionUtils.GetOrthographicSize(ReferenceOrthoSize), GameCameraType.Map);
            //if (mapBG != null)
            //{
            //    var scale = ScreenResolutionUtils.GetObjectScale(ReferenceMapScale, ReferenceOrthoSize);
            //    mapBG.localScale = new Vector3(scale.x, scale.y, 1f);
            //}
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
        #endregion


#if UNITY_EDITOR
        public void GenerateLevelVisuals(int _mapIndex, List<BezierPoint> bezierPoints, Vector3[] linePoints)
        {
            //Destroy previous level visuals if any 
            //var levelVisualChilds = mapDatas[_mapIndex].levelsParent.GetComponentsInChildren<LevelVisual>();
            //foreach (var visual in levelVisualChilds)
            //{
            //    Undo.DestroyObjectImmediate(visual.gameObject);
            //}
            //mapDatas[_mapIndex].levelVisuals.Clear();
            //mapDatas[_mapIndex].points = bezierPoints;

            //// Generate new level visuals based on bezier points
            //SetPathLine(_mapIndex, linePoints);
            //for (int i = 0; i < bezierPoints.Count; i++)
            //{
            //    var levelObject = Instantiate(levelPrefab, mapDatas[_mapIndex].levelsParent);
            //    Undo.RegisterCreatedObjectUndo(levelObject, "Create Level Visual");
            //    mapDatas[_mapIndex].levelVisuals.Add(levelObject.GetComponent<LevelVisual>());
            //    mapDatas[_mapIndex].levelVisuals[i].SetPositions(bezierPoints[i].anchorPoint);
            //}

            //// Set up the level visuals
            //for (var i = 0; i < mapDatas[_mapIndex].levelVisuals.Count - 1; i++)
            //{
            //    var levelTransform = mapDatas[_mapIndex].levelVisuals[i].transform;
            //    BezierPoint bp0 = bezierPoints[i];
            //    BezierPoint bp1 = bezierPoints[i + 1];

            //    Vector3 p0 = bp0.anchorPoint;
            //    Vector3 p1 = p0 + bp0.outTangent;
            //    Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
            //    Vector3 p3 = bp1.anchorPoint;

            //    Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, 0.1f).normalized;
            //    levelTransform.up = forward;
            //}
        }
        public void SetPathLine(int _mapIndex, Vector3[] linePoints)
        {
            //mapDatas[_mapIndex].pathLine.positionCount = linePoints.Length;
            //mapDatas[_mapIndex].pathLine.SetPositions(linePoints);
        }
#endif
    }
}
