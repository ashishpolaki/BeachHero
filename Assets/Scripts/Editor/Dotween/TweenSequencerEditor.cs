#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using DG.Tweening;
using DG.DOTweenEditor;

namespace BeachHero
{
    [CustomEditor(typeof(TweenSequencer))]
    public class TweenSequencerEditor : Editor
    {
        public static TweenSequencerEditor Instance;

        private SerializedProperty _clipsProp;
        private SerializedProperty _timelineDurationProp;
        private SerializedProperty _triggerEventsProp;
        private TweenSequencer _sequencer;
        private FieldInfo _sequenceField;

        // preview
        private Sequence _previewSequence;
        private float _progress = 0f;
        private bool _autoReplay = false;
        private bool _isPlaying = false;

        // timeline visuals
        private const float TIMELINE_HEIGHT = 60f;
        private const float TIMELINE_LEFT_MARGIN = 20f;
        private const float TIMELINE_RIGHT_MARGIN = 20f;
        private const float CLIP_BAR_HEIGHT = 30f;
        private const float CLIP_BAR_PADDING = 6f;
        private const float EDGE_HANDLE_WIDTH = 6f; // px for left/right resize handles

        // selection + dragging
        private int _selectedIndex = -1;
        private int _draggingIndex = -1;
        private DragMode _dragMode = DragMode.None;
        private float _dragStartMouseX;
        private float _dragOriginalStart;
        private float _dragOriginalDuration;

        // trigger dragging state
        private int _draggingTriggerIndex = -1;
        private float _triggerDragStartMouseX = 0f;
        private float _triggerOriginalPercent = 0f;
        private int _selectedTriggerIndex = -1;

        // timeline duration limits
        private float minimumTimelineDuration = 0.1f;
        private float maximumTimelineDuration = 3f;

        private enum DragMode { None, Move, ResizeLeft, ResizeRight }

        private void OnEnable()
        {
            Instance = this;
            if (Application.isPlaying)
            {
                return;
            }
            _sequencer = (TweenSequencer)target;
            _clipsProp = serializedObject.FindProperty("clips");
            _timelineDurationProp = serializedObject.FindProperty("timelineDuration");
            _triggerEventsProp = serializedObject.FindProperty("triggerEvents"); // <-- init
            _sequenceField = typeof(TweenSequencer).GetField("_sequence", BindingFlags.NonPublic | BindingFlags.Instance);

            if (_selectedIndex >= _clipsProp.arraySize) _selectedIndex = -1;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            if (Application.isPlaying)
            {
                return;
            }
            StopPreviewAndCleanup();
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                return;
            }
            serializedObject.Update();
            HandleKeyboardShortcuts();

            // top toolbar
            EditorGUILayout.Space(6);
            DrawTopToolbar();
            EditorGUILayout.Space(4);
            EditorGUILayout.Slider(_timelineDurationProp, minimumTimelineDuration, maximumTimelineDuration, new GUIContent("Duration (s)"));

            // Progress slider
            EditorGUI.BeginChangeCheck();
            _progress = EditorGUILayout.Slider("Progress", _progress, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                ScrubToProgress(_progress);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopType"));

            // Timeline label
            DrawTimelineArea();
            DrawBottomControls();

            // Trigger events inspector (inline, editable)
            DrawTriggerEventInspector();

            // Selected clip details
            EditorGUILayout.Space(6);
            DrawSelectedClipInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTriggerEventInspector()
        {
            if (_triggerEventsProp == null)
                _triggerEventsProp = serializedObject.FindProperty("triggerEvents");

            EditorGUILayout.Space(6);

            // --- Label + Buttons on same horizontal row ---
            EditorGUILayout.BeginHorizontal();

            // Label takes all remaining space
            EditorGUILayout.LabelField("Trigger Events", EditorStyles.boldLabel, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));

            // Add Event (+)
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                int idx = _triggerEventsProp.arraySize;
                _triggerEventsProp.InsertArrayElementAtIndex(idx);
                var newElem = _triggerEventsProp.GetArrayElementAtIndex(idx);

                // Initialize timePercent to current scrub position
                var timePercentProp = newElem.FindPropertyRelative("timePercent");
                if (timePercentProp != null) timePercentProp.floatValue = Mathf.Clamp01(_progress) * 100f;

                newElem.isExpanded = true;

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_sequencer);
                try { EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene); } catch { }

