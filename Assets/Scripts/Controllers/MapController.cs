using UnityEngine;

namespace BeachHero
{
    public class MapCameraController : MonoBehaviour
    {
        private static readonly Vector2 originalTextureScale = new Vector2(0.35f,1);
        private static readonly Vector2 zoomInTextureScale = new Vector2(4,1);

        private static readonly float zoomInThick = 0.05f;
        private static readonly float zoomOutThick = 1f;
        [SerializeField] private LineRenderer pathLine;
        
    }
}
