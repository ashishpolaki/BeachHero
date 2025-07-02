using BeachHero;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class MapNode
{
    public Transform controlPoint;
    public GameObject levelObject;
    public GameObject levelCompleteIndicator;
    public GameObject dash1Object;
    public GameObject dash1CompleteIndicator;
    public GameObject dash2Object;
    public GameObject dash2CompleteIndicator;
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
    public MapDashEditor dashPrefab;
    public List<BezierPoint> bezierPoints = new List<BezierPoint>();
    public List<MapNode> mapNodes = new List<MapNode>();
    public List<Vector3> evenlySpacedPoints = new List<Vector3>();

    [Range(0f, 1f)] public float dashOffset1 = 0.33f;
    [Range(0f, 1f)] public float dashOffset2 = 0.66f;
    public int curveSamples = 20;
    public int pointsPerSegment = 100;

#if UNITY_EDITOR
    public void GenerateMapPointsInEditor()
    {
        if (!Application.isPlaying && bezierPoints != null && bezierPoints.Count >= 2)
        {
            evenlySpacedPoints = GenerateGlobalEvenlySpacedPoints(bezierPoints, pointsPerSegment);
            ClearGeneratedObjects();

            //  STEP 1: Spawn levels on evenly spaced points
            foreach (Vector3 point in evenlySpacedPoints)
            {
                GameObject level = InstantiateInEditor(levelPrefab.gameObject, point, Quaternion.identity);
                DestroyImmediate(level.GetComponent<MapLevelEditor>());

                MapNode mapNode = new MapNode
                {
                    controlPoint = null,
                    levelObject = level,
                    levelCompleteIndicator = levelPrefab.levelCompleteIndicator
                };

                mapNodes.Add(mapNode);
            }

            // STEP 2: Spawn dashes using Bézier points
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

                // Dash 1 at t = dashOffset1
                Vector3 dashPos1 = BezierCurveUtils.GetPoint(p0, p1, p2, p3, dashOffset1);
                Quaternion dashRot1 = Quaternion.LookRotation(BezierCurveUtils.GetTangent(p0, p1, p2, p3, dashOffset1));
                GameObject dash1 = InstantiateInEditor(dashPrefab.gameObject, dashPos1, dashRot1);

                // Dash 2 at t = dashOffset2
                Vector3 dashPos2 = BezierCurveUtils.GetPoint(p0, p1, p2, p3, dashOffset2);
                Quaternion dashRot2 = Quaternion.LookRotation(BezierCurveUtils.GetTangent(p0, p1, p2, p3, dashOffset2));
                GameObject dash2 = InstantiateInEditor(dashPrefab.gameObject, dashPos2, dashRot2);

                // Optional: Link dashes to a node if you want
                if (i < mapNodes.Count)
                {
                    mapNodes[i].dash1Object = dash1;
                    mapNodes[i].dash2Object = dash2;
                    mapNodes[i].dash1CompleteIndicator = dashPrefab.dashCompleteIndicator;
                    mapNodes[i].dash2CompleteIndicator = dashPrefab.dashCompleteIndicator;
                }

                DestroyImmediate(dash1.GetComponent<MapDashEditor>());
                DestroyImmediate(dash2.GetComponent<MapDashEditor>());
            }
        }
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

            var segment = BezierCurveUtils.GetEvenlySpacedPoints(p0, p1, p2, p3, 200); // High-res per segment

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

    private GameObject InstantiateInEditor(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.SetParent(transform);
        return obj;
    }

    public void ClearGeneratedObjects()
    {
        foreach (var node in mapNodes)
        {
            if (node.levelObject != null) DestroyImmediate(node.levelObject);
            if (node.dash1Object != null) DestroyImmediate(node.dash1Object);
            if (node.dash2Object != null) DestroyImmediate(node.dash2Object);
        }
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
                Vector3 pos = BezierCurveUtils.GetPoint(p0, p1, p2, p3, t);
                Gizmos.DrawLine(prev, pos);
                prev = pos;
            }
        }

        //Draw evenly spaced points
        Gizmos.color = Color.yellow;
        foreach (var point in evenlySpacedPoints)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }
    }
#endif
}
