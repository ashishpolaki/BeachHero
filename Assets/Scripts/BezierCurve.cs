using Codice.Client.BaseCommands.FastExport;
using Codice.Client.Common.GameUI;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class BezierKeyframe
{
    public Vector3 position; // Keyframe position (world position)
    public Vector3 inTangentLocal; // Incoming tangent (local position relative to position)
    public Vector3 outTangentLocal; // Outgoing tangent (local position relative to position)

    // Property to calculate the world position of the inTangent
    public Vector3 InTangentWorld => position + inTangentLocal;

    // Property to calculate the world position of the outTangent
    public Vector3 OutTangentWorld => position + outTangentLocal;
}

public class BezierCurve : MonoBehaviour
{
    public BezierKeyframe[] keyframes; // Array of keyframes
    public List<Vector3> curvePoints = new List<Vector3>(); // List to store collected points
    public int resolution = 20; // Resolution of the curve (number of segments per keyframe pair)

    public bool isvalidate = false;
    public LineRenderer lineRenderer; // LineRenderer component to visualize the curve
    private void OnValidate()
    {
        if (!isvalidate)
            return;
        lineRenderer.positionCount = curvePoints.Count;
        lineRenderer.SetPositions(curvePoints.ToArray());
    }

    private void OnDrawGizmos()
    {
        if (keyframes == null || keyframes.Length < 2)
            return;

        Gizmos.color = Color.red;
        curvePoints.Clear(); // Clear the list before collecting new points

        // Draw the curve between each pair of keyframes
        for (int i = 0; i < keyframes.Length - 1; i++)
        {
            BezierKeyframe start = keyframes[i];
            BezierKeyframe end = keyframes[i + 1];

            // Draw the cubic B�zier curve
            Vector3 previousPoint = start.position;
            curvePoints.Add(previousPoint); // Add the starting point to the list

            for (int j = 1; j <= resolution; j++)
            {
                float t = j / (float)resolution; // Calculate t based on resolution
                Vector3 point = CalculateBezierPoint(t, start.position, start.OutTangentWorld, end.InTangentWorld, end.position);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;

                curvePoints.Add(point); // Collect the point
            }
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0; // (1-t)^3 * P0
        point += 3 * uu * t * p1; // 3(1-t)^2 * t * P1
        point += 3 * u * tt * p2; // 3(1-t) * t^2 * P2
        point += ttt * p3;        // t^3 * P3

        return point;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor
{
    private BezierCurve curve;
    private bool[] showTangents;
    private bool[] showHandles;
    public static float KeyFramePositionSize = 0.2f; 
    public static float KeyFramePositionPickUpSize = 1f; 
    public static float keyFrameTangetHandleSize = 0.5f;
    public static float keyFrameTangetCubeSize = 0.1f;

    private void OnEnable()
    {
        curve = (BezierCurve)target;
    }

    private void OnSceneGUI()
    {
        if (curve.keyframes == null || curve.keyframes.Length < 2)
            return;

        // Initialize the showTangents array if needed
        if (showTangents == null || showTangents.Length != curve.keyframes.Length)
        {
            showTangents = new bool[curve.keyframes.Length];
        }
        // Ensure the showHandles array is initialized
        if (showHandles == null || showHandles.Length != curve.keyframes.Length)
        {
            showHandles = new bool[curve.keyframes.Length];
        }

        for (int i = 0; i < curve.keyframes.Length; i++)
        {
            BezierKeyframe keyframe = curve.keyframes[i];

            // Draw an interactive Sphere Handle
            Handles.color = Color.green;
            if (Handles.Button(keyframe.position, Quaternion.identity, KeyFramePositionSize, KeyFramePositionPickUpSize, Handles.SphereHandleCap))
            {
                // Set all other showHandles to false
                for (int j = 0; j < showHandles.Length; j++)
                {
                    showHandles[j] = false;
                }
                // Reset all tangent visibility flags
                for (int j = 0; j < showTangents.Length; j++)
                {
                    showTangents[j] = false;
                }

                // Toggle the visibility of the PositionHandle for this keyframe
                showHandles[i] = true;
                // Enable tangent visibility for the current keyframe
                showTangents[i] = true;
            }

            // Show the PositionHandle only if the button was clicked
            if (showHandles[i])
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPosition = Handles.PositionHandle(keyframe.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(curve, "Move Keyframe");
                    keyframe.position = newPosition;

                    // Force the Scene view to repaint
                    SceneView.RepaintAll();
                }
            }

            // Show tangent handlers only if the position handler was interacted with
            if (showTangents[i])
            {
                // Editable inTangentLocal (Sliders for X, Y, Z axes)
                Vector3 inTangentWorld = keyframe.InTangentWorld; // Get the current world position of the inTangent
                EditorGUI.BeginChangeCheck();
                Vector3 newInTangentWorld = CustomPositionHandle(inTangentWorld, Quaternion.identity, keyFrameTangetHandleSize, 0.15f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(curve, "Move In Tangent");

                    // Update the local position of the inTangent
                    keyframe.inTangentLocal = newInTangentWorld - keyframe.position;
                }
                Handles.color = Color.yellow;
                // Draw cube for inTangent
                Handles.CubeHandleCap(0, keyframe.InTangentWorld, Quaternion.identity, keyFrameTangetCubeSize, EventType.Repaint);


                // Editable outTangentLocal  (Sliders for X, Y, Z axes)
                Vector3 outTangentWorld = keyframe.OutTangentWorld; // Get the current world position of the outTangent
                EditorGUI.BeginChangeCheck();
                Vector3 newOutTangentWorld = CustomPositionHandle(outTangentWorld, Quaternion.identity, keyFrameTangetHandleSize, 0.15f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(curve, "Move Out Tangent");

                    // Update the local position of the outTangent
                    keyframe.outTangentLocal = newOutTangentWorld - keyframe.position;
                }
                Handles.color = Color.cyan;
                // Draw cube for inTangent
                Handles.CubeHandleCap(0, keyframe.OutTangentWorld, Quaternion.identity, keyFrameTangetCubeSize, EventType.Repaint);

                // Draw tangent lines
                Handles.color = Color.white;
                Handles.DrawLine(keyframe.position, keyframe.InTangentWorld);
                Handles.DrawLine(keyframe.position, keyframe.OutTangentWorld);
            }
        }

        //for (int i = 0; i < curve.keyframes.Length; i++)
        //{
        //    BezierKeyframe keyframe = curve.keyframes[i];

        //    // Editable keyframe position (Sphere Handle)
        //    Handles.color = Color.gray;
        //    EditorGUI.BeginChangeCheck();
        ////    Vector3 newPosition = Handles.PositionHandle(keyframe.position, Quaternion.identity);
        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        Undo.RecordObject(curve, "Move Keyframe");

        //        // Move the keyframe and its tangents by the same delta
        //     //   keyframe.position = newPosition;

        //        // Reset all tangent visibility flags
        //        for (int j = 0; j < showTangents.Length; j++)
        //        {
        //            showTangents[j] = false;
        //        }

        //        // Enable tangent visibility for the current keyframe
        //        showTangents[i] = true;

        //        // Force the Scene view to repaint
        //        SceneView.RepaintAll();
        //    }


        //    // Interactive Sphere Handle
        //    if (Handles.Button(keyframe.position, Quaternion.identity, 0.4f, 0.4f, Handles.SphereHandleCap))
        //    {
        //        Handles.color = Color.green;
        //        Debug.Log($"Clicked keyframe {i}");
        //    }

        //    // Draw sphere for keyframe position
        //  //  Handles.SphereHandleCap(0, keyframe.position, Quaternion.identity, 0.2f, EventType.Repaint);

        //    // Show tangent handlers only if the position handler was interacted with
        //    if (showTangents[i])
        //    {
        //        // Editable inTangentLocal (Position Handle)
        //        Handles.color = Color.yellow;
        //        EditorGUI.BeginChangeCheck();
        //        Vector3 newInTangentWorld = Handles.PositionHandle(keyframe.InTangentWorld, Quaternion.identity);
        //        if (EditorGUI.EndChangeCheck())
        //        {
        //            Undo.RecordObject(curve, "Move In Tangent");

        //            // Update the local position of the inTangent
        //            keyframe.inTangentLocal = newInTangentWorld - keyframe.position;
        //        }

        //        // Draw cube for inTangent
        //        Handles.CubeHandleCap(0, keyframe.InTangentWorld, Quaternion.identity, 0.1f, EventType.Repaint);

        //        // Editable outTangentLocal (Position Handle)
        //        Handles.color = Color.cyan;
        //        EditorGUI.BeginChangeCheck();
        //        Vector3 newOutTangentWorld = Handles.PositionHandle(keyframe.OutTangentWorld, Quaternion.identity);
        //        if (EditorGUI.EndChangeCheck())
        //        {
        //            Undo.RecordObject(curve, "Move Out Tangent");

        //            // Update the local position of the outTangent
        //            keyframe.outTangentLocal = newOutTangentWorld - keyframe.position;
        //        }

        //        // Draw cube for outTangent
        //        Handles.CubeHandleCap(0, keyframe.OutTangentWorld, Quaternion.identity, 0.1f, EventType.Repaint);

        //        // Draw tangent lines
        //        Handles.color = Color.white;
        //        Handles.DrawLine(keyframe.position, keyframe.InTangentWorld);
        //        Handles.DrawLine(keyframe.position, keyframe.OutTangentWorld);
        //    }
        //}
    }

    private Vector3 CustomPositionHandle(Vector3 position, Quaternion rotation, float handleSize, float rectangleToSliderRatio = 0.15f)
    {
        int controlId1 = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
        int controlId2 = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
        int controlId3 = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

        float sliderSize = HandleUtility.GetHandleSize(position) * handleSize;
        float rectangleSize = sliderSize * rectangleToSliderRatio;

        Vector3 snap = Vector3.zero;

        Vector3 axis1 = rotation * Vector3.right;
        Vector3 axis2 = rotation * Vector3.up;
        Vector3 axis3 = rotation * Vector3.forward;
        Color axis1Color = new Color(0.9f, 0.3f, 0.1f);
        Color axis2Color = new Color(0.6f, 0.9f, 0.3f);
        Color axis3Color = new Color(0.2f, 0.4f, 0.9f);

        Vector3 updatedPosition = position;

        Handles.color = axis1Color;
        updatedPosition += Handles.Slider(position, axis1, sliderSize, Handles.ArrowHandleCap, snap.x) - position;
        updatedPosition += Handles.Slider2D(controlId1, position, rectangleSize * (axis3 + axis2), Vector3.Cross(axis3, axis2), axis2, axis3, rectangleSize, Handles.RectangleHandleCap, new Vector2(snap.y, snap.z)) - position;

        Handles.color = axis2Color;
        updatedPosition += Handles.Slider(position, axis2, sliderSize, Handles.ArrowHandleCap, snap.y) - position;
        updatedPosition += Handles.Slider2D(controlId2, position, rectangleSize * (axis1 + axis3), Vector3.Cross(axis1, axis3), axis3, axis1, rectangleSize, Handles.RectangleHandleCap, new Vector2(snap.z, snap.x)) - position;

        Handles.color = axis3Color;
        updatedPosition += Handles.Slider(position, axis3, sliderSize, Handles.ArrowHandleCap, snap.z) - position;
        updatedPosition += Handles.Slider2D(controlId3, position, rectangleSize * (axis1 + axis2), Vector3.Cross(axis1, axis2), axis2, axis1, rectangleSize, Handles.RectangleHandleCap, new Vector2(snap.y, snap.x)) - position;

        return updatedPosition;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add Keyframe"))
        {
            AddKeyframe();
        }

        if (GUILayout.Button("Remove Last Keyframe"))
        {
            RemoveKeyframe();
        }
    }

    private void AddKeyframe()
    {
        Undo.RecordObject(curve, "Add Keyframe");

        BezierKeyframe newKeyframe = new BezierKeyframe
        {
            position = Vector3.zero,
            inTangentLocal = Vector3.left,
            outTangentLocal = Vector3.right
        };

        if (curve.keyframes == null)
        {
            curve.keyframes = new BezierKeyframe[] { newKeyframe };
        }
        else
        {
            ArrayUtility.Add(ref curve.keyframes, newKeyframe);
        }
    }

    private void RemoveKeyframe()
    {
        if (curve.keyframes == null || curve.keyframes.Length == 0)
            return;

        Undo.RecordObject(curve, "Remove Keyframe");
        ArrayUtility.RemoveAt(ref curve.keyframes, curve.keyframes.Length - 1);
    }
}

#endif