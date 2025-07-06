using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
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




        public LevelDatabaseSO levelDatabase;
        public GameObject levelPrefab; // A prefab with SpriteRenderer + optional 3D stuff


        public List<BezierPoint> bezierPoints = new List<BezierPoint>();

        public Transform levelsParent;
        public List<LevelVisual> levelVisuals;
        public MapEditor mapEditor;

        [Header("UI")]
        [SerializeField] private Toggle zoomToggle;
        [SerializeField] private Button mapExitBtn;

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
            mapExitBtn.ButtonRegister(MapExit);
        }

        private void MapExit()
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(StringUtils.GAME_SCENE));
            SceneManager.UnloadSceneAsync(StringUtils.MAP_SCENE);
        }

        public Transform boat;

        public bool move;

        public void Update()
        {
            if (move)
            {
                move = false;
                MoveBoatFromCurrentLevelToNextLevel();
            }
        }

        public void MoveBoatFromCurrentLevelToNextLevel()
        {
            var currentLevel = levelVisuals.Find(x => x.IsCurrentLevel).LevelNumber - 1;
            var nextLevel = currentLevel + 1;
            if (nextLevel != null)
            {
                BezierPoint bp0 = bezierPoints[currentLevel];
                BezierPoint bp1 = bezierPoints[nextLevel];

                Vector3 p0 = bp0.anchorPoint;
                Vector3 p1 = p0 + bp0.outTangent;
                Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
                Vector3 p3 = bp1.anchorPoint;

                float time = 0;
                DOTween.To(() => time,
                    x =>
                        {
                            time = x;
                            Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, time);
                            Vector3 forward = BezierCurveUtils.GetTangent(p0, p1, p2, p3, time).normalized;
                            boat.up = forward;
                            boat.position = pos;
                        },
                    1,
                    2f).SetEase(Ease.Linear);
            }
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
