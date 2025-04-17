using UnityEngine;

namespace Bokka.BeachRescue
{
    public class MovingObstaclePathRenderer : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;

        public void GeneratePath(Vector3[] path)
        {
            lineRenderer.positionCount = path.Length;
            lineRenderer.SetPositions(path);
        }
    }
}