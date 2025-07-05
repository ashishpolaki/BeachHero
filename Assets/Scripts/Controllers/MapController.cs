using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace BeachHero
{
    public class MapController : SingleTon<MapController>
    {
        private static readonly Vector2 originalTextureScale = new Vector2(1, 1);
        private static readonly Vector2 zoomInTextureScale = new Vector2(4, 1);

        private static readonly float zoomInThick = 0.05f;
        private static readonly float zoomOutThick = 0.35f;


        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private GameObject zoomOutCam, zoomInCam;
        [SerializeField] private InputManager inputManager;

        Vector3 originalCameraPosition;
        public LevelDatabaseSO levelDatabase;
        public GameObject levelPrefab; // A prefab with SpriteRenderer + optional 3D stuff


        public List<BezierPoint> bezierPoints = new List<BezierPoint>();

        public Transform levelsParent;
        public List<LevelVisual> levelVisuals;
        public MapEditor mapEditor;


        private void OnValidate()
        {
            if (levelPrefab == null)
            {
                return;
            }
            if (mapEditor)
            {
                pathLine.SetPositions(mapEditor.GenerateBezierCurvePoints(bezierPoints).ToArray());
            }
        }

        private void OnDrawGizmos()
        {
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
