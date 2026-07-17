#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BeachHero;
using System.Collections.Generic;

[CustomEditor(typeof(PlayerPreviewEditTool))]
public class PlayerPreviewEditor : Editor
{
    private PlayerPreviewEditTool tool;

    // raw sampling + curve buffers for spline smoothing 
    private List<Vector3> rawDrawnPoints = new List<Vector3>();
    private List<Vector3> curvePoints = new List<Vector3>();
    private float editorSplineStep = 0.05f;

    // freehand drawing state
    private bool isDragging = false;
    private Vector3 lastAddedPointWorld = Vector3.zero;
    private bool isDrawingMode = false;
    private bool hasStartedFromPlayer = false;

    // Overlay drag state
    private static Rect overlayRect = new Rect(10, 10, 300, 160);
    private Color overlayColor = new Color(0.18f, 0.18f, 0.18f, 0.95f);
    private bool isOverlayDragging = false;
    private Vector2 overlayDragOffset;

    private void OnEnable()
    {
        tool = (PlayerPreviewEditTool)target;
        tool.freehandEnabled = true;
        EditorUtility.SetDirty(tool);
    }

    public override void OnInspectorGUI()
    {
        // Keep inspector minimal — editor UI is in Scene GUI
        DrawDefaultInspector();
        GUILayout.Space(4);
        GUILayout.Label("Scene GUI contains preview controls.");
    }

