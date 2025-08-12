using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Unity.Cinemachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeachHero
{
    public class MapController : MonoBehaviour
    {
        public static MapController GetInstance { get; private set; }

        #region Readonly Variables 
        private static readonly Vector2 originalTextureScale = new Vector2(1, 1);
        private static readonly Vector2 zoomInTextureScale = new Vector2(3, 1);
        private static readonly float zoomInThick = 0.05f;
        private static readonly float zoomOutThick = 0.35f;
        private static readonly float referenceOrthoSize = 31f;
        private static readonly Vector2 referenceMapScale = new Vector2(4.5f, 4f);
        #endregion

        #region Inspector Variables
        [SerializeField] private Transform boat;
        [SerializeField] private LevelDatabaseSO levelDatabase;
        [SerializeField] private CinemachineCamera zoomOutCam, zoomInCam;
        [SerializeField] private ParticleSystem confettiParticle;

        [Header("Map")]
        [SerializeField] private Transform mapBG;
        [SerializeField] private MapData[] mapDatas;

        [Header("Boat")]
        [SerializeField] private float boatDuration = 1f;
        [SerializeField] private float boatOffset = 0.5f;
        [SerializeField] private Ease boatEase = Ease.OutCubic;
        #endregion

        #region Private Variables
        private Tween boatTween;
        private int mapNumber = 0;
        private bool isNewMapUnlocked = false;
        #endregion

        #region Properties
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
        public event Action OnMapButtonsActive;
        public event Action OnPushPowerupSelectionScreen;
        public event Action OnNewMapUnlockAction;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            if (GetInstance == null)
            {
                GetInstance = this;
            }
            //Parse Map number
            int savedMapNumber = SaveSystem.LoadInt(StringUtils.MAP_NUMER, IntUtils.DEFAULT_MAP_NUMBER);
            var levelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            for (int i = 0; i < mapDatas.Length; i++)
            {
                if (levelNumber >= mapDatas[i].startLevelNumber && levelNumber <= mapDatas[i].endLevelNumber)
                {
                    if(savedMapNumber != mapDatas[i].mapNumber)
                    {
                        isNewMapUnlocked = true;
                    }
                    MapNumber = mapDatas[i].mapNumber;
                    break;
                }
            }
            InitializeMapData();
        }
        private void OnDestroy()
        {
            if (GetInstance == this)
            {
                GetInstance = null;
            }
        }
        #endregion

        #region Boat 
        public void SetBoatInCurrentLevel()
        {
            // Set Boat Position to Current Level
            int currentLevelNumber = GameController.GetInstance.CurrentLevelIndex + 1;
            Transform target = mapDatas[mapNumber - 1].GetLevelVisual(currentLevelNumber).transform;
            mapDatas[mapNumber - 1].CalculateOffsetDirection(target, out Vector3 boatOffsetDirection);
            boat.SetPositionAndRotation(target.position + boatOffsetDirection * boatOffset, target.rotation);
            OnMapButtonsActive?.Invoke();
        }

        public void MoveBoatFromPrevToCurrentLevel()
        {
            if(isNewMapUnlocked)
            {
                NewMapUnlocked();
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
                    () => 0, x =>
                    {
                        time = x;
                        Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, time);
                        Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, time).normalized;
                        boat.up = forward;
                        mapDatas[mapNumber - 1].CalculateOffsetDirectionFromCross(forward, out Vector3 boatOffsetDirection);
                        boat.position = pos + boatOffsetDirection * boatOffset;
                    },
                    1,
                    boatDuration).SetEase(boatEase).OnComplete(() =>
                    {
                        OnPushPowerupSelectionScreen?.Invoke();
                    });
            }
        }
        #endregion

        #region Zoom
        public void ZoomIn()
        {
            Vector2 position = mapDatas[mapNumber - 1].GetLevelVisual(GameController.GetInstance.CurrentLevelIndex + 1).WorldPosition;
            zoomOutCam.gameObject.SetActive(false);
            zoomInCam.gameObject.SetActive(true);
            zoomInCam.transform.position = new Vector3(position.x, position.y, zoomInCam.transform.position.z);
            var pathLine = mapDatas[mapNumber - 1].pathLine;
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, zoomInThick, 1.5f);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, zoomInThick, 1.5f);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, zoomInTextureScale, 1.5f);
        }
        public void ZoomOut()
        {
            UpdateMapBGScale();
            zoomOutCam.gameObject.SetActive(true);
            zoomInCam.gameObject.SetActive(false);
            var pathLine = mapDatas[mapNumber - 1].pathLine;
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, zoomOutThick, 1.5f);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, zoomOutThick, 1.5f);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, originalTextureScale, 1.5f);
        }
        #endregion

        #region Map
        private void NewMapUnlocked()
        {
            SetBoatInCurrentLevel();
            confettiParticle.Play();
            isNewMapUnlocked = false;
            OnNewMapUnlockAction?.Invoke();
        }
        private void InitializeMapData()
        {
            // Initialize Map Data
            foreach (var mapData in mapDatas)
            {
                mapData.mapObject.SetActive(false);
            }
        }
        public void ChangeMapVisual(int previous, int current)
        {
            mapDatas[current - 1].mapObject.SetActive(true);
            if (previous != -1)
            {
                mapDatas[previous - 1].mapObject.SetActive(false);
            }
            mapDatas[current - 1].LevelSetup(levelDatabase);
        }
        private void UpdateMapBGScale()
        {
            zoomOutCam.Lens.OrthographicSize = ScreenResolutionUtils.GetOrthographicSize(referenceOrthoSize);
            if (mapBG != null)
            {
                var scale = ScreenResolutionUtils.GetObjectScale(referenceMapScale, referenceOrthoSize);
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
