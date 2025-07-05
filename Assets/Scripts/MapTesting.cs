using BeachHero;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public struct MapNode
{
    public Vector3 position;
    public GameObject levelObject;
    public GameObject levelCompleteIndicator;
}
[System.Serializable]
public class BezierPoint
{
    public Transform anchor;       // Main control point
    public Vector3 inTangent = Vector3.left;   // Local position
    public Vector3 outTangent = Vector3.right; // Local position
}
public class MapTesting : MonoBehaviour
{
    public MapLevelEditor levelPrefab;
    public LineRenderer unCompleteLineRenderer;
    public LineRenderer completeLineRenderer;
    public Transform bezierPointsParent;
    public Transform levelsParent;
    public Transform boatTransform;
    public List<BezierPoint> bezierPoints = new List<BezierPoint>();
    public List<MapNode> mapNodes = new List<MapNode>();
    public int currentIndex;
    public int nextIndex = 1;
    public int curveSamples = 20;
    public int lineResolutionPerLoop = 10;
    [Tooltip("Total Number of levels")]
    public int pointsPerSegment = 100;
    [Range(0, 1)] public float elapsedTime;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 1)
        {
            elapsedTime = 0;
            currentIndex++;
            nextIndex++;
        }
        BezierPoint bp0 = bezierPoints[currentIndex];
        BezierPoint bp1 = bezierPoints[nextIndex];

        Vector3 p0 = bp0.anchor.position;
        Vector3 p1 = p0 + bp0.outTangent;
        Vector3 p2 = bp1.anchor.position + bp1.inTangent;
        Vector3 p3 = bp1.anchor.position;

        Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, elapsedTime);
        boatTransform.position = pos;
    }

    public void GenerateMapPointsInEditor()
    {
        if (!Application.isPlaying && bezierPoints != null && bezierPoints.Count >= 2)
        {
            ClearGeneratedObjects();
            ReplaceBezierCurveWithTangents();

            // Generate map nodes and line renderer points
            List<Vector3> linePoints = new List<Vector3>();
            for (int i = 0; i < bezierPoints.Count - 1; i++)
            {
                BezierPoint bp0 = bezierPoints[i];
                BezierPoint bp1 = bezierPoints[i + 1];

                //Instantiate level prefab 
                MapLevelEditor level = InstantiateInEditor<MapLevelEditor>(levelPrefab.gameObject, bezierPoints[i].anchor.position, levelPrefab.transform.rotation, levelsParent);
                MapNode mapNode = new MapNode
                {
                    position = bezierPoints[i].anchor.position,
                    levelObject = level.gameObject,
                    levelCompleteIndicator = level.levelCompleteIndicator
                };
                DestroyImmediate(level.GetComponent<MapLevelEditor>());
                mapNodes.Add(mapNode);

                if (bp0.anchor == null || bp1.anchor == null)
                    continue;

                // Calculate points on the Bezier curve
                Vector3 p0 = bp0.anchor.position;
                Vector3 p1 = p0 + bp0.outTangent;
                Vector3 p2 = bp1.anchor.position + bp1.inTangent;
                Vector3 p3 = bp1.anchor.position;

                for (int j = 0; j <= lineResolutionPerLoop; j++)
                {
                    float t = j / (float)lineResolutionPerLoop;
                    Vector3 pointOnCurve = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                    linePoints.Add(pointOnCurve);
                }
            }

            // Instantiate the last level
            if (bezierPoints.Count > 0)
            {
                int index = bezierPoints.Count - 1;
                MapLevelEditor level = InstantiateInEditor<MapLevelEditor>(levelPrefab.gameObject, bezierPoints[index].anchor.position, levelPrefab.transform.rotation, levelsParent);
                MapNode mapNode = new MapNode
                {
                    position = bezierPoints[index].anchor.position,
                    levelObject = level.gameObject,
                    levelCompleteIndicator = level.levelCompleteIndicator
                };
                DestroyImmediate(level.GetComponent<MapLevelEditor>());
                mapNodes.Add(mapNode);
            }

            if (unCompleteLineRenderer != null)
            {
                SetLineRenderer(linePoints.ToArray());
            }
        }
    }

    private void ReplaceBezierCurveWithTangents()
    {
        List<Vector3> sampledPoints;
        List<Vector3> tangentVectors;
        BezierCurveUtils.SampleEvenlySpacedPointsWithTangents(bezierPoints, pointsPerSegment, out sampledPoints, out tangentVectors);

        // Clear existing anchors and create new ones
        foreach (var bp in bezierPoints)
        {
            if (bp.anchor != null)
            {
                DestroyImmediate(bp.anchor.gameObject);
            }
        }
        bezierPoints.Clear();

        // Create new BezierPoints with anchors and tangents
        for (int i = 0; i < sampledPoints.Count; i++)
        {
            GameObject go = new GameObject("Bezier Anchor " + i);
            go.transform.position = sampledPoints[i];
            go.transform.SetParent(bezierPointsParent);

            Vector3 tangent = tangentVectors[i].normalized;
            float handleLength = 0.5f; // You can adapt this based on spacing

            bezierPoints.Add(new BezierPoint
            {
                anchor = go.transform,
                inTangent = -tangent * handleLength,
                outTangent = tangent * handleLength
            });
        }
    }

    #region Line Renderer
    public void SetLineRenderer(Vector3[] points)
    {
        unCompleteLineRenderer.positionCount = points.Length;
        unCompleteLineRenderer.SetPositions(points);
    }
    #endregion

    private T InstantiateInEditor<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parentTransform) where T : MonoBehaviour
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

            if (bp0.anchor == null || bp1.anchor == null)
                continue;

            Vector3 p0 = bp0.anchor.position;
            Vector3 p1 = p0 + bp0.outTangent;
            Vector3 p2 = bp1.anchor.position + bp1.inTangent;
            Vector3 p3 = bp1.anchor.position;

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
        foreach (var node in mapNodes)
        {
            if (node.levelObject != null) DestroyImmediate(node.levelObject);
        }
        if (unCompleteLineRenderer != null)
        {
            unCompleteLineRenderer.positionCount = 0;
            unCompleteLineRenderer.SetPositions(new Vector3[0]);
        }
        if (completeLineRenderer != null)
        {
            completeLineRenderer.positionCount = 0;
            completeLineRenderer.SetPositions(new Vector3[0]);
        }
        mapNodes.Clear();
    }
}
