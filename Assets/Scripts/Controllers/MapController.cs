using System;
using UnityEngine;
using DG.Tweening;

namespace BeachHero
{
    public class MapController : MonoBehaviour
    {
        public static MapController Instance;
        private static readonly Vector2 originalTextureScale = new Vector2(0.35f, 1);
        private static readonly Vector2 zoomInTextureScale = new Vector2(4, 1);

        private static readonly float zoomInThick = 0.05f;
        private static readonly float zoomOutThick = 0.35f;
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private GameObject zoomOutCam, zoomInCam;
        [SerializeField] private InputManager inputManager;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            inputManager.OnEscapePressed += ZoomOut;
            DOTween.Init();
        }

        private void OnDisable()
        {
            inputManager.OnEscapePressed -= ZoomOut;
        }

        public void ZoomIn(Vector2 position)
        {
            zoomOutCam.SetActive(false);
            zoomInCam.transform.position = position;
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
