using UnityEngine;

namespace BeachHero
{
    public class PathTrail : MonoBehaviour
    {
        [SerializeField] private TrailRenderer trailRenderer;
        //[SerializeField] private LineRenderer lineRenderer;
        //public void SetLineRenderer(Vector3[] curvePoints)
        //{
        //    //Lift the first point to fix the line is visible above the water
        //    curvePoints[0] = new Vector3(curvePoints[0].x, 0.6f, curvePoints[0].z); // Ensure the first point is on the ground
        //    lineRenderer.positionCount = curvePoints.Length;
        //    lineRenderer.SetPositions(curvePoints);
        //}

        public void ResetTrail(Vector3 position)
        {
            transform.position = position;
            ClearRenderer();
        }
        public void SetTrailSpeed(float speed)
        {
            trailRenderer.material.SetVector(Shader.PropertyToID($"{StringUtils.TRAIL_SPEED}"), new Vector2(speed, 0));
        }
        public void ClearRenderer()
        {
            trailRenderer.Clear();
            SetTrailSpeed(0f);
            //lineRenderer.positionCount = 0;
            //lineRenderer.SetPositions(new Vector3[0]);
        }
    }
}
