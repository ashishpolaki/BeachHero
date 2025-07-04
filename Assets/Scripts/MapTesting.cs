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
    public List<BezierPoint> bezierPoints = new List<BezierPoint>();
    public List<Vector3> evenlySpacedPoints = new List<Vector3>();
    public List<MapNode> mapNodes = new List<MapNode>();

    [Range(0f, 1f)] public float dashOffset1 = 0.33f;
    [Range(0f, 1f)] public float dashOffset2 = 0.66f;
    public int curveSamples = 20;
    public int lineResolutionPerLoop = 10;
    [Tooltip("Total Number of levels")]
    public int pointsPerSegment = 100;

#if UNITY_EDITOR
    //[Header("Validate")]
    //public bool isValidate;
    //[Range(0, 1)] public float elapsedTime;
    //public int index1;
    //public int index2 = 1;
    //public Transform spawnObject;

    //private void OnValidate()
    //{
    //    if (isValidate)
    //    {
    //        BezierPoint bp0 = bezierPoints[index1];
    //        BezierPoint bp1 = bezierPoints[index2];

    //        Vector3 p0 = bp0.anchor.position;
    //        Vector3 p1 = p0 + bp0.outTangent;
    //        Vector3 p2 = bp1.anchor.position + bp1.inTangent;
    //        Vector3 p3 = bp1.anchor.position;

    //        Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, elapsedTime);
    //        spawnObject.position = pos;
    //    }
    //}
    public Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float oneMinusT = 1f - t;

        return oneMinusT * oneMinusT * oneMinusT * p0 +
               3f * oneMinusT * oneMinusT * t * p1 +
               3f * oneMinusT * t * t * p2 +
               t * t * t * p3;
    }

    public void GenerateMapPointsInEditor()
    {
        if (!Application.isPlaying && bezierPoints != null && bezierPoints.Count >= 2)
        {
            ClearGeneratedObjects();
            evenlySpacedPoints = GenerateGlobalEvenlySpacedPoints(bezierPoints, pointsPerSegment);
            List<Vector3> linePoints = new List<Vector3>();

            // Instantiate levelPrefab at each evenly spaced point
            foreach (Vector3 point in evenlySpacedPoints)
            {
                MapLevelEditor level = InstantiateInEditor<MapLevelEditor>(levelPrefab.gameObject, point, levelPrefab.transform.rotation);

                MapNode mapNode = new MapNode
                {
                    position = point,
                    levelObject = level.gameObject,
                    levelCompleteIndicator = level.levelCompleteIndicator
                };

                DestroyImmediate(level.GetComponent<MapLevelEditor>());
                mapNodes.Add(mapNode);
            }

            // Still populate the lineRenderer using the Bezier curve (not evenly spaced)
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

                for (int j = 0; j <= lineResolutionPerLoop; j++)
                {
                    float t = j / (float)lineResolutionPerLoop;
                    Vector3 pointOnCurve = GetPoint(p0, p1, p2, p3, t);
                    linePoints.Add(pointOnCurve);
                }
            }

            if (unCompleteLineRenderer != null)
            {
                SetLineRenderer(linePoints.ToArray());
            }
        }
    }

    public void SetLineRenderer(Vector3[] points)
    {
        unCompleteLineRenderer.positionCount = points.Length;
        unCompleteLineRenderer.SetPositions(points);
    }
    public List<Vector3> GenerateGlobalEvenlySpacedPoints(List<BezierPoint> bezierPoints, int totalPoints)
    {
        List<Vector3> allPoints = new List<Vector3>();
        List<float> arcLengths = new List<float>();

        // Step 1: Sample high-res points from each segment
        List<Vector3> denseSamples = new List<Vector3>();
        float totalLength = 0f;

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

            var segment = GetEvenlySpacedPoints(p0, p1, p2, p3, 200); // High-res per segment

            for (int j = 0; j < segment.Count; j++)
            {
                Vector3 pt = segment[j];
                if (denseSamples.Count > 0)
                    totalLength += Vector3.Distance(denseSamples[denseSamples.Count - 1], pt);

                denseSamples.Add(pt);
                arcLengths.Add(totalLength);
            }
        }


        // Step 2: Resample to 100 evenly spaced points along total length
        float step = totalLength / (totalPoints - 1);
        float distTarget = 0;
        int sampleIndex = 0;

        for (int i = 0; i < totalPoints; i++)
        {
            while (sampleIndex < arcLengths.Count - 1 && arcLengths[sampleIndex] < distTarget)
                sampleIndex++;

            if (sampleIndex == 0 || sampleIndex >= denseSamples.Count - 1)
            {
                allPoints.Add(denseSamples[sampleIndex]);
            }
            else
            {
                float d0 = arcLengths[sampleIndex - 1];
                float d1 = arcLengths[sampleIndex];
                float lerpT = Mathf.InverseLerp(d0, d1, distTarget);
                Vector3 pt = Vector3.Lerp(denseSamples[sampleIndex - 1], denseSamples[sampleIndex], lerpT);
                allPoints.Add(pt);
            }

            distTarget += step;
        }

        return allPoints;
    }
    public List<Vector3> GetEvenlySpacedPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int count)
    {
        List<Vector3> sampled = new List<Vector3>();
        List<float> cumulative = new List<float>();
        int resolution = 1000;
        float totalDist = 0;
        Vector3 prev = GetPoint(p0, p1, p2, p3, 0);

        sampled.Add(prev);
        cumulative.Add(0);

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 pt = GetPoint(p0, p1, p2, p3, t);
            totalDist += Vector3.Distance(prev, pt);
            sampled.Add(pt);
            cumulative.Add(totalDist);
            prev = pt;
        }

        List<Vector3> even = new List<Vector3>();
        float interval = totalDist / (count - 1);
        float distTarget = 0f;
        int index = 0;

        for (int i = 0; i < count; i++)
        {
            while (index < cumulative.Count - 1 && cumulative[index] < distTarget)
                index++;

            if (index == 0)
            {
                even.Add(sampled[0]);
            }
            else
            {
                float d0 = cumulative[index - 1];
                float d1 = cumulative[index];
                float t = Mathf.InverseLerp(d0, d1, distTarget);
                Vector3 pt = Vector3.Lerp(sampled[index - 1], sampled[index], t);
                even.Add(pt);
            }

            distTarget += interval;
        }

        return even;
    }

    private T InstantiateInEditor<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour
    {
        if (prefab == null) return null;

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(transform);

        return obj.GetComponent<T>();
    }

    //private GameObject InstantiateInEditor(GameObject prefab, Vector3 position, Quaternion rotation)
    //{
    //    if (prefab == null) return null;

    //    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
    //    obj.transform.position = position;
    //    obj.transform.rotation = rotation;
    //    obj.transform.SetParent(transform);
    //    return obj;
    //}

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
        evenlySpacedPoints.Clear();
        mapNodes.Clear();
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
                Vector3 pos = GetPoint(p0, p1, p2, p3, t);
                Gizmos.DrawLine(prev, pos);
                prev = pos;
            }
        }
    }
#endif
}
