using QFSW.QC.Utilities;
using UnityEngine;

namespace BeachHero
{
    public class PathTrail : MonoBehaviour
    {
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private TrailRenderer transparentRenderer;
        private Vector3[] cachedPositions;
        //[SerializeField] private LineRenderer lineRenderer;
        //public void SetLineRenderer(Vector3[] curvePoints)
        //{
        //    //Lift the first point to fix the line is visible above the water
        //    curvePoints[0] = new Vector3(curvePoints[0].x, 0.6f, curvePoints[0].z); // Ensure the first point is on the ground
        //    lineRenderer.positionCount = curvePoints.Length;
        //    lineRenderer.SetPositions(curvePoints);
        //}
        public void InitializeTrail(Vector3[] positions,float speed)
        {
            trailRenderer.Clear();
            transparentRenderer.Clear();
            cachedPositions = positions.SubArray(0, positions.Length - 3);
            trailRenderer.AddPositions(cachedPositions);
            transparentRenderer.AddPositions(cachedPositions);
            SetTrailSpeed(speed);
        }
        public void TrimTrailFromStart()
        {
            int count = trailRenderer.positionCount;

            if (trailRenderer.positionCount > 0)
            {
                // Cache positions to avoid unnecessary allocations
                if (cachedPositions == null || cachedPositions.Length < count)
                {
                    cachedPositions = new Vector3[count];
                }

                // Shift left manually 
                trailRenderer.GetPositions(cachedPositions);
                for (int i = 1; i < count; i++)
                {
                    int index = i;
                    cachedPositions[index - 1] = cachedPositions[index];
                }
                trailRenderer.Clear();
                for (int i = 0; i < count - 1; i++)
                {
                    int index = i;
                    trailRenderer.AddPosition(cachedPositions[index]);
                }
            }
        }
        public void SetTrailSpeed(float speed)
        {
            trailRenderer.material.SetVector(Shader.PropertyToID($"{StringUtils.TRAIL_SPEED}"), new Vector2(speed, 0));
            transparentRenderer.material.SetVector(Shader.PropertyToID($"{StringUtils.TRAIL_SPEED}"), new Vector2(speed, 0));
        }
        public void ResetTrail(Vector3 position)
        {
            transform.position = position;
            cachedPositions = null;
            ClearRenderer();
            SetTrailSpeed(0f);
        }
        public void ClearRenderer()
        {
            trailRenderer.Clear();
            transparentRenderer.Clear();
            //lineRenderer.positionCount = 0;
            //lineRenderer.SetPositions(new Vector3[0]);
        }
    }
}
