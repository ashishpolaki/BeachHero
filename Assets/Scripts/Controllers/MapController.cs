using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace BeachHero
{
    public class MapController : MonoBehaviour
    {
        public static MapController Instance;
        private static readonly Vector2 originalTextureScale = new Vector2(1, 1);
        private static readonly Vector2 zoomInTextureScale = new Vector2(4, 1);

        private static readonly float zoomInThick = 0.05f;
        private static readonly float zoomOutThick = 0.35f;
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private GameObject zoomOutCam, zoomInCam;
        [SerializeField] private InputManager inputManager;

        Vector3 originalCameraPosition;
        [SerializeField] private Transform levelPointPrefab;
        public LevelDatabaseSO levelDatabase;
        public GameObject levelPrefab; // A prefab with SpriteRenderer + optional 3D stuff

        public List<LevelVisual> levelVisuals;


        private void OnValidate()
        {
            if (levelPointPrefab == null)
            {
                return;
            }
            int pointCount = pathLine.positionCount;
            Vector3[] positions = new Vector3[pointCount];
            pathLine.GetPositions(positions);

            var points = CatmullSplineUtils.GetEvenlySpacedPoints(positions.ToList(), 0.4f);
        }

        private void OnDrawGizmos()
        {
            SetupLevelVisuals();
        }

        private void SetupLevelVisuals()
        {
            int pointCount = pathLine.positionCount;
            Vector3[] positions = new Vector3[pointCount];
            pathLine.GetPositions(positions);
            List<Vector3> points = CatmullSplineUtils.GetEvenlySpacedPointsByCount(positions.ToList(), 100);
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                point.z -= 0.5f;
                points[i] = point;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(points[i], 0.1f);
            }
            // if (levelVisuals == null || levelVisuals.Count == 0)
            // {
            //     levelVisuals = new List<LevelVisual>();
            //     var leveldatas = levelDatabase.LevelDatas;
            //     for (int i = 0; i < points.Count && i < leveldatas.Count; i++)
            //     {
            //         leveldatas[i].WorldPosition = points[i];
            //         GameObject levelObj = Instantiate(levelPrefab, points[i], Quaternion.identity, transform);
            //         LevelVisual controller = levelObj.GetComponent<LevelVisual>();
            //         controller.Setup(leveldatas[i]);
            //         levelVisuals.Add(controller);
            //     }
            // }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            inputManager.OnEscapePressed += ZoomOut;
            DOTween.Init();
            ZoomOut();
        }

        private void OnDisable()
        {
            inputManager.OnEscapePressed -= ZoomOut;
        }

        public void ZoomIn(Vector2 position)
        {
            zoomOutCam.SetActive(false);
            zoomInCam.transform.position = new Vector3(position.x, position.y, zoomInCam.transform.position.z);
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, zoomInThick, 1.5f);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, zoomInThick, 1.5f);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, zoomInTextureScale, 1.5f);
        }

        private void ZoomOut()
        {
            zoomOutCam.SetActive(true);
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, zoomOutThick, 1.5f);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, zoomOutThick, 1.5f);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, originalTextureScale, 1.5f);
        }
    }
}
