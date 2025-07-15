using BeachHero;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

[System.Serializable]
public class BezierPoint
{
    public Vector3 anchorPoint; // Main control point
    public Vector3 inTangent = Vector3.zero; // Local position
    public Vector3 outTangent = Vector3.zero; // Local position

    public void ResetZ()
    {
        anchorPoint.z = 0;
        inTangent.z = 0;
        outTangent.z = 0;
    }
}
#if UNITY_EDITOR

public class MapEditor : SingleTon<MapEditor>
{
    public LineRenderer visualLineRenderer;
    public List<BezierPoint> bezierPoints = new List<BezierPoint>();
    public List<Vector3> linePoints = new List<Vector3>();
    public bool validate;

    private void OnValidate()
    {
        if (validate)
        {
            validate = false;
        }
    }

    public int curveSamples = 20;
    public int lineResolutionPerLoop = 10;
    [Tooltip("Total Number of levels")]
    public int pointsPerSegment = 100;

    public void GenerateMapPointsInEditor()
    {
        if (!Application.isPlaying && bezierPoints != null && bezierPoints.Count >= 2)
        {
            ClearGeneratedObjects();
            ReplaceBezierCurveWithTangents();

            // Generate map nodes and line renderer points
            linePoints ??= new List<Vector3>();
            linePoints.Clear();
            for (int i = 0; i < bezierPoints.Count - 1; i++)
            {
                BezierPoint bp0 = bezierPoints[i];
                bp0.ResetZ();
                BezierPoint bp1 = bezierPoints[i + 1];
                bp1.ResetZ();

                if (bp0.anchorPoint == null || bp1.anchorPoint == null)
                    continue;

                // Calculate points on the Bezier curve
                Vector3 p0 = bp0.anchorPoint;
                Vector3 p1 = p0 + bp0.outTangent;
                Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
                Vector3 p3 = bp1.anchorPoint;

                for (int j = 0; j <= lineResolutionPerLoop; j++)
                {
                    float t = j / (float)lineResolutionPerLoop;
                    Vector3 pointOnCurve = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                    linePoints.Add(pointOnCurve);
                }
            }

            if (visualLineRenderer != null)
            {
                SetLineRenderer(linePoints.ToArray());
            }
        }
    }

    public List<Vector3> GenerateBezierCurvePoints(List<BezierPoint> bezierPoints)
    {
        linePoints ??= new List<Vector3>();
        linePoints.Clear();
        for (int i = 0; i < bezierPoints.Count - 1; i++)
        {
            BezierPoint bp0 = bezierPoints[i];
            BezierPoint bp1 = bezierPoints[i + 1];

            Vector3 p0 = bp0.anchorPoint;
            Vector3 p1 = p0 + bp0.outTangent;
            Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
            Vector3 p3 = bp1.anchorPoint;

            for (int j = 0; j <= lineResolutionPerLoop; j++)
            {
                float t = j / (float)lineResolutionPerLoop;
                Vector3 pointOnCurve = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                linePoints.Add(pointOnCurve);
            }
        }
        return linePoints;
    }

    private void ReplaceBezierCurveWithTangents()
    {
        List<Vector3> sampledPoints;
        List<Vector3> tangentVectors;
        BezierCurveUtils.SampleEvenlySpacedPointsWithTangents(bezierPoints, pointsPerSegment, out sampledPoints, out tangentVectors);

        bezierPoints.Clear();

        // Create new BezierPoints with anchors and tangents
        for (int i = 0; i < sampledPoints.Count; i++)
        {
            // GameObject go = new GameObject("Bezier Anchor " + i);
            // go.transform.position = sampledPoints[i];
            // go.transform.SetParent(bezierPointsParent);

            Vector3 tangent = tangentVectors[i].normalized;
            float handleLength = 0.5f; // You can adapt this based on spacing

            bezierPoints.Add(new BezierPoint
            {
                anchorPoint = sampledPoints[i],
                inTangent = -tangent * handleLength,
                outTangent = tangent * handleLength
            });
        }
    }

    #region Line Renderer
    public void SetLineRenderer(Vector3[] points)
    {
        visualLineRenderer.positionCount = points.Length;
        visualLineRenderer.SetPositions(points);
    }
    #endregion

    private T InstantiateInEditor<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parentTransform) where T : Object
    {
        if (prefab == null) return null;

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parentTransform);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(parentTransform);

        return obj.GetComponent<T>();
    }

    private void OnDrawGizmos()
    {
        if (bezierPoints == null || bezierPoints.Count < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < bezierPoints.Count - 1; i++)
        {
            BezierPoint bp0 = bezierPoints[i];
            BezierPoint bp1 = bezierPoints[i + 1];

            if (bp0.anchorPoint == null || bp1.anchorPoint == null)
                continue;

            Vector3 p0 = bp0.anchorPoint;
            Vector3 p1 = p0 + bp0.outTangent;
            Vector3 p2 = bp1.anchorPoint + bp1.inTangent;
            Vector3 p3 = bp1.anchorPoint;

            Vector3 prev = p0;

            for (int j = 1; j <= curveSamples; j++)
            {
                float t = j / (float)curveSamples;
                Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                Gizmos.DrawLine(prev, pos);
                prev = pos;
            }
        }
    }

    public void ClearGeneratedObjects()
    {
        if (visualLineRenderer != null)
        {
            visualLineRenderer.positionCount = 0;
            visualLineRenderer.SetPositions(new Vector3[0]);
        }
    }
}
#endif

