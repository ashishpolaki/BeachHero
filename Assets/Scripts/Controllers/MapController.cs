using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace BeachHero
{
    public class MapController : SingleTon<MapController>
    {
        private static readonly Vector2 originalTextureScale = new Vector2(1, 1);
        private static readonly Vector2 zoomInTextureScale = new Vector2(3, 1);

        private static readonly float zoomInThick = 0.05f;
        private static readonly float zoomOutThick = 0.35f;


        public bool validate;
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private GameObject zoomOutCam, zoomInCam;
        [SerializeField] private InputManager inputManager;

        Vector3 originalCameraPosition;



        [SerializeField] private Toggle zoomToggle;


        public LevelDatabaseSO levelDatabase;
        public GameObject levelPrefab; // A prefab with SpriteRenderer + optional 3D stuff


        public List<BezierPoint> bezierPoints = new List<BezierPoint>();

        public Transform levelsParent;
        public List<LevelVisual> levelVisuals;
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
                var points = mapEditor.GenerateBezierCurvePoints(bezierPoints);
                pathLine.positionCount = points.Count;
                pathLine.SetPositions(points.ToArray());
            }
            for (var i = 0; i < levelVisuals.Count; i++)
            {
                var level = new LevelData();
                level.LevelNumber = i + 1;
                level.WorldPosition = levelVisuals[i].transform.position;

                levelDatabase.LevelDatas.Add(level);
            }
        }

        private void OnDrawGizmos()
        {
        }

        private void Start()
        {
            for (var i = 0; i < levelVisuals.Count; i++)
            {
                levelVisuals[i].Setup(levelDatabase.LevelDatas[i]);
            }
            inputManager.OnEscapePressed += ZoomOut;
            DOTween.Init();
            ZoomOut();
            zoomToggle.onValueChanged.AddListener(ZoomToggle);
            zoomToggle.isOn = true;
        }

 

        private void ZoomToggle(bool value)
        {
            if (value)
            {
                ZoomOut();
            }
            else
            {
                ZoomIn(levelVisuals.Find(x => x.IsCurrentLevel).WorldPosition);
            }
        }

        private void OnDisable()
        {
            inputManager.OnEscapePressed -= ZoomOut;
        }

        public void ZoomIn(Vector2 position)
        {
            zoomOutCam.SetActive(false);
            zoomInCam.SetActive(true);
            zoomInCam.transform.position = new Vector3(position.x, position.y, zoomInCam.transform.position.z);
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, zoomInThick, 1.5f);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, zoomInThick, 1.5f);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, zoomInTextureScale, 1.5f);
        }

        private void ZoomOut()
        {
            zoomOutCam.SetActive(true);
            zoomInCam.SetActive(false);
            DOTween.To(() => pathLine.startWidth, (x) => pathLine.startWidth = x, zoomOutThick, 1.5f);
            DOTween.To(() => pathLine.endWidth, (x) => pathLine.endWidth = x, zoomOutThick, 1.5f);
            DOTween.To(() => pathLine.textureScale, (x) => pathLine.textureScale = x, originalTextureScale, 1.5f);
        }
    }
}
