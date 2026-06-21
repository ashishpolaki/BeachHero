#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

namespace BeachHero
{
    [CustomEditor(typeof(TweenAnimator))]
    public class TweenAnimatorEditor : Editor
    {
        public static TweenAnimatorEditor Instance;

        private SerializedProperty _clipsProp;
        private SerializedProperty _timelineDurationProp;
        private SerializedProperty _triggerEventsProp;
        private SerializedProperty _delayProp;
        private TweenAnimator _animator;
        private FieldInfo _sequenceField;

        // preview
        private TweenSequence _previewSequence;
        private float _progress = 0f;
        private bool _isPlaying = false;
        private bool _loopPreview = false;

        // timeline visuals
        private const float TIMELINE_HEIGHT = 50f;
        private const float TIMELINE_LEFT_MARGIN = 20f;
        private const float TIMELINE_RIGHT_MARGIN = 20f;
        private const float CLIP_BAR_HEIGHT = 30f;
        private const float CLIP_BAR_PADDING = 15f;
        private const float EDGE_HANDLE_WIDTH = 6f; // px for left/right resize handles
        private const float TRIGGER_AREA_HEIGHT = 18f;
        private const float TICK_AREA_HEIGHT = 28f;

        // Clip selection + dragging
        private int _selectedClipIndex = -1;
        private int _draggingIndex = -1;
        private DragMode _dragMode = DragMode.None;
        private float _dragClipStartMouseX;
        private float _dragClipOriginalStart;
        private float _dragClipOriginalDuration;

        // trigger events dragging state
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
            _animator = (TweenAnimator)target;
            _clipsProp = serializedObject.FindProperty("clips");
            _timelineDurationProp = serializedObject.FindProperty("timelineDuration");
            _triggerEventsProp = serializedObject.FindProperty("triggerEvents");
            _delayProp = serializedObject.FindProperty("delayFrames");
            _sequenceField = typeof(TweenAnimator).GetField("_sequence", BindingFlags.NonPublic | BindingFlags.Instance);

            if (_selectedClipIndex >= _clipsProp.arraySize) _selectedClipIndex = -1;
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
            UpdatePlaybackState();
            serializedObject.Update();
            HandleKeyboardShortcuts();

            // top toolbar
            EditorGUILayout.Space(6);
            DrawTopToolbar();
            EditorGUILayout.Space(4);

            // Progress and Duration,Delay sliders
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Slider(_timelineDurationProp, minimumTimelineDuration, maximumTimelineDuration, new GUIContent("Duration (s)"));
            EditorGUILayout.Slider(_delayProp, 0, 1, new GUIContent("Start Delay (frames)"));
            _progress = EditorGUILayout.Slider("Progress", _progress, 0f, 1f);
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("loopCount"));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("loopType"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                float newTimeline = _timelineDurationProp.floatValue;
                AdjustClipsToTimeline(newTimeline);
                ScrubToProgress(_progress);
                OnClipOrSequencerDataChanged();
            }

            // Timeline label
            DrawTimelineArea();
            DrawAddClipOrTriggerButtons();