                // select the newly added trigger
                _selectedTriggerIndex = idx;
            }

            // Remove Last (-)
            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                int sizeBefore = _triggerEventsProp.arraySize;
                if (sizeBefore > 0)
                {
                    int lastIdx = sizeBefore - 1;

                    // if selected trigger was the last one, deselect; otherwise clamp selection
                    if (_selectedTriggerIndex == lastIdx) _selectedTriggerIndex = -1;
                    else if (_selectedTriggerIndex > lastIdx) _selectedTriggerIndex = Mathf.Clamp(_selectedTriggerIndex, -1, lastIdx - 1);

                    // if dragging that trigger, cancel drag
                    if (_draggingTriggerIndex == lastIdx) _draggingTriggerIndex = -1;
                    else if (_draggingTriggerIndex > lastIdx) _draggingTriggerIndex = Mathf.Clamp(_draggingTriggerIndex, -1, lastIdx - 1);

                    _triggerEventsProp.DeleteArrayElementAtIndex(lastIdx);

                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_sequencer);
                    try { EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene); } catch { }

                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            serializedObject.ApplyModifiedProperties();
        }

        private void HandleKeyboardShortcuts()
        {
            var e = Event.current;
            if (e == null) return;

            bool mod = e.control || e.command; // Ctrl on Win, Cmd on macOS
            if (e.type == EventType.KeyDown && mod)
            {
                if (e.keyCode == KeyCode.D) // Ctrl/Cmd + D -> duplicate
                {
                    DuplicateSelectedClip();
                    e.Use();
                    GUI.FocusControl(null);
                }
                else if (e.keyCode == KeyCode.Delete) // Ctrl/Cmd + Delete -> delete
                {
                    DeleteSelectedClip();
                    e.Use();
                    GUI.FocusControl(null);
                }
            }
        }

        private void DuplicateSelectedClip()
        {
            if (_clipsProp == null) return;
            if (_selectedIndex < 0 || _selectedIndex >= _clipsProp.arraySize) return;

            // record undo
            Undo.RegisterCompleteObjectUndo(_sequencer, "Duplicate Clip");

            var srcProp = _clipsProp.GetArrayElementAtIndex(_selectedIndex);
            object srcObj = srcProp.managedReferenceValue;

            int insertIndex = _selectedIndex + 1;

            // Insert a new element slot
            _clipsProp.InsertArrayElementAtIndex(insertIndex);
            var newElem = _clipsProp.GetArrayElementAtIndex(insertIndex);

            if (srcObj == null)
            {
                // leave the new element as null/default
                newElem.managedReferenceValue = null;
            }
            else
            {
                // deep-copy the managed reference using JSON (Editor-only)
                Type t = srcObj.GetType();
                string json = EditorJsonUtility.ToJson(srcObj);
                object copy = Activator.CreateInstance(t);
                EditorJsonUtility.FromJsonOverwrite(json, copy);
                newElem.managedReferenceValue = copy;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_sequencer);
            _selectedIndex = insertIndex;
            // mark scene dirty so user is prompted to save
            try { EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene); } catch { }
        }

        private void DeleteSelectedClip()
        {
            if (_clipsProp == null) return;
            if (_selectedIndex < 0 || _selectedIndex >= _clipsProp.arraySize) return;

            // confirm? (optional) — we perform deletion immediately
            Undo.RegisterCompleteObjectUndo(_sequencer, "Delete Clip");

            // Delete element (works for managedReference arrays as used)
            _clipsProp.DeleteArrayElementAtIndex(_selectedIndex);
            serializedObject.ApplyModifiedProperties();

            // clamp selection
            int newIndex = Mathf.Clamp(_selectedIndex, 0, _clipsProp.arraySize - 1);
            _selectedIndex = (_clipsProp.arraySize == 0) ? -1 : newIndex;

            EditorUtility.SetDirty(_sequencer);
            try { EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene); } catch { }
        }

        private void DrawTopToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (!_isPlaying)
            {
                if (GUILayout.Button("Play", GUILayout.Width(64)))
                {
                    StartPreview(playImmediately: true);
                }
            }
            else
            {
                if (GUILayout.Button("Pause", GUILayout.Width(64)))
                {
                    PausePreview();
                }
            }

            if (GUILayout.Button("Stop", GUILayout.Width(64)))
            {
                StopPreviewAndCleanup();
                _progress = 0f;
            }

            _autoReplay = GUILayout.Toggle(_autoReplay, "Auto Replay", "Button", GUILayout.Width(100));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTimelineArea()
        {
            GUIStyle centeredBold = new GUIStyle(EditorStyles.boldLabel);
            centeredBold.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Timeline", centeredBold, GUILayout.ExpandWidth(true));

            float total = ComputeTotalDuration();
            if (total <= 0f) total = 1f;

            int sequencerClipsLength = _sequencer.Clips != null ? _sequencer.Clips.Length : 1;
            float timelineHeight = TIMELINE_HEIGHT + ((CLIP_BAR_HEIGHT + CLIP_BAR_PADDING - 2f) * sequencerClipsLength);
            Rect timelineRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, timelineHeight);
            GUI.Box(timelineRect, GUIContent.none);

            Rect inner = new Rect(
                timelineRect.x + TIMELINE_LEFT_MARGIN,
                timelineRect.y + 28,
                timelineRect.width - TIMELINE_LEFT_MARGIN - TIMELINE_RIGHT_MARGIN,
                 -40 + timelineHeight
            );

            EditorGUI.DrawRect(inner, new Color(0.11f, 0.11f, 0.11f, 1f));

            // Tick spacing (0.1s steps). If total is not an exact multiple of step, we ceil and clamp.
            float step = 0.1f;
            int steps = Mathf.CeilToInt(total / step);

            for (int i = 0; i <= steps; i++)
            {
                float time = i * step;
                if (time > total) time = total; // final clamp for last tick

                float normalized = Mathf.Clamp01(time / total);
                float x = inner.x + normalized * inner.width;

                // make full-second ticks slightly bigger
                bool isWholeSecond = Mathf.Abs(time - Mathf.Round(time)) < 0.0001f;
                float tickHeight = isWholeSecond ? 12f : 6f;
                Rect tick = new Rect(x - 0.5f, inner.y - tickHeight - 2f, 1f, tickHeight);
                EditorGUI.DrawRect(tick, new Color(0.7f, 0.7f, 0.7f, 0.6f));

                // Label in seconds (0.1s, 0.2s ...). Show up to one decimal place
                string label = $"{time:0.0}s";
                Vector2 size = EditorStyles.miniLabel.CalcSize(new GUIContent(label));
                Rect lbl = new Rect(x - size.x * 0.5f, inner.y - tickHeight - 18f, size.x, size.y);
                EditorGUI.LabelField(lbl, label, EditorStyles.miniLabel);
            }

            // draw clips
            var clipsRuntime = _sequencer.Clips;
            if (clipsRuntime != null)
            {
                for (int i = 0; i < clipsRuntime.Length; i++)
                {
                    var clip = clipsRuntime[i];
                    if (clip == null) continue;

                    // clip times are in seconds; display them normalized by `total`
                    float start = clip.startTime;
                    float dur = Mathf.Max(0.0001f, clip.duration);

                    // clamp to timeline bounds driven by total
                    start = Mathf.Clamp(start, 0f, total);
                    if (start + dur > total) dur = Mathf.Max(0.0001f, total - start);

                    float x = inner.x + Mathf.Clamp01(start / total) * inner.width;
                    float w = Mathf.Clamp01(dur / total) * inner.width;
                    float y = inner.y + 6 + i * (CLIP_BAR_HEIGHT + CLIP_BAR_PADDING);

                    if (y + CLIP_BAR_HEIGHT > inner.y + inner.height)
                    {
                        y = inner.y + inner.height - CLIP_BAR_HEIGHT - 2;
                    }

                    Rect barRect = new Rect(x, y, Mathf.Max(6f, w), CLIP_BAR_HEIGHT);

                    // 1) Fill bar base and outline for crispness
                    Color baseFill = new Color(0.18f, 0.18f, 0.18f, 1f);
                    EditorGUI.DrawRect(barRect, baseFill);
                    Handles.DrawSolidRectangleWithOutline(barRect, new Color(0, 0, 0, 0), new Color(0.12f, 0.12f, 0.12f, 1f));

                    // 2) Label inside the bar (centered)
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        clipping = TextClipping.Clip,
                        richText = true   // <<-- IMPORTANT: enable rich text so <color> tags work
                    };
                    Rect lblRect = new Rect(barRect.x, barRect.y + 2, Mathf.Max(16f, barRect.width - 5f), 14);
                    string clipName = GetTweenDisplayName(clip);
                    EditorGUI.LabelField(lblRect, clipName, labelStyle);

                    // 3) Draw color stripe beneath the label (visual only; editing moved to inspector)
                    Color clipColor = LoadClipColor(clip);
                    float stripeHeight = 4f;
                    Rect stripeRect = new Rect(lblRect.x + 3, lblRect.yMax + 3f, Mathf.Clamp(lblRect.width, 12f, barRect.width), stripeHeight);
                    EditorGUI.DrawRect(stripeRect, clipColor);

                    // 4) Edge handle rects (for cursor and click detection)
                    Rect leftHandle = new Rect(barRect.x - EDGE_HANDLE_WIDTH / 2f, barRect.y, EDGE_HANDLE_WIDTH, CLIP_BAR_HEIGHT);
                    Rect rightHandle = new Rect(barRect.xMax - EDGE_HANDLE_WIDTH / 2f, barRect.y, EDGE_HANDLE_WIDTH, CLIP_BAR_HEIGHT);

                    // 5) Add cursor rectangles so hovering shows the appropriate mouse cursor
                    //    Resize cursor on edges, move cursor on the body.
                    EditorGUIUtility.AddCursorRect(leftHandle, MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(rightHandle, MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(barRect, MouseCursor.MoveArrow);

                    Vector2 mouse = Event.current.mousePosition;

                    // Prioritize handle clicks
                    if (Event.current.type == EventType.MouseDown && leftHandle.Contains(mouse))
                    {
                        BeginDrag(i, DragMode.ResizeLeft);
                        Event.current.Use();
                    }
                    else if (Event.current.type == EventType.MouseDown && rightHandle.Contains(mouse))
                    {
                        BeginDrag(i, DragMode.ResizeRight);
                        Event.current.Use();
                    }
                    // clicking the bar selects (and prepares for drag) DO NOT change progress on simple click
                    else if (Event.current.type == EventType.MouseDown && barRect.Contains(mouse))
                    {
                        // prepare dragging (BeginDrag records undo and stores original values).
                        // We still call BeginDrag so an immediate mouse-drag will move the clip.
                        BeginDrag(i, DragMode.Move);

                        // select the clip under the mouse
                        _selectedIndex = i;

                        // NOTE: do NOT change _progress or call ScrubToProgress here.
                        // Progress should only be changed when the user drags in the timeline area (handled elsewhere).
                        Event.current.Use();
                    }

                    // dragging updates
                    if (_draggingIndex == i && _dragMode != DragMode.None &&
                        (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp))
                    {
                        if (Event.current.type == EventType.MouseDrag)
                        {
                            UpdateDrag(i, inner, total);
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            EndDrag();
                            Event.current.Use();
                        }
                    }

                    // 6) Selected highlight (white outline when selected)
                    if (i == _selectedIndex)
                    {
                        Handles.DrawSolidRectangleWithOutline(barRect, new Color(0, 0, 0, 0), Color.white);
                    }
                }
            }

            // --- Draw trigger event markers (draggable, show time in seconds) ---
            if (_triggerEventsProp != null && _triggerEventsProp.isArray)
            {
                float totalDuration = Mathf.Max(0.0001f, ComputeTotalDuration()); // used to show seconds

                // end any trigger drag on mouse up
                if (Event.current.type == EventType.MouseUp && _draggingTriggerIndex != -1)
                {
                    _draggingTriggerIndex = -1;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_sequencer);
                    EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene);
                    Event.current.Use();
                }

                for (int i = 0; i < _triggerEventsProp.arraySize; i++)
                {
                    var te = _triggerEventsProp.GetArrayElementAtIndex(i);
                    if (te == null) continue;
                    var timePercentProp = te.FindPropertyRelative("timePercent");
                    float percent = (timePercentProp != null) ? timePercentProp.floatValue : 0f;

                    float normalized = Mathf.Clamp01(percent / 100f);
                    float x = inner.x + normalized * inner.width;

                    // diamond shape
                    float markerCenterY = inner.y - 14f;
                    float half = 6f;
                    Vector3[] diamond = new Vector3[]
                    {
            new Vector3(x, markerCenterY - half),
            new Vector3(x - half, markerCenterY),
            new Vector3(x, markerCenterY + half),
            new Vector3(x + half, markerCenterY)
                    };

                    Rect markerRect = new Rect(x - half - 2f, markerCenterY - half - 2f, half * 2f + 4f, half * 2f + 4f);

                    // guide line
                    Handles.color = new Color(0.95f, 0.55f, 0.15f, 0.6f);
                    Handles.DrawLine(new Vector3(x, markerCenterY + half + 2f), new Vector3(x, inner.y + inner.height));
                    Handles.color = Color.white;

                    // draw marker
                    Handles.DrawAAConvexPolygon(diamond);
                    Handles.DrawAAPolyLine(2f, new Vector3[] { diamond[0], diamond[1], diamond[2], diamond[3], diamond[0] });

                    // compute seconds for display
                    float seconds = normalized * totalDuration;

                    Vector2 mouse = Event.current.mousePosition;

                    // If this trigger is being dragged, update its percent from mouse delta
                    if (_draggingTriggerIndex == i && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseMove))
                    {
                        float mouseDelta = Event.current.mousePosition.x - _triggerDragStartMouseX;
                        float deltaNorm = (mouseDelta / Mathf.Max(1f, inner.width));
                        float newNorm = Mathf.Clamp01((_triggerOriginalPercent / 100f) + deltaNorm);
                        float newPercent = newNorm * 100f;
                        if (timePercentProp != null)
                        {
                            timePercentProp.floatValue = newPercent;
                            serializedObject.ApplyModifiedProperties();
                            percent = newPercent;
                            normalized = newNorm;
                            seconds = normalized * totalDuration;
                            x = inner.x + normalized * inner.width;
                        }
                        Event.current.Use();
                    }
                    // hover: show seconds tooltip above the marker & change cursor
                    if (markerRect.Contains(mouse))
                    {
                        string tipText = $"Trigger: {seconds:0.00}s";
                        Vector2 tipSize = EditorStyles.miniLabel.CalcSize(new GUIContent(tipText));

                        // place tooltip centered above the marker (leave a small gap)
                        float tipX = x - tipSize.x * 0.5f;
                        float tipY = markerCenterY - half - tipSize.y - 6f; // 6px gap above marker

                        // clamp to inner bounds so it doesn't overflow the timeline rect
                        tipX = Mathf.Clamp(tipX, inner.x + 2f, inner.x + inner.width - tipSize.x - 2f);
                        tipY = Mathf.Max(tipY, inner.y - 40f); // don't go too far above

                        Rect tipRect = new Rect(tipX, tipY, tipSize.x, tipSize.y);
                        GUI.Label(tipRect, tipText, EditorStyles.miniLabel);

                        EditorGUIUtility.AddCursorRect(markerRect, MouseCursor.SlideArrow);

                        // start drag (left mouse down)
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            _draggingTriggerIndex = i;
                            _triggerDragStartMouseX = Event.current.mousePosition.x;
                            _triggerOriginalPercent = percent;

                            // DO NOT change _progress here - clicking/selecting a trigger should not move the scrub
                            // expand the property for editing
                            te.isExpanded = true;
                            serializedObject.ApplyModifiedProperties();

                            Selection.activeObject = _sequencer;
                            EditorGUIUtility.PingObject(_sequencer);

                            Event.current.Use();
                        }
                    }

                    // click without drag: select and expand (do not change progress)
                    if (Event.current.type == EventType.MouseDown && markerRect.Contains(Event.current.mousePosition) && _draggingTriggerIndex == -1)
                    {
                        // select but do not change scrub/progress
                        _selectedTriggerIndex = i;

                        te.isExpanded = true;
                        serializedObject.ApplyModifiedProperties();
                        Selection.activeObject = _sequencer;
                        EditorGUIUtility.PingObject(_sequencer);

                        Event.current.Use();
                    }

                    // optional focused outline when expanded
                    if (te.isExpanded)
                    {
                        Handles.DrawAAPolyLine(3f, new Vector3[] { diamond[0], diamond[1], diamond[2], diamond[3], diamond[0] });
                    }
                }
            }

            // scrub handle (positioned by normalized _progress)
            float scrubX = inner.x + Mathf.Clamp01(_progress) * inner.width;
            Rect scrubRect = new Rect(scrubX - 1, inner.y - 4, 2, inner.height + 8);
            EditorGUI.DrawRect(scrubRect, Color.white);

            // dragging scrub -> set normalized progress based on mouse X
            if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && inner.Contains(Event.current.mousePosition))
            {
                // don't override if currently dragging a clip
                // also don't scrub if the pointer is over a clip or over a trigger marker (so clip/marker clicks remain selection-only)
                bool overInteractive = IsPointerOverTweenClipOrTriggerEvent(Event.current.mousePosition, inner, total);
                if (_dragMode == DragMode.None && !overInteractive)
                {
                    float normalizedAtMouse = Mathf.Clamp01((Event.current.mousePosition.x - inner.x) / inner.width);
                    _progress = normalizedAtMouse;
                    ScrubToProgress(_progress);
                    Event.current.Use();
                }
            }
        }

        private bool IsPointerOverTweenClipOrTriggerEvent(Vector2 mousePos, Rect inner, float total)
        {
            // check clips
            var clipsRuntime = _sequencer.Clips;
            if (clipsRuntime != null)
            {
                for (int i = 0; i < clipsRuntime.Length; i++)
                {
                    var clip = clipsRuntime[i];
                    if (clip == null) continue;
                    float start = Mathf.Clamp(clip.startTime, 0f, total);
                    float dur = Mathf.Max(0.0001f, clip.duration);
                    if (start + dur > total) dur = Mathf.Max(0.0001f, total - start);
                    float x = inner.x + Mathf.Clamp01(start / total) * inner.width;
                    float w = Mathf.Clamp01(dur / total) * inner.width;
                    Rect barRect = new Rect(x, inner.y + 6 + i * (CLIP_BAR_HEIGHT + CLIP_BAR_PADDING), Mathf.Max(6f, w), CLIP_BAR_HEIGHT);
                    if (barRect.Contains(mousePos)) return true;
                }
            }

            // check trigger markers
            if (_triggerEventsProp != null && _triggerEventsProp.isArray)
            {
                for (int i = 0; i < _triggerEventsProp.arraySize; i++)
                {
                    var te = _triggerEventsProp.GetArrayElementAtIndex(i);
                    if (te == null) continue;
                    var timePercentProp = te.FindPropertyRelative("timePercent");
                    float percent = (timePercentProp != null) ? timePercentProp.floatValue : 0f;
                    float normalized = Mathf.Clamp01(percent / 100f);
                    float x = inner.x + normalized * inner.width;
                    float markerCenterY = inner.y - 14f;
                    float half = 6f;
                    Rect markerRect = new Rect(x - half - 2f, markerCenterY - half - 2f, half * 2f + 4f, half * 2f + 4f);
                    if (markerRect.Contains(mousePos)) return true;
                }
            }

            return false;
        }

        private void BeginDrag(int index, DragMode mode)
        {
            _draggingIndex = index;
            _dragMode = mode;
            _dragStartMouseX = Event.current.mousePosition.x;

            var clipProp = _clipsProp.GetArrayElementAtIndex(index);
            var startProp = clipProp.FindPropertyRelative("startTime");
            var durProp = clipProp.FindPropertyRelative("duration");

            _dragOriginalStart = startProp != null ? startProp.floatValue : 0f;
            _dragOriginalDuration = durProp != null ? durProp.floatValue : 0.1f;

            // capture undo
            Undo.RecordObject(_sequencer, "Drag Clip");
        }

        private void UpdateDrag(int index, Rect inner, float total)
        {
            if (_clipsProp == null) return;
            if (index < 0 || index >= _clipsProp.arraySize) return;

            float mouseDelta = Event.current.mousePosition.x - _dragStartMouseX;
            float deltaTime = (mouseDelta / Mathf.Max(1f, inner.width)) * total;

            var clipProp = _clipsProp.GetArrayElementAtIndex(index);
            var startProp = clipProp.FindPropertyRelative("startTime");
            var durProp = clipProp.FindPropertyRelative("duration");

            if (startProp == null || durProp == null) return;

            const float minDuration = 0.01f;

            if (_dragMode == DragMode.Move)
            {
                float newStart = _dragOriginalStart + deltaTime;
                newStart = Mathf.Max(0f, newStart);
                // ensure clip stays within timeline bounds
                newStart = Mathf.Min(newStart, Mathf.Max(0f, total - _dragOriginalDuration));
                startProp.floatValue = newStart;
            }
            else if (_dragMode == DragMode.ResizeLeft)
            {
                // robust left-resize: compute original end and clamp newStart so it never crosses end
                float originalStart = _dragOriginalStart;
                float originalDur = _dragOriginalDuration;
                float originalEnd = originalStart + originalDur;

                float newStart = originalStart + deltaTime;
                // Clamp newStart between 0 and (originalEnd - minDuration)
                newStart = Mathf.Clamp(newStart, 0f, originalEnd - minDuration);

                float newDuration = originalEnd - newStart;
                newDuration = Mathf.Max(minDuration, newDuration);

                startProp.floatValue = newStart;
                durProp.floatValue = newDuration;
            }
            else if (_dragMode == DragMode.ResizeRight)
            {
                // extend/shrink duration by deltaTime, but keep end <= total and duration >= minDuration
                float newDuration = _dragOriginalDuration + deltaTime;
                newDuration = Mathf.Max(minDuration, newDuration);
                newDuration = Mathf.Min(newDuration, Mathf.Max(minDuration, total - _dragOriginalStart));
                durProp.floatValue = newDuration;
            }

            // apply changes
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_sequencer);
            // update preview live if playing
            if (_previewSequence != null && _previewSequence.IsActive())
            {
                _sequencer.BuildSequence();
                _previewSequence = _sequenceField?.GetValue(_sequencer) as Sequence;
                // keep preview playing from same progress
                float totalNow = ComputeTotalDuration();
                float time = Mathf.Clamp01(_progress) * Mathf.Max(0.0001f, totalNow);

                _previewSequence.Goto(time, andPlay: _isPlaying);
            }
        }

        private void EndDrag()
        {
            _draggingIndex = -1;
            _dragMode = DragMode.None;
            // finalize undo snapshot
            Undo.FlushUndoRecordObjects();
        }

        private void DrawSelectedClipInspector()
        {
            if (_selectedIndex < 0 || _selectedIndex >= (_clipsProp.arraySize))
            {
                EditorGUILayout.HelpBox("Select a clip from the timeline to view/edit details.", MessageType.Info);
                return;
            }

            var elem = _clipsProp.GetArrayElementAtIndex(_selectedIndex);
            if (elem == null)
            {
                EditorGUILayout.HelpBox("Selected clip unavailable.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            //  EditorGUILayout.LabelField("Selected Clip", EditorStyles.boldLabel);

            // Show the serialized clip fields (this will include all public fields on the clip)
            EditorGUILayout.PropertyField(elem, true);
            var clipObj = _sequencer.Clips[_selectedIndex];
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Capture From"))
            {
                var mi = clipObj.GetType().GetMethod("CaptureFromState", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (mi != null) mi.Invoke(clipObj, null);
                else
                {
                    var f = clipObj.GetType().GetField("fromPosition");
                    var tf = clipObj.GetType().GetField("target");
                    if (f != null && tf != null)
                    {
                        var t = tf.GetValue(clipObj) as Transform;
                        if (t != null) f.SetValue(clipObj, t.position);
                    }
                }
                EditorUtility.SetDirty(_sequencer);
            }
            if (GUILayout.Button("Remove"))
            {
                _clipsProp.DeleteArrayElementAtIndex(_selectedIndex);
                _selectedIndex = -1;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_sequencer);
                EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawBottomControls()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Clip", GUILayout.Width(100)))
            {
                ShowAddMenu();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowAddMenu()
        {
            var menu = new GenericMenu();
            //Move
            menu.AddItem(new GUIContent("Position/Transform Position"), false, () => AddClip(typeof(TransformPositionClip)));
            menu.AddItem(new GUIContent("Position/Anchor Position"), false, () => AddClip(typeof(AnchorPositionClip)));
            //Scale 
            menu.AddItem(new GUIContent("Scale/Scale"), false, () => AddClip(typeof(ScaleClip)));
            //Rotation
            menu.AddItem(new GUIContent("Rotate/Rotate"), false, () => AddClip(typeof(RotateClip)));
            //Punch
            menu.AddItem(new GUIContent("Punch/Position"), false, () => AddClip(typeof(PunchPositionClip)));
            menu.AddItem(new GUIContent("Punch/Rotation"), false, () => AddClip(typeof(PunchRotationClip)));
            menu.AddItem(new GUIContent("Punch/Scale"), false, () => AddClip(typeof(PunchScaleClip)));
            menu.AddItem(new GUIContent("Punch/Anchor Position"), false, () => AddClip(typeof(PunchAnchorPosClip)));
            //Shake
            menu.AddItem(new GUIContent("Shake/Position"), false, () => AddClip(typeof(ShakePositionClip)));
            menu.AddItem(new GUIContent("Shake/Rotation"), false, () => AddClip(typeof(ShakeRotationClip)));
            menu.AddItem(new GUIContent("Shake/Scale"), false, () => AddClip(typeof(ShakeScaleClip)));
            //Blendable
            menu.AddItem(new GUIContent("Blendable/Scale"), false, () => AddClip(typeof(BlendableScaleClip)));
            menu.AddItem(new GUIContent("Blendable/Position"), false, () => AddClip(typeof(BlendablePositionClip)));
            menu.AddItem(new GUIContent("Blendable/Rotation"), false, () => AddClip(typeof(BlendableRotationClip)));
            menu.AddItem(new GUIContent("Blendable/Punch Rotation"), false, () => AddClip(typeof(BlendablePunchRotationClip)));
            menu.ShowAsContext();
        }

        private string GetTweenDisplayName(TweenClipBase clip)
        {
            if (clip == null) return "Unknown (No Clip)";

            //Clip Display Name
            string tweenType = clip switch
            {
                //Move
                TransformPositionClip => "Pos",
                AnchorPositionClip => "Anchor Pos",
                //Scale 
                ScaleClip => "Scale",
                //Rotation
                RotateClip => "Rotate",
                //Punch
                PunchPositionClip => "Punch Pos",
                PunchRotationClip => "Punch Rotation",
                PunchScaleClip => "Punch Scale",
                PunchAnchorPosClip => "Punch Anchor Pos",
                //Shake
                ShakePositionClip => "Shake Pos",
                ShakeRotationClip => "Shake Rotation",
                ShakeScaleClip => "Shake Scale",
                //Blendable
                BlendableScaleClip => "Blend Scale",
                BlendablePositionClip => "Blend Pos",
                BlendableRotationClip => "Blend Rotation",
                BlendablePunchRotationClip => "Blend Punch Rotation",
                _ => "Unknown"
            };

            // Target Name
            var target = clip.GetType().GetField("target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            string targetName = "<color=#FF4040>No Target</color>";
            if (target != null)
            {
                var targetValue = target.GetValue(clip);

                if (targetValue != null)
                {
                    RectTransform rt = targetValue as RectTransform;
                    if (rt != null)
                    {
                        targetName = rt.gameObject.name;
                    }
                    Transform t = targetValue as Transform;
                    if (t != null)
                    {
                        targetName = t.gameObject.name;
                    }
                }
            }

            return $"{tweenType} ({targetName})";
        }

        private Color LoadClipColor(TweenClipBase clip)
        {
            Color color = clip switch
            {
                // Position (Blue)
                TransformPositionClip => new Color(0.20f, 0.45f, 0.85f),   // Bright Blue
                AnchorPositionClip => new Color(0.45f, 0.70f, 1.00f),   // Sky Blue

                // Rotation (Orange)
                RotateClip => new Color(0.95f, 0.45f, 0.20f),               // Orange

                // Scale (Purple)
                ScaleClip => new Color(0.65f, 0.50f, 1.00f),                // Soft Purple

                // Punch (Green family, separated by tone)
                PunchPositionClip => new Color(0.10f, 0.70f, 0.35f),      // Emerald
                PunchRotationClip => new Color(0.00f, 0.80f, 0.55f),      // Mint
                PunchScaleClip => new Color(0.00f, 0.90f, 0.45f),         // Bright Green
                PunchAnchorPosClip => new Color(0.20f, 0.65f, 0.40f),      // Forest Green

                // Shake (Golds)
                ShakePositionClip => new Color(0.95f, 0.75f, 0.10f),      // Golden Yellow
                ShakeRotationClip => new Color(1.00f, 0.85f, 0.30f),      // Warm Yellow
                ShakeScaleClip => new Color(1.00f, 0.95f, 0.55f),        // Pale Gold

                // Blendable (Magenta family — high contrast)
                BlendableScaleClip => new Color(0.80f, 0.30f, 0.90f),      // Magenta
                BlendablePositionClip => new Color(1.00f, 0.60f, 1.00f),   // Light Pink
                BlendableRotationClip => new Color(0.90f, 0.70f, 1.00f),   // Lavender
                BlendablePunchRotationClip => new Color(0.60f, 0.20f, 0.70f),   // Deep Purple
                _ => Color.gray
            };

            return color;
        }

        private void AddClip(Type t)
        {
            int index = _clipsProp.arraySize;
            _clipsProp.InsertArrayElementAtIndex(index);
            var elem = _clipsProp.GetArrayElementAtIndex(index);
            var instance = Activator.CreateInstance(t);
            elem.managedReferenceValue = instance;
            serializedObject.ApplyModifiedProperties();
            _selectedIndex = -1;
        }

        private void StartPreview(bool playImmediately = true)
        {
            if (_sequencer == null) return;

            DOTweenEditorPreview.Stop();

            _sequencer.ApplyAllFromStates();
            _sequencer.Kill();
            _sequencer.BuildSequence();

            _previewSequence = _sequencer._sequence;
            if (_previewSequence == null)
            {
                Debug.LogWarning("No sequence built - check clips configuration.");
                return;
            }

            if (_autoReplay)
            {
                _previewSequence.SetLoops(10, LoopType.Restart);
            }

            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            DOTweenEditorPreview.PrepareTweenForPreview(_previewSequence, true, true, true);

            DOTweenEditorPreview.Start(() => { SceneView.RepaintAll(); });

            if (playImmediately)
                _previewSequence.Restart();

            _isPlaying = true;
        }

        private void PausePreview()
        {
            if (_previewSequence != null && _previewSequence.IsActive())
            {
                _previewSequence.Pause();
                _isPlaying = false;
            }
        }

        private void StopPreviewAndCleanup()
        {
            _sequencer.Kill();
            if (_previewSequence != null)
            {
                _previewSequence.Complete(true);
                _previewSequence.Kill();
                _previewSequence = null;
            }

            DOTweenEditorPreview.Stop();

            _isPlaying = false;

            EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene);
            SceneView.RepaintAll();
        }

        private void ScrubToProgress(float progress)
        {
            if (_sequencer.Clips.Length > 0)
            {
                if (_previewSequence == null || _sequencer == null)
                {
                    _sequencer.ApplyAllFromStates();
                    _sequencer.BuildSequence();
                    _previewSequence = _sequencer._sequence;
                    DOTweenEditorPreview.PrepareTweenForPreview(_previewSequence, true, true, false);
                }
                float normalized = Mathf.Clamp01(progress);
                float time = normalized * _previewSequence.Duration();
                _previewSequence.Goto(time, andPlay: false);
                EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene);
                SceneView.RepaintAll();
            }
        }

        private float ComputeTotalDuration()
        {
            // if user provided a positive timelineDuration, prefer it (user requested timelineDuration-driven length)
            if (_sequencer != null && _sequencer.timelineDuration > 0f)
            {
                return _sequencer.timelineDuration;
            }

            // fallback: compute from clips
            float max = 0f;
            var clipsRuntime = _sequencer.Clips;
            if (clipsRuntime == null) return 0f;
            foreach (var c in clipsRuntime)
            {
                if (c == null) continue;
                max = Mathf.Max(max, c.startTime + Mathf.Max(0f, c.duration));
            }
            return max;
        }

    }
}
#endif
