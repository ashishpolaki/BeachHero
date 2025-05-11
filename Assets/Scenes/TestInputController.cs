using Bokka.BeachRescue;
using UnityEngine;
using BeachHero;
using Bokka;
using System.Collections.Generic;

public class TestInputController : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private TrailBehaviour pathTrailBehaviour;
    [SerializeField] private Camera cam;
    [SerializeField] private LineRenderer LineRenderer;
    private bool mouseClicked = false;
    [SerializeField] LayerMask startPointLayer;
    [SerializeField] LayerMask touchLayer;
    private Vector3 lastStartPointPosition;
    private Vector3 lastTrailPoint;
    [SerializeField] float minTrailPointsDistance = 0.3f;

    private void OnEnable()
    {
        inputManager.OnMouseClickDown += OnMouseClickDown;
        inputManager.OnMouseClickUp += OnMouseClickUp;
    }

    private void OnDisable()
    {
        inputManager.OnMouseClickDown -= OnMouseClickDown;
        inputManager.OnMouseClickUp -= OnMouseClickUp;
    }

    private void Update()
    {
        if (mouseClicked)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(InputManager.MousePosition);
            if (Physics.Raycast(ray, out hit, 1000f, touchLayer))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    UpdatePath(hit.point);
                }
                else
                {
                    UpdatePath(hit.point.SetY(0f));
                }
            }
        }
    }
    //private void UpdatePath(Vector3 newPosition)
    //{
    //    if (Vector3.Distance(newPosition, lastTrailPoint) > minTrailPointsDistance)
    //    {
    //        pathTrailBehaviour.Transform.position = Vector3.Lerp(pathTrailBehaviour.Transform.position, newPosition, 0.5f);
    //        lastTrailPoint = newPosition;
    //    }
    //}
    private List<Vector3> pathPoints = new List<Vector3>();
    private List<Vector3> smoothedPath = new List<Vector3>();

    private void UpdatePath(Vector3 newPosition)
    {
        if (Vector3.Distance(newPosition, lastTrailPoint) > minTrailPointsDistance)
        {
            // Add the new position to the path points
            pathPoints.Add(newPosition);

            // Generate a smooth curve using Catmull-Rom splines
            if (pathPoints.Count >= 4) // Need at least 4 points for Catmull-Rom
            {
                for (float t = 0; t <= 1; t += 0.05f) // Adjust step size for smoother curves
                {
                    Vector3 interpolatedPoint = CatmullRom(
                        pathPoints[pathPoints.Count - 4], // P0
                        pathPoints[pathPoints.Count - 3], // P1
                        pathPoints[pathPoints.Count - 2], // P2
                        pathPoints[pathPoints.Count - 1], // P3
                        t
                    );

                    // Update the trail position to the interpolated point
                    pathTrailBehaviour.Transform.position = interpolatedPoint;
                }
            }

            // Update the last trail point
            lastTrailPoint = newPosition;
        }
    }

    // Catmull-Rom spline calculation
    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Catmull-Rom spline formula
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
    private void OnMouseClickDown(Vector2 position)
    {
        mouseClicked = true;
        Ray ray = cam.ScreenPointToRay(InputManager.MousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, startPointLayer))
        {
            Debug.Log($"Hit: {hit.collider.name}");
            hit.collider.GetComponent<BeachHero.StartPointBehaviour>();
            lastStartPointPosition = hit.collider.gameObject.transform.position;
        }
    }
    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //foreach (var point in pathPoints)
        //{
        //    Gizmos.DrawSphere(point, 0.1f);
        //}
        Gizmos.color = Color.blue;
        Debug.Log($"Smoothed Path Count: {smoothedPath.Count}");
        foreach (var item in smoothedPath)
        {
            Gizmos.DrawSphere(item, 0.1f);
        }
    }
    [SerializeField] private float smoothingStep = 0.05f;
    [SerializeField] private float spacing = 0.5f; // Adjust the spacing as needed
    private void OnValidate()
    {
        OnMouseClickUp(Vector2.zero);
    }

    private void OnMouseClickUp(Vector2 position)
    {
        mouseClicked = false;
        // OnPathDrawn();
        //if (pathPoints.Count >= 4)
        //{
        //     smoothedPath = new List<Vector3>();

        //    // Recalculate the entire path with higher resolution
        //    for (int i = 0; i < pathPoints.Count - 3; i++)
        //    {
        //        for (float t = 0; t <= 1; t += 0.05f) // Higher resolution for smoother curves
        //        {
        //            Vector3 interpolatedPoint = CatmullRom(
        //                pathPoints[i],
        //                pathPoints[i + 1],
        //                pathPoints[i + 2],
        //                pathPoints[i + 3],
        //                t
        //            );

        //            smoothedPath.Add(interpolatedPoint);
        //        }
        //    }
        // //   pathTrailBehaviour.Reset(lastStartPointPosition);
        //    Debug.Log($"Smoothed Path Count OnMouseClickUp: {smoothedPath.Count}");

        //    // Optionally, update the pathTrailBehaviour to follow the smoothed path
        //    foreach (var point in smoothedPath)
        //    {
        //      //  pathTrailBehaviour.Transform.position = point;
        //    }
        //    LineRenderer.positionCount = smoothedPath.Count;
        //    LineRenderer.SetPositions(smoothedPath.ToArray());
        //}

        if (pathPoints.Count >= 4)
        {
            smoothedPath = GetEvenlySpacedPoints(pathPoints, spacing);

            // Reset the path trail to the last start point position
            // pathTrailBehaviour.Reset(lastStartPointPosition);
            Debug.Log($"Evenly Spaced Path Count: {smoothedPath.Count}");
            for (int i = 0; i < smoothedPath.Count; i++)
            {
                // Update the path trail to follow the evenly spaced path
                // pathTrailBehaviour.Transform.position = smoothedPath[i];
            }
            // Update the LineRenderer to visualize the evenly spaced path
            //  LineRenderer.positionCount = smoothedPath.Count;
            // LineRenderer.SetPositions(smoothedPath.ToArray());
        }
    }

    private List<Vector3> GetEvenlySpacedPoints(List<Vector3> pathPoints, float spacing)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        float distanceSinceLastPoint = 0f;

        evenlySpacedPoints.Add(pathPoints[0]); // Start with the first point

        for (int i = 0; i < pathPoints.Count - 3; i++)
        {
            Vector3 previousPoint = pathPoints[i + 1]; // Start from the second control point

            for (float t = 0; t <= 1; t += 0.01f) // High resolution for accurate arc length calculation
            {
                Vector3 interpolatedPoint = CatmullRom(
                    pathPoints[i],
                    pathPoints[i + 1],
                    pathPoints[i + 2],
                    pathPoints[i + 3],
                    t
                );

                // Accumulate distance between the previous point and the current interpolated point
                distanceSinceLastPoint += Vector3.Distance(previousPoint, interpolatedPoint);

                // If the accumulated distance exceeds the spacing, add a new point
                if (distanceSinceLastPoint >= spacing)
                {
                    evenlySpacedPoints.Add(interpolatedPoint);
                    distanceSinceLastPoint = 0f; // Reset the distance counter
                }

                previousPoint = interpolatedPoint; // Update the previous point
            }
        }

        if (evenlySpacedPoints.Contains(pathPoints[pathPoints.Count - 1]) == false)
            evenlySpacedPoints.Add(pathPoints[pathPoints.Count - 1]); // Add the last point if not already added

        return evenlySpacedPoints;
    }

}