            // Selected clip details
            EditorGUILayout.Space(3);
            if (_selectedTriggerIndex >= 0)
            {
                DrawSelectedTriggerInspector();
            }
            else
            {
                DrawSelectedClipInspector();
            }
            serializedObject.ApplyModifiedProperties();
        }
        private void UpdatePlaybackState()
        {
            if (!_isPlaying || _loopPreview)
            {
                return;
            }

            if(!_previewSequence.IsValid)
            {
                _isPlaying = false;

                Repaint();
                return;
            }

            float currentTime = (float)_previewSequence.Handle.Time;
            float duration = _previewSequence.Duration;
            if (currentTime >= duration)
            {
                _isPlaying = false;
                // snap to end (optional but cleaner)
                _previewSequence.SetTime(duration);
                _previewSequence.SetPlaybackSpeed(0);
                Repaint();
            }
        }
        private void AdjustClipsToTimeline(float newTimeline)
        {
            if (_clipsProp == null) return;

            for (int i = 0; i < _clipsProp.arraySize; i++)
            {
                var clipProp = _clipsProp.GetArrayElementAtIndex(i);
                if (clipProp == null) continue;

                var startProp = clipProp.FindPropertyRelative("startTime");
                var durationProp = clipProp.FindPropertyRelative("duration");

                if (startProp == null || durationProp == null) continue;

                float start = startProp.floatValue;
                float duration = durationProp.floatValue;

                float originalDuration = duration;
                float end = start + duration;

                if (end > newTimeline)
                {
                    float newEnd = newTimeline;

                    // PRIORITY: preserve duration by shifting start
                    float newStart = newEnd - originalDuration;

                    if (newStart < 0f)
                    {
                        newStart = 0f;
                        duration = newEnd - newStart; // shrink duration
                    }
                    else
                    {
                        duration = originalDuration;
                    }

                    startProp.floatValue = newStart;
                    durationProp.floatValue = Mathf.Max(0.01f, duration);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        public void OnClipOrSequencerDataChanged()
        {
            // Ensure serialized changes are applied (defensive)
            serializedObject.ApplyModifiedProperties();

            if (_animator == null) return;

            // Apply states and rebuild the actual runtime sequence
            _animator.ApplyAllFromStates();
            _animator.BuildSequence();

            // Pull the runtime sequence from the private field (you already use _sequenceField)
            // _previewSequence = (TweenSequence)_sequenceField?.GetValue(_sequencer);
            _previewSequence = _animator._sequence;

            if (_previewSequence.IsValid)
            {
                float totalNow = Mathf.Max(0.0001f, _previewSequence.Duration);
                float time = Mathf.Clamp01(_progress) * totalNow;
                _previewSequence.SetTime(time);
            }

            // Mark dirty so Unity will prompt to save scene/prefab changes
            EditorUtility.SetDirty(_animator);
            try { EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene); } catch { }
            ScrubToProgress(_progress);
            SceneView.RepaintAll();
        }

        private void DrawSelectedTriggerInspector()
        {
            if (_triggerEventsProp == null || !_triggerEventsProp.isArray)
            {
                EditorGUILayout.HelpBox("No trigger events present.", MessageType.Info);
                return;
            }

            if (_selectedTriggerIndex < 0 || _selectedTriggerIndex >= _triggerEventsProp.arraySize)
            {
                EditorGUILayout.HelpBox("Select a trigger from the timeline to view/edit details.", MessageType.Info);
                return;
            }

            var te = _triggerEventsProp.GetArrayElementAtIndex(_selectedTriggerIndex);
            if (te == null)
            {
                EditorGUILayout.HelpBox("Selected trigger unavailable.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField($"Trigger #{_selectedTriggerIndex}", EditorStyles.boldLabel);

            // Show the serialized trigger fields
            EditorGUILayout.PropertyField(te, true);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Go To"))
            {
                var timePercentProp = te.FindPropertyRelative("timePercent");
                if (timePercentProp != null)
                {
                    _progress = Mathf.Clamp01(timePercentProp.floatValue / 100f);
                    ScrubToProgress(_progress);
                }
            }

            if (GUILayout.Button("Remove"))
            {
                // delete the selected trigger and clear selection
                _triggerEventsProp.DeleteArrayElementAtIndex(_selectedTriggerIndex);
                _selectedTriggerIndex = -1;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_animator);
                try { EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene); } catch { }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #region Keyboard Shortcuts
        private void HandleKeyboardShortcuts()
        {
            var e = Event.current;
            if (e == null) return;

            bool mod = e.control || e.command; // Ctrl on Win, Cmd on macOS
            if (e.type == EventType.KeyDown && mod)
            {
                if (e.keyCode == KeyCode.D) // Ctrl/Cmd + D -> duplicate
                {
                    if (_selectedClipIndex >= 0)
                        DuplicateSelectedClip();
                    else if (_selectedTriggerIndex >= 0)
                        DuplicateSelectedTrigger();

                    e.Use();
                    GUI.FocusControl(null);
                }
                else if (e.keyCode == KeyCode.Delete) // Ctrl/Cmd + Delete -> delete
                {
                    if (_selectedClipIndex >= 0)
                        DeleteSelectedClip();
                    else if (_selectedTriggerIndex >= 0)
                        DeleteSelectedTrigger();

                    e.Use();
                    GUI.FocusControl(null);
                }
            }
        }

        private void DuplicateSelectedClip()
        {
            if (_clipsProp == null) return;
            if (_selectedClipIndex < 0 || _selectedClipIndex >= _clipsProp.arraySize) return;

            // record undo
            Undo.RegisterCompleteObjectUndo(_animator, "Duplicate Clip");

            var srcProp = _clipsProp.GetArrayElementAtIndex(_selectedClipIndex);
            object srcObj = srcProp.managedReferenceValue;

            int insertIndex = _selectedClipIndex + 1;

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
            EditorUtility.SetDirty(_animator);
            _selectedClipIndex = insertIndex;
            // mark scene dirty so user is prompted to save
            try { EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene); } catch { }
        }

        private void DeleteSelectedClip()
        {
            if (_clipsProp == null) return;
            if (_selectedClipIndex < 0 || _selectedClipIndex >= _clipsProp.arraySize) return;

            // confirm? (optional) — we perform deletion immediately
            Undo.RegisterCompleteObjectUndo(_animator, "Delete Clip");

            // Delete element (works for managedReference arrays as used)
            _clipsProp.DeleteArrayElementAtIndex(_selectedClipIndex);
            serializedObject.ApplyModifiedProperties();

            // clamp selection
            int newIndex = Mathf.Clamp(_selectedClipIndex, 0, _clipsProp.arraySize - 1);
            _selectedClipIndex = (_clipsProp.arraySize == 0) ? -1 : newIndex;

            EditorUtility.SetDirty(_animator);
            try { EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene); } catch { }
        }

        private void DeleteSelectedTrigger()
        {
            if (_triggerEventsProp == null || _selectedTriggerIndex < 0 || _selectedTriggerIndex >= _triggerEventsProp.arraySize)
                return;

            Undo.RegisterCompleteObjectUndo(_animator, "Delete Trigger");

            _triggerEventsProp.DeleteArrayElementAtIndex(_selectedTriggerIndex);
            serializedObject.ApplyModifiedProperties();

            // clamp selection
            int newIndex = Mathf.Clamp(_selectedTriggerIndex, 0, _triggerEventsProp.arraySize - 1);
            _selectedTriggerIndex = (_triggerEventsProp.arraySize == 0) ? -1 : newIndex;

            EditorUtility.SetDirty(_animator);
            try { EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene); } catch { }
        }

        private void DuplicateSelectedTrigger()
        {
            if (_triggerEventsProp == null || _selectedTriggerIndex < 0 || _selectedTriggerIndex >= _triggerEventsProp.arraySize)
                return;

            Undo.RegisterCompleteObjectUndo(_animator, "Duplicate Trigger");

            var srcProp = _triggerEventsProp.GetArrayElementAtIndex(_selectedTriggerIndex);
            int insertIndex = _selectedTriggerIndex + 1;

            _triggerEventsProp.InsertArrayElementAtIndex(insertIndex);
            var newElem = _triggerEventsProp.GetArrayElementAtIndex(insertIndex);

            // Copy all serialized fields
            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(srcProp.managedReferenceValue), newElem.managedReferenceValue);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_animator);

            _selectedTriggerIndex = insertIndex;

            try { EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene); } catch { }
        }
        #endregion

        private void DrawTopToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (!_isPlaying)
            {
                if (GUILayout.Button("Play", GUILayout.Width(64)))
                {
                    StartPreview();
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

            // Loop toggle button 
            Color prevBg = GUI.backgroundColor;
            if (_loopPreview)
            {
                GUI.backgroundColor = new Color(0.32f, 0.72f, 0.32f, 1f);  // soft green tint for active loop
            }
            bool newLoop = GUILayout.Toggle(_loopPreview, "Loop", "Button", GUILayout.Width(64));
            GUI.backgroundColor = prevBg; // restore background color immediately so other UI unaffected
            if (newLoop != _loopPreview)
            {
                _loopPreview = newLoop;
            }
            _animator.IsLoop = _loopPreview;

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

            int sequencerClipsLength = _animator.Clips != null ? _animator.Clips.Length : 1;
            float timelineHeight = TIMELINE_HEIGHT + ((CLIP_BAR_HEIGHT + CLIP_BAR_PADDING) * sequencerClipsLength)
                                   + TICK_AREA_HEIGHT + TRIGGER_AREA_HEIGHT;
            Rect timelineRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, timelineHeight - 30);
            GUI.Box(timelineRect, GUIContent.none);
            Handles.DrawSolidRectangleWithOutline(timelineRect, new Color(0, 0, 0, 0), Color.black);

            // inner usable area (full width minus margins). We'll split it into:
            //  - tickArea (top)
            //  - triggerArea (below ticks)
            //  - clipArea (below triggers, contains clip bars)
            Rect innerBase = new Rect(
                timelineRect.x + TIMELINE_LEFT_MARGIN,
                timelineRect.y,
                timelineRect.width - TIMELINE_LEFT_MARGIN - TIMELINE_RIGHT_MARGIN,
                timelineHeight - 30 // keep a small bottom padding
            );

            // Clamp minimum width/height just in case
            if (innerBase.width < 10f) innerBase.width = 10f;
            if (innerBase.height < (TICK_AREA_HEIGHT + TRIGGER_AREA_HEIGHT + 10f)) innerBase.height = TICK_AREA_HEIGHT + TRIGGER_AREA_HEIGHT + 10f;

            // Define sub-areas (ticks on top, triggers below ticks)
            Rect tickArea = new Rect(innerBase.x, innerBase.y, innerBase.width, TICK_AREA_HEIGHT);
            Rect triggerArea = new Rect(innerBase.x, tickArea.yMax + 2f, innerBase.width, TRIGGER_AREA_HEIGHT);
            Rect clipArea = new Rect(innerBase.x, triggerArea.yMax + 2f, innerBase.width, innerBase.yMax - (triggerArea.yMax + 4f) - 2f);

            // background for clips
            EditorGUI.DrawRect(clipArea, new Color(0.3f, 0.3f, 0.3f, 1));
            //Solid Rectangle with outline
            Handles.DrawSolidRectangleWithOutline(clipArea, new Color(0, 0, 0, 0), Color.black);

            // Tick spacing (0.1s steps). If total is not an exact multiple of step, we ceil and clamp.
            float step = 0.1f;
            int steps = Mathf.CeilToInt(total / step);

            float minLabelSpacing = 15f; // pixels
            float lastLabelX = -minLabelSpacing;

            for (int i = 0; i <= steps; i++)
            {
                float time = i * step;
                if (time > total) time = total;

                float normalized = Mathf.Clamp01(time / total);
                float x = innerBase.x + normalized * innerBase.width;

                // draw tick
                bool isWholeSecond = Mathf.Abs(time - Mathf.Round(time)) < 0.0001f;
                float tickHeight = isWholeSecond ? 9f : 6f;
                Rect tick = new Rect(x - 0.5f, tickArea.y + 2f, 1f, tickHeight);
                EditorGUI.DrawRect(tick, new Color(0.7f, 0.7f, 0.7f, 0.6f));

                // draw label only if far enough from last label
                if (x - lastLabelX >= minLabelSpacing)
                {
                    string label = $"{time:0.##}";
                    Vector2 size = EditorStyles.miniLabel.CalcSize(new GUIContent(label));
                    Rect lbl = new Rect(x - size.x * 0.5f, tick.yMax + 2f, size.x, size.y);
                    EditorGUI.LabelField(lbl, label, EditorStyles.miniLabel);

                    lastLabelX = x; // update last drawn label position
                }
            }

            // draw tween clips into clipArea (clips appear below triggers now)
            var clipsRuntime = _animator.Clips;
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

                    float x = clipArea.x + Mathf.Clamp01(start / total) * clipArea.width;
                    float w = Mathf.Clamp01(dur / total) * clipArea.width;
                    float y = clipArea.y + 6 + i * (CLIP_BAR_HEIGHT + CLIP_BAR_PADDING);

                    if (y + CLIP_BAR_HEIGHT > clipArea.y + clipArea.height)
                    {
                        y = clipArea.y + clipArea.height - CLIP_BAR_HEIGHT - 2;
                    }

                    Rect barRect = new Rect(x, y + 8, Mathf.Max(6f, w), CLIP_BAR_HEIGHT);

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
                        GUI.FocusControl(null);
                        EditorGUIUtility.editingTextField = false;
                        serializedObject.ApplyModifiedProperties();

                        BeginDrag(i, DragMode.Move);

                        _selectedClipIndex = i;
                        _selectedTriggerIndex = -1;

                        Event.current.Use();
                    }

                    // dragging updates
                    if (_draggingIndex == i && _dragMode != DragMode.None &&
                        (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp))
                    {
                        if (Event.current.type == EventType.MouseDrag)
                        {
                            UpdateDrag(i, clipArea, total);
                            Event.current.Use();
                            OnClipOrSequencerDataChanged();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            EndDrag();
                            Event.current.Use();
                        }
                    }

                    // 6) Selected highlight (white outline when selected)
                    if (i == _selectedClipIndex)
                    {
                        Handles.DrawSolidRectangleWithOutline(barRect, new Color(0, 0, 0, 0), Color.white);
                    }
                }
            }

            // --- Draw trigger event markers in triggerArea (draggable, show time in seconds) ---
            if (_triggerEventsProp != null && _triggerEventsProp.isArray)
            {
                float totalDuration = Mathf.Max(0.0001f, ComputeTotalDuration()); // used to show seconds

                // end any trigger drag on mouse up
                if (Event.current.type == EventType.MouseUp && _draggingTriggerIndex != -1)
                {
                    _draggingTriggerIndex = -1;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_animator);
                    try { EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene); } catch { }
                    Event.current.Use();
                }

                for (int i = 0; i < _triggerEventsProp.arraySize; i++)
                {
                    var te = _triggerEventsProp.GetArrayElementAtIndex(i);
                    if (te == null) continue;
                    var timePercentProp = te.FindPropertyRelative("timePercent");
                    float percent = (timePercentProp != null) ? timePercentProp.floatValue : 0f;

                    float normalized = Mathf.Clamp01(percent / 100f);
                    float x = triggerArea.x + normalized * triggerArea.width;

                    // diamond shape centered in triggerArea
                    float markerCenterY = triggerArea.center.y;
                    float half = 6f;
                    Vector3[] diamond = new Vector3[]
                    {
                        new Vector3(x, markerCenterY - half),
                        new Vector3(x - half, markerCenterY),
                        new Vector3(x, markerCenterY + half),
                        new Vector3(x + half, markerCenterY)
                    };

                    Rect markerRect = new Rect(x - half - 2f, markerCenterY - half - 2f, half * 2f + 4f, half * 2f + 4f);

                    // draw guide line if the trigger is selected or being dragged
                    if (i == _draggingTriggerIndex)
                    {
                        Handles.color = new Color(0.95f, 0.55f, 0.15f, 0.6f);
                        Handles.DrawLine(new Vector3(x, markerCenterY + half + 2f), new Vector3(x, clipArea.y + clipArea.height));
                    }
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
                        float deltaNorm = (mouseDelta / Mathf.Max(1f, triggerArea.width));
                        float newNorm = Mathf.Clamp01((_triggerOriginalPercent / 100f) + deltaNorm);
                        float newPercent = newNorm * 100f;
                        if (timePercentProp != null)
                        {
                            timePercentProp.floatValue = newPercent;
                            serializedObject.ApplyModifiedProperties();
                            percent = newPercent;
                            normalized = newNorm;
                            seconds = normalized * totalDuration;
                            x = triggerArea.x + normalized * triggerArea.width;
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
                        float tipY = markerCenterY - half - tipSize.y - 0.5f; // 6px gap above marker

                        // clamp to inner bounds so it doesn't overflow the timeline rect
                        tipX = Mathf.Clamp(tipX, innerBase.x + 2f, innerBase.x + innerBase.width - tipSize.x - 2f);
                        tipY = Mathf.Max(tipY, timelineRect.y - 100f); // don't go too far above

                        Rect tipRect = new Rect(tipX, tipY, tipSize.x, tipSize.y);
                        GUIStyle tipStyle = new GUIStyle(EditorStyles.miniLabel);
                        tipStyle.normal.textColor = Color.cyan;
                        GUI.Label(tipRect, tipText, tipStyle);

                        EditorGUIUtility.AddCursorRect(markerRect, MouseCursor.SlideArrow);

                        // on left mouse down over the marker: either start drag or select
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            GUI.FocusControl(null);
                            EditorGUIUtility.editingTextField = false;
                            serializedObject.ApplyModifiedProperties();
                            if (_draggingTriggerIndex == -1)
                            {
                                _draggingTriggerIndex = i;
                                _selectedTriggerIndex = i;
                                _selectedClipIndex = -1; // deselect any clip
                                _triggerDragStartMouseX = Event.current.mousePosition.x;
                                _triggerOriginalPercent = percent;

                                if (te != null) te.isExpanded = true;
                                serializedObject.ApplyModifiedProperties();

                                Selection.activeObject = _animator;
                                EditorGUIUtility.PingObject(_animator);

                                Event.current.Use();
                            }
                        }
                    }

                    // optional focused outline when expanded
                    if (te.isExpanded)
                    {
                        Handles.DrawAAPolyLine(3f, new Vector3[] { diamond[0], diamond[1], diamond[2], diamond[3], diamond[0] });
                    }
                }
            }

            // scrub handle (positioned by normalized _progress)
            float scrubX = innerBase.x + Mathf.Clamp01(_progress) * innerBase.width;
            Rect scrubRect = new Rect(scrubX - 1, clipArea.y - 4, 2, clipArea.height + 8);
            EditorGUI.DrawRect(scrubRect, Color.white);

            // dragging scrub -> set normalized progress based on mouse X
            if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && innerBase.Contains(Event.current.mousePosition))
            {
                // don't override if currently dragging a clip
                if (_dragMode == DragMode.None)
                {
                    float normalizedAtMouse = Mathf.Clamp01((Event.current.mousePosition.x - innerBase.x) / innerBase.width);
                    _progress = normalizedAtMouse;
                    ScrubToProgress(_progress);
                    Event.current.Use();
                }
            }
        }

        private bool IsPointerOverTweenClipOrTriggerEvent(Vector2 mousePos, Rect inner, float total)
        {
            // check clips
            var clipsRuntime = _animator.Clips;
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
            _dragClipStartMouseX = Event.current.mousePosition.x;

            var clipProp = _clipsProp.GetArrayElementAtIndex(index);
            var startProp = clipProp.FindPropertyRelative("startTime");
            var durProp = clipProp.FindPropertyRelative("duration");

            _dragClipOriginalStart = startProp != null ? startProp.floatValue : 0f;
            _dragClipOriginalDuration = durProp != null ? durProp.floatValue : 0.1f;

            // capture undo
            Undo.RecordObject(_animator, "Drag Clip");
        }

        private void UpdateDrag(int index, Rect inner, float total)
        {
            if (_clipsProp == null) return;
            if (index < 0 || index >= _clipsProp.arraySize) return;

            float mouseDelta = Event.current.mousePosition.x - _dragClipStartMouseX;
            float deltaTime = (mouseDelta / Mathf.Max(1f, inner.width)) * total;

            var clipProp = _clipsProp.GetArrayElementAtIndex(index);
            var startProp = clipProp.FindPropertyRelative("startTime");
            var durProp = clipProp.FindPropertyRelative("duration");

            if (startProp == null || durProp == null) return;

            const float minDuration = 0.01f;

            if (_dragMode == DragMode.Move)
            {
                float newStart = _dragClipOriginalStart + deltaTime;
                newStart = Mathf.Max(0f, newStart);
                // ensure clip stays within timeline bounds
                newStart = Mathf.Min(newStart, Mathf.Max(0f, total - _dragClipOriginalDuration));
                startProp.floatValue = newStart;
            }
            else if (_dragMode == DragMode.ResizeLeft)
            {
                // robust left-resize: compute original end and clamp newStart so it never crosses end
                float originalStart = _dragClipOriginalStart;
                float originalDur = _dragClipOriginalDuration;
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
                float newDuration = _dragClipOriginalDuration + deltaTime;
                newDuration = Mathf.Max(minDuration, newDuration);
                newDuration = Mathf.Min(newDuration, Mathf.Max(minDuration, total - _dragClipOriginalStart));
                durProp.floatValue = newDuration;
            }

            // apply changes
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_animator);
            // update preview live if playing

            //if (_previewSequence != null && _previewSequence.IsActive())
            //{
            //    _sequencer.BuildSequence();
            //    _previewSequence = _sequenceField?.GetValue(_sequencer) as Sequence;
            //    // keep preview playing from same progress
            //    float totalNow = ComputeTotalDuration();
            //    float time = Mathf.Clamp01(_progress) * Mathf.Max(0.0001f, totalNow);

            //    _previewSequence.Goto(time, andPlay: _isPlaying);
            //}
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
            if (_selectedClipIndex < 0 || _selectedClipIndex >= (_clipsProp.arraySize))
            {
                EditorGUILayout.HelpBox("Select a clip from the timeline to view/edit details.", MessageType.Info);
                return;
            }

            var elem = _clipsProp.GetArrayElementAtIndex(_selectedClipIndex);
            if (elem == null)
            {
                EditorGUILayout.HelpBox("Selected clip unavailable.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            //  EditorGUILayout.LabelField("Selected Clip", EditorStyles.boldLabel);

            // Show the serialized clip fields (this will include all public fields on the clip)
            EditorGUILayout.PropertyField(elem, true);
            var clipObj = _animator.Clips[_selectedClipIndex];
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
                EditorUtility.SetDirty(_animator);
            }
            if (GUILayout.Button("Remove"))
            {
                _clipsProp.DeleteArrayElementAtIndex(_selectedClipIndex);
                _selectedClipIndex = -1;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_animator);
                EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawAddClipOrTriggerButtons()
        {
            EditorGUILayout.BeginHorizontal();

            // Add padding/margin on sides
            GUILayout.Space(6);

            float buttonSpacing = 6f;
            float totalAvailable = EditorGUIUtility.currentViewWidth - 24f; // account for padding
            float buttonWidth = (totalAvailable - buttonSpacing) / 2f;

            // --- Add Tween Clip Button ---
            if (GUILayout.Button("Add Tween Clip", GUILayout.Width(buttonWidth)))
            {
                ShowAddMenu();
            }

            GUILayout.Space(buttonSpacing);

            // --- Add Trigger Event Button ---
            if (GUILayout.Button("Add Trigger Event", GUILayout.Width(buttonWidth)))
            {
                int idx = _triggerEventsProp.arraySize;
                _triggerEventsProp.InsertArrayElementAtIndex(idx);
                var newElem = _triggerEventsProp.GetArrayElementAtIndex(idx);

                // Initialize timePercent to current scrub position
                var timePercentProp = newElem.FindPropertyRelative("timePercent");
                if (timePercentProp != null)
                    timePercentProp.floatValue = Mathf.Clamp01(_progress) * 100f;

                newElem.isExpanded = true;

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_animator);
                try { EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene); } catch { }

                // select the newly added trigger
                _selectedTriggerIndex = idx;
                _selectedClipIndex = -1;
            }

            GUILayout.Space(6);
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
            menu.AddItem(new GUIContent("Punch/Scale"), false, () => AddClip(typeof(PunchScaleClip)));
            //Shake
            menu.AddItem(new GUIContent("Shake/Position"), false, () => AddClip(typeof(ShakePositionClip)));
            menu.AddItem(new GUIContent("Shake/Scale"), false, () => AddClip(typeof(ShakeScaleClip)));
            //Image
            menu.AddItem(new GUIContent("Image/Fade"), false, () => AddClip(typeof(ImageFadeClip)));
            menu.AddItem(new GUIContent("Image/Fill Amount"), false, () => AddClip(typeof(ImageFillAmountClip)));
            menu.AddItem(new GUIContent("Image/Gradient Color"), false, () => AddClip(typeof(ImageGradientColorClip)));
            //CanvasGroup
            menu.AddItem(new GUIContent("CanvasGroup/Fade"), false, () => AddClip(typeof(CanvasGroupFadeClip)));
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
                PunchScaleClip => "Punch Scale",
                //Shake
                ShakePositionClip => "Shake Pos",
                ShakeScaleClip => "Shake Scale",
                //Image
                ImageFadeClip => "Image Fade",
                ImageFillAmountClip => "Image Fill",
                ImageGradientColorClip => "Image Gradient",
                //CanvasGroup
                CanvasGroupFadeClip => "CanvasGroup Fade",
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
                    CanvasGroup cg = targetValue as CanvasGroup;
                    if (cg != null)
                    {
                        targetName = cg.gameObject.name;
                    }
                    Image img = targetValue as Image;
                    if (img != null)
                    {
                        targetName = img.gameObject.name;
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
                                                                          // Mint
                PunchScaleClip => new Color(0.00f, 0.90f, 0.45f),         // Bright Green
                                                                          // Forest Green

                // Shake (Golds)
                ShakePositionClip => new Color(0.95f, 0.75f, 0.10f),      // Golden Yellow
                ShakeScaleClip => new Color(1.00f, 0.95f, 0.55f),        // Pale Gold

                // Image 
                ImageFadeClip => new Color(1.00f, 0.75f, 0.70f),      // Peach Tint
                ImageFillAmountClip => new Color(1.00f, 0.65f, 0.85f),     // Light Rose
                ImageGradientColorClip => new Color(1.00f, 0.55f, 0.75f), // Pinkish

                // CanvasGroup  
                CanvasGroupFadeClip => new Color(0.55f, 0.85f, 0.95f), // Soft Cyan Mist

                // misc
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
            _selectedClipIndex = -1;
        }

        private void StartPreview()
        {
            if (_animator == null) return;

            if (_previewSequence.IsActive)
            {
                _previewSequence.Cancel();
            }
            _animator.ApplyAllFromStates();
            _animator.Kill();
            _animator.BuildSequence();
            _previewSequence = _animator._sequence;

            if (!_previewSequence.IsValid)
            {
                DebugUtils.LogWarning("No sequence built - check clips configuration.");
                return;
            }

            _animator.Play();
            _isPlaying = true;
        }

        private void PausePreview()
        {
            if (_previewSequence.IsPlaying)
            {
                _previewSequence.SetPlaybackSpeed(0);
            }
            _isPlaying = false;
        }

        private void StopPreviewAndCleanup()
        {
            if (_previewSequence.IsActive)
            {
                _previewSequence.Cancel();
            }
            if (_animator != null)
            {
                _animator.Kill();
            }

            _isPlaying = false;

            if (_animator != null)
            {
                EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene);
            }
            SceneView.RepaintAll();
        }

        private void ScrubToProgress(float progress)
        {
            if (_animator.Clips.Length > 0)
            {
                if (!_animator.IsActive)
                {
                    _animator.ApplyAllFromStates();
                    _animator.BuildSequence();
                    _previewSequence = _animator._sequence;
                }
                float normalized = Mathf.Clamp01(progress);
                float time = normalized * _animator.Duration;
                _previewSequence.SetPlaybackSpeed(0);
                _previewSequence.SetTime(time);
                EditorSceneManager.MarkSceneDirty(_animator.gameObject.scene);
                SceneView.RepaintAll();
            }
        }

        private float ComputeTotalDuration()
        {
            // if user provided a positive timelineDuration, prefer it (user requested timelineDuration-driven length)
            if (_animator != null && _animator.timelineDuration > 0f)
            {
                return _animator.timelineDuration;
            }

            // fallback: compute from clips
            float max = 0f;
            var clipsRuntime = _animator.Clips;
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