    private void OnSceneGUI()
    {
        if (tool == null) tool = (PlayerPreviewEditTool)target;

        HandleOverlayDragging();

        Transform t = tool.transform;
        Handles.color = Color.cyan;

        // Draw non-interactive red sphere gizmos for each control point
        Handles.color = Color.red;
        for (int i = 0; i < tool.pathPoints.Count; i++)
        {
            Vector3 world = t.TransformPoint(tool.pathPoints[i]);
            float sphereSize = HandleUtility.GetHandleSize(world) * 0.08f;
            Handles.SphereHandleCap(0, world, Quaternion.identity, sphereSize, EventType.Repaint);
        }

        // Draw lines
        for (int i = 0; i < tool.pathPoints.Count - 1; i++)
        {
            Vector3 a = t.TransformPoint(tool.pathPoints[i]);
            Vector3 b = t.TransformPoint(tool.pathPoints[i + 1]);
            Handles.DrawLine(a, b);
        }

        // Scene GUI overlay
        Handles.BeginGUI();

        // background
        EditorGUI.DrawRect(overlayRect, overlayColor);

        GUILayout.BeginArea(overlayRect, GUIStyle.none);

        // Header (drag handle)
        Rect header = GUILayoutUtility.GetRect(overlayRect.width - 8, 20);
        GUI.Box(header, "Player Preview (drag header to move)");

        GUILayout.Space(6);

        // single toggle button for drawing + Clear Path
        GUILayout.BeginHorizontal();

        Color prevBg = GUI.backgroundColor;
        Color prevContent = GUI.contentColor;

        // Start / End toggle button: green when starting, red when ending
        GUI.backgroundColor = isDrawingMode ? new Color(0.85f, 0.22f, 0.22f, 1f) : new Color(0.18f, 0.76f, 0.28f, 1f);
        GUI.contentColor = Color.white;
        string drawLabel = isDrawingMode ? "End Drawing" : "Start Drawing";
        if (GUILayout.Button(drawLabel, GUILayout.Height(22)))
        {
            // toggle drawing mode
            isDrawingMode = !isDrawingMode;
            // reset start-from-player state whenever mode toggles
            hasStartedFromPlayer = false;

            // if stopping drawing, clear drag state
            if (!isDrawingMode)
            {
                isDragging = false;
                GUIUtility.hotControl = 0;
            }
            SceneView.RepaintAll();
        }

        // Clear Path button: orange / warning color
        GUI.backgroundColor = new Color(0.95f, 0.55f, 0.10f, 1f);
        GUI.contentColor = Color.white;
        if (GUILayout.Button("Clear Path", GUILayout.Height(22)))
        {
            Undo.RecordObject(tool, "Clear Path");
            tool.ClearPoints();

            // reset editor drawing state so the next draw must start from the player/startpoint
            hasStartedFromPlayer = false;
            isDrawingMode = false;
            isDragging = false;
            GUIUtility.hotControl = 0;

            // reset preview to start
            tool.previewPercent = 0f;
            tool.UpdatePreview(0f);
            EditorUtility.SetDirty(tool);
        }

        // restore GUI colors
        GUI.backgroundColor = prevBg;
        GUI.contentColor = prevContent;

        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // 2) Preview Slider (label + slider on single row)
        GUILayout.BeginHorizontal();
        GUILayout.Label("Percent", GUILayout.Width(60));
        EditorGUI.BeginChangeCheck();
        float newPercent = GUILayout.HorizontalSlider(tool.previewPercent, 0f, 1f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tool, "Preview Scrub");
            tool.previewPercent = Mathf.Clamp01(newPercent);
            tool.UpdatePreview(tool.previewPercent);
            EditorUtility.SetDirty(tool);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        // 3) Step buttons advance/rewind by a fixed editor step
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<< Step", GUILayout.Height(20)))
        {
            Undo.RecordObject(tool, "Preview Step Back");
            tool.AdvancePreviewByFixedStep(false);
            EditorUtility.SetDirty(tool);
        }
        if (GUILayout.Button("Step >>", GUILayout.Height(20)))
        {
            Undo.RecordObject(tool, "Preview Step Forward");
            tool.AdvancePreviewByFixedStep(true);
            EditorUtility.SetDirty(tool);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(6);

        // 5) Unchangeable field: Time (computed from previewSpeed and path length)
        float totalDuration = tool.GetTotalDuration(); // seconds
        float currentTime = totalDuration * tool.previewPercent;
        EditorGUILayout.LabelField("Time (s)", totalDuration > 0f ? currentTime.ToString("F2") : "0.00");

        // 6) Unchangeable field: Distance travelled (computed)
        float totalLen = tool.CalculateTotalLength();
        float distanceTravelled = totalLen * tool.previewPercent;
        EditorGUILayout.LabelField("Distance", distanceTravelled.ToString("F2") + " units");

        GUILayout.EndArea();
        Handles.EndGUI();

        // Handle drawing input only when in drawing mode
        HandleDrawingInput();
    }
    // Replace the existing HandleDrawingInput method with this
    private void HandleDrawingInput()
    {
        Event e = Event.current;

        // don't process drawing unless drawing mode is active
        if (!isDrawingMode)
            return;

        // ignore scene input while pointer is over the overlay UI
        if (overlayRect.Contains(e.mousePosition))
            return;

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        bool leftBtn = (e.button == 0);
        Plane ground = new Plane(Vector3.up, Vector3.zero);

        // Freehand drawing (drag) if freehandEnabled, otherwise single-click add
        if (tool.freehandEnabled)
        {
            if (e.type == EventType.MouseDown && leftBtn && !e.alt)
            {
                // If we haven't yet started drawing from the player/startpoint,
                // require the initial click to hit the Player or StartPointBehaviour.
                if (!hasStartedFromPlayer)
                {
                    Ray guiRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    if (Physics.Raycast(guiRay, out RaycastHit hitInfo, 1000f, LayerMask.GetMask("StartPoint")))
                    {
                        bool hitStart = hitInfo.collider.GetComponent<StartPointBehaviour>() != null;

                        if (!hitStart)
                        {
                            // initial click didn't start on player/startpoint — ignore it
                            e.Use();
                            return;
                        }

                        // valid start
                        hasStartedFromPlayer = true;
                    }
                    else
                    {
                        // no ray hit — ignore
                        e.Use();
                        return;
                    }
                }

                // proceed with existing logic for adding/starting a freehand stroke
                isDragging = true;
                GUIUtility.hotControl = controlID;
                e.Use();

                if (TryGetMouseWorldPoint(e.mousePosition, ground, out Vector3 worldPt))
                {
                    rawDrawnPoints.Clear();
                    curvePoints.Clear();
                    rawDrawnPoints.Add(worldPt);
                    lastAddedPointWorld = worldPt;
                    UpdateSmoothedPathFromRaw();
                    // Undo.RecordObject(tool, "Freehand Add Point");
                    // tool.AddPointWorld(worldPt);
                    // EditorUtility.SetDirty(tool);
                }
            }
            else if (e.type == EventType.MouseDrag && isDragging && leftBtn && !e.alt)
            {
                if (TryGetMouseWorldPoint(e.mousePosition, ground, out Vector3 worldPt))
                {
                    float dist = Vector3.Distance(lastAddedPointWorld, worldPt);
                    if (dist > tool.GetFreehandSpacing())
                    {
                        //Undo.RecordObject(tool, "Freehand Add Point");
                        //tool.AddPointWorld(worldPt);
                        //lastAddedPointWorld = worldPt;
                        //EditorUtility.SetDirty(tool);
                        //tool.UpdatePreview(tool.previewPercent);

                        rawDrawnPoints.Add(worldPt);
                        lastAddedPointWorld = worldPt;
                        UpdateSmoothedPathFromRaw();
                    }
                }
                e.Use();
            }
            else if (e.type == EventType.MouseUp && isDragging && leftBtn)
            {
                isDragging = false;
                GUIUtility.hotControl = 0;
                Event.current.Use();
                // finalize smoothing one last time
                UpdateSmoothedPathFromRaw();
                isDrawingMode = false;
                hasStartedFromPlayer = false;
                SceneView.RepaintAll();
            }
        }
    }

    private void UpdateSmoothedPathFromRaw()
    {
        curvePoints.Clear();

        if (rawDrawnPoints.Count < 4)
        {
            // fallback: use raw points directly (convert to local)
            tool.pathPoints = new List<Vector3>();
            foreach (var wp in rawDrawnPoints)
                tool.pathPoints.Add(tool.transform.InverseTransformPoint(wp));
        }
        else
        {
            // Build Catmull-Rom sampled points for every consecutive 4 points
            for (int i = 0; i <= rawDrawnPoints.Count - 4; i++)
            {
                Vector3 p0 = rawDrawnPoints[i];
                Vector3 p1 = rawDrawnPoints[i + 1];
                Vector3 p2 = rawDrawnPoints[i + 2];
                Vector3 p3 = rawDrawnPoints[i + 3];

                // sample along segment using editorSplineStep
                for (float t = 0f; t <= 1f; t += editorSplineStep)
                {
                    Vector3 pt = CatmullSplineUtils.GetPoint(
                        p0, p1, p2, p3, t
                    );
                    curvePoints.Add(pt);
                }
            }

            // Now create evenly spaced points using same spacing used at runtime
            var smoothed = CatmullSplineUtils.GetEvenlySpacedPoints(curvePoints, tool.evenlySpacing);

            // Convert world smoothed points to tool local-space and assign
            tool.pathPoints = new List<Vector3>();
            for (int i = 0; i < smoothed.Count; i++)
            {
                Vector3 local = tool.transform.InverseTransformPoint(smoothed[i]);
                if (tool.enforceYZero) local.y = 0f;
                tool.pathPoints.Add(local);
            }
        }

        // update preview display
        tool.UpdatePreview(tool.previewPercent);
        EditorUtility.SetDirty(tool);
    }

    private void HandleOverlayDragging()
    {
        Event e = Event.current;
        Rect headerRect = new Rect(overlayRect.x, overlayRect.y, overlayRect.width, 20f);

        if (e.type == EventType.MouseDown && e.button == 0 && headerRect.Contains(e.mousePosition))
        {
            isOverlayDragging = true;
            overlayDragOffset = e.mousePosition - overlayRect.position;
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && isOverlayDragging && e.button == 0)
        {
            overlayRect.position = e.mousePosition - overlayDragOffset;

            if (SceneView.lastActiveSceneView != null)
            {
                Rect svRect = SceneView.lastActiveSceneView.position;
                overlayRect.x = Mathf.Clamp(overlayRect.x, 0, svRect.width - overlayRect.width);
                overlayRect.y = Mathf.Clamp(overlayRect.y, 0, svRect.height - overlayRect.height);
            }

            e.Use();
            SceneView.RepaintAll();
        }
        else if (e.type == EventType.MouseUp && isOverlayDragging && e.button == 0)
        {
            isOverlayDragging = false;
            e.Use();
        }
    }

    private bool TryGetMouseWorldPoint(Vector2 guiMousePos, Plane ground, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;
        Ray ray = HandleUtility.GUIPointToWorldRay(guiMousePos);
        if (ground.Raycast(ray, out float enter))
        {
            worldPoint = ray.GetPoint(enter);
            worldPoint.y = 0f;
            return true;
        }
        return false;
    }
}
#endif