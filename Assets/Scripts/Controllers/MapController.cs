using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace BeachHero
{
    public class MapController : MonoBehaviour
    {
        public static MapController GetInstance { get; private set; }

        private static readonly Vector2 originalTextureScale = new Vector2(1, 1);
        private static readonly Vector2 zoomInTextureScale = new Vector2(3, 1);

        private static readonly float zoomInThick = 0.05f;
        private static readonly float zoomOutThick = 0.35f;

        [SerializeField] private Transform boat;
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private GameObject zoomOutCam, zoomInCam;
        [SerializeField] private GameObject levelPrefab; // A prefab with SpriteRenderer + optional 3D stuff
        [SerializeField] private Transform levelsParent;

        [SerializeField] private LevelDatabaseSO levelDatabase;
        [SerializeField] private List<BezierPoint> bezierPoints = new List<BezierPoint>();
        [SerializeField] private List<LevelVisual> levelVisuals;
        [SerializeField] private float boatDuration = 1f;
        [SerializeField] private float boatOffset = 0.5f;

        private Tween boatTween;
        private Vector2 pendingClickPosition;
        private bool shouldCheckClick;

#if UNITY_EDITOR
        public bool validate;
        public MapEditor mapEditor;
        private void OnValidate()
        {
            if (!validate)
            {
                return;
            }
            validate = false;
            if (levelPrefab == null)
            {
                return;
            }
            if (mapEditor)
            {
                // var points = mapEditor.GenerateBezierCurvePoints(bezierPoints);
                // pathLine.positionCount = points.Count;
                // pathLine.SetPositions(points.ToArray());
                //
                // for (int i = 0; i < bezierPoints.Count; i++)
                // {
                //     levelVisuals[i].SetPositions(bezierPoints[i].anchorPoint);
                // }
            }
            // for (var i = 0; i < levelVisuals.Count - 1; i++)
            // {
            //     var tr = levelVisuals[i].transform;
            //     BezierPoint bp0 = bezierPoints[i];
            //     BezierPoint bp1 = bezierPoints[i + 1];
            //
            //     Vector3 p0 = bp0.anchorPoint;
            //     Vector3 p1 = p0 + bp0.outTangent;
            //     Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
            //     Vector3 p3 = bp1.anchorPoint;
            //
            //     Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, 0.1f).normalized;
            //     tr.up = forward;
            // }
        }
#endif

        #region Unity Methods
        private void Awake()
        {
            if (GetInstance == null)
            {
                GetInstance = this;
            }
            for (var i = 0; i < levelVisuals.Count; i++)
            {
                levelVisuals[i].Setup(levelDatabase.LevelDatas[i]);
            }
            DOTween.Init();
            ZoomIn();
        }
        private void OnEnable()
        {
            if (InputManager.GetInstance != null)
            {
                InputManager.GetInstance.OnEscapePressed += ZoomOut;
                InputManager.GetInstance.OnMouseClickDown += HandleClick;
            }
        }
        private void OnDisable()
        {
            if (InputManager.GetInstance != null)
            {
                InputManager.GetInstance.OnEscapePressed -= ZoomOut;
                InputManager.GetInstance.OnMouseClickDown -= HandleClick;
            }
            if (boatTween != null && boatTween.IsActive())
            {
                boatTween.Kill();
            }
        }
        private void OnDestroy()
        {
            if (GetInstance == this)
            {
                GetInstance = null;
            }
        }
        private void Update()
        {
            if (shouldCheckClick)
            {
                shouldCheckClick = false;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    // Click was on UI
                    return;
                }

                Vector2 mousePos = Camera.main.ScreenToWorldPoint(pendingClickPosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f);

                if (hit.collider != null && hit.collider.TryGetComponent<LevelVisual>(out var levelVisual))
                {
                    UIController.GetInstance.ScreenEvent(ScreenType.PowerupSelection, UIScreenEvent.Push);
                }
            }
        }
        #endregion

        #region Public Methods
        public void SetBoatInCurrentLevel()
        {
            // Set Boat Position to Current Level
            int currentLevelIndex = levelVisuals.Find(x => x.IsCurrentLevel).LevelNumber - 1;
            Transform target = levelVisuals[currentLevelIndex].transform;
            boat.SetPositionAndRotation(levelVisuals[currentLevelIndex].transform.position + target.right * boatOffset, target.rotation);
        }
        public void MoveBoatFromPrevToCurrentLevel()
        {
            int currentLevelIndex = SaveSystem.LoadInt(StringUtils.LEVELNUMBER, IntUtils.DEFAULT_LEVEL) - 1;
            int previousLevelIndex = currentLevelIndex - 1;

            if (currentLevelIndex < bezierPoints.Count)
            {
                BezierPoint bp0 = bezierPoints[previousLevelIndex];
                BezierPoint bp1 = bezierPoints[currentLevelIndex];

                Vector3 p0 = bp0.anchorPoint;
                Vector3 p1 = p0 + bp0.outTangent;
                Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
                Vector3 p3 = bp1.anchorPoint;

                float time = 0;
                boatTween = DOTween.To(
                    () => time, x =>
                    {
                        time = x;
                        Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, time);
                        Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, time).normalized;
                        boat.up = forward;
                        Vector3 right = Vector3.Cross(forward, Vector3.forward).normalized;
                        boat.position = pos + right * boatOffset;
                    },
                    1,
                    boatDuration).SetEase(Ease.InQuad);
            }
        }
        public void ZoomIn()
        {
            Vector2 position = levelVisuals.Find(x => x.IsCurrentLevel).WorldPosition;
            zoomOutCam.SetActive(false);
            zoomInCam.SetActive(true);
            zoomInCam.transform.position = new Vector3(position.x, position.y, zoomInCam.transform.position.z);
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, zoomInThick, 1.5f);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, zoomInThick, 1.5f);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, zoomInTextureScale, 1.5f);
        }
        public void ZoomOut()
        {
            zoomOutCam.SetActive(true);
            zoomInCam.SetActive(false);
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, zoomOutThick, 1.5f);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, zoomOutThick, 1.5f);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, originalTextureScale, 1.5f);
        }
        #endregion

        private void HandleClick(Vector2 screenPosition)
        {
            pendingClickPosition = screenPosition;
            shouldCheckClick = true;
        }
    }
}
