#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using DG.Tweening;
using DG.DOTweenEditor;

[CustomEditor(typeof(TweenSequencer))]
public class TweenSequencerEditor : Editor
{
    public static TweenSequencerEditor Instance;

    private SerializedProperty _clipsProp;
    private SerializedProperty _timelineDurationProp;
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
        EditorGUILayout.Space(2);
        EditorGUILayout.Slider(_timelineDurationProp, minimumTimelineDuration, maximumTimelineDuration, new GUIContent("Duration (s)"));
        EditorGUILayout.Space(2);

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
        GUIStyle centeredBold = new GUIStyle(EditorStyles.boldLabel);
        centeredBold.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Timeline", centeredBold, GUILayout.ExpandWidth(true));
        DrawTimelineArea();
        DrawBottomControls();

        // Selected clip details
        EditorGUILayout.Space(6);
        DrawSelectedClipInspector();

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
                Rect lblRect = new Rect(barRect.x + 4, barRect.y + 2, Mathf.Max(16f, barRect.width - 28f), 14);
                string clipName = GetTweenDisplayName(clip);
                EditorGUI.LabelField(lblRect, clipName, labelStyle);

                // 3) Draw color stripe beneath the label (visual only; editing moved to inspector)
                Color clipColor = LoadClipColor(clip);
                float stripeHeight = 4f;
                Rect stripeRect = new Rect(lblRect.x + 8, lblRect.yMax + 3f, Mathf.Clamp(lblRect.width, 12f, barRect.width - 8f), stripeHeight);
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
                // clicking the bar selects / moves
                else if (Event.current.type == EventType.MouseDown && barRect.Contains(mouse))
                {
                    BeginDrag(i, DragMode.Move);
                    _selectedIndex = i;
                    // set progress normalized (0..1) to clip start
                    _progress = Mathf.Clamp01(start / total);
                    ScrubToProgress(_progress);
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

        // scrub handle (positioned by normalized _progress)
        float scrubX = inner.x + Mathf.Clamp01(_progress) * inner.width;
        Rect scrubRect = new Rect(scrubX - 1, inner.y - 4, 2, inner.height + 8);
        EditorGUI.DrawRect(scrubRect, Color.white);

        // dragging scrub -> set normalized progress based on mouse X
        if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && inner.Contains(Event.current.mousePosition))
        {
            // don't override if currently dragging a clip
            if (_dragMode == DragMode.None)
            {
                float normalizedAtMouse = Mathf.Clamp01((Event.current.mousePosition.x - inner.x) / inner.width);
                _progress = normalizedAtMouse;
                ScrubToProgress(_progress);
                Event.current.Use();
            }
        }
    }

    private string GetTweenDisplayName(TweenClipBase clip)
    {
        if (clip == null) return "Unknown (No Clip)";

        string tweenType = clip switch
        {
            TransformMoveClip => "Move",
            RectTransformMoveClip => "RectMove",
            ScaleClip => "Scale",
            PunchScaleClip => "PunchScale",
            BlendableScaleClip => "BlendScale",
            _ => "Unknown"
        };

        string targetName = clip switch
        {
            TransformMoveClip moveClip when moveClip.target != null => moveClip.target.gameObject.name,
            RectTransformMoveClip rectClip when rectClip.target != null => rectClip.target.gameObject.name,
            ScaleClip scaleClip when scaleClip.target != null => scaleClip.target.gameObject.name,
            PunchScaleClip punchClip when punchClip.target != null => punchClip.target.gameObject.name,
            BlendableScaleClip blendClip when blendClip.target != null => blendClip.target.gameObject.name,
            _ => "<color=#FF4040>No Target</color>"
        };
        return $"{tweenType} ({targetName})";
    }

    private Color LoadClipColor(TweenClipBase clip)
    {
        Color color = clip switch
        {
            TransformMoveClip => new Color(0.13f, 0.37f, 0.75f),   // Dark Blue
            RectTransformMoveClip => new Color(0.44f, 0.66f, 1.0f),   // Light Blue
            ScaleClip => new Color(0.55f, 0.42f, 0.95f),            // Soft Violet
            PunchScaleClip => new Color(1.00f, 0.58f, 0.20f),       // Orange
            BlendableScaleClip => new Color(0.33f, 0.87f, 0.47f),   // Lime Green
            _ => Color.gray
        };

        return color;
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
        menu.AddItem(new GUIContent("Move/Transform"), false, () => AddClip(typeof(TransformMoveClip)));
        menu.AddItem(new GUIContent("Move/RectTransform"), false, () => AddClip(typeof(RectTransformMoveClip)));
        menu.AddItem(new GUIContent("Scale/Scale"), false, () => AddClip(typeof(ScaleClip)));
        menu.AddItem(new GUIContent("Scale/PunchScale"), false, () => AddClip(typeof(PunchScaleClip)));
        menu.AddItem(new GUIContent("Scale/BlendableScale"), false, () => AddClip(typeof(BlendableScaleClip)));
        menu.ShowAsContext();
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
        _previewSequence.Complete(true);
        _previewSequence.Kill();
        _previewSequence = null;

        DOTweenEditorPreview.Stop();

        _isPlaying = false;

        EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene);
        SceneView.RepaintAll();
    }

    private void ScrubToProgress(float progress)
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
#endif
