// Assets/Editor/TweenSequencerEditor.cs
#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor.SceneManagement;
using DG.Tweening;
using DG.DOTweenEditor;

[CustomEditor(typeof(TweenSequencer))]
public class TweenSequencerEditor : Editor
{
    private SerializedProperty _clipsProp;
    private TweenSequencer _sequencer;
    private FieldInfo _sequenceField;

    // preview
    private Sequence _previewSequence;
    private bool _isPreviewing = false;
    private float _progress = 0f;
    private bool _autoReplay = false;
    private bool _isPlaying = false;

    // timeline visuals
    private const float TIMELINE_HEIGHT = 120f;
    private const float TIMELINE_LEFT_MARGIN = 20f;
    private const float TIMELINE_RIGHT_MARGIN = 20f;
    private const float CLIP_BAR_HEIGHT = 20f;
    private const float CLIP_BAR_PADDING = 6f;
    private const float EDGE_HANDLE_WIDTH = 6f; // px for left/right resize handles

    // selection + dragging
    private int _selectedIndex = -1;
    private int _draggingIndex = -1;
    private DragMode _dragMode = DragMode.None;
    private float _dragStartMouseX;
    private float _dragOriginalStart;
    private float _dragOriginalDuration;

    private enum DragMode { None, Move, ResizeLeft, ResizeRight }

    private void OnEnable()
    {
        _sequencer = (TweenSequencer)target;
        _clipsProp = serializedObject.FindProperty("clips");
        _sequenceField = typeof(TweenSequencer).GetField("_sequence", BindingFlags.NonPublic | BindingFlags.Instance);

        if (_selectedIndex >= _clipsProp.arraySize) _selectedIndex = -1;
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
            StopPreviewAndCleanup();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawTopToolbar();
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Timeline", EditorStyles.boldLabel);
        DrawTimelineArea();

        EditorGUILayout.Space(6);
        DrawSelectedClipInspector();

        EditorGUILayout.Space(8);
        DrawBottomControls();

        // Progress slider
        EditorGUI.BeginChangeCheck();
        _progress = EditorGUILayout.Slider("Progress", _progress, 0f, 1f);
        if (EditorGUI.EndChangeCheck() && !_isPlaying)
        {
            ScrubToProgress(_progress);
        }

        serializedObject.ApplyModifiedProperties();
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

        Rect timelineRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, TIMELINE_HEIGHT);
        GUI.Box(timelineRect, GUIContent.none);

        // labels
        Rect leftLabelRect = new Rect(timelineRect.x + 4, timelineRect.y + 4, TIMELINE_LEFT_MARGIN - 8, 20);
        EditorGUI.LabelField(leftLabelRect, "0s");
        Rect rightLabelRect = new Rect(timelineRect.x + timelineRect.width - TIMELINE_RIGHT_MARGIN - 60, timelineRect.y + 4, 60, 20);
        EditorGUI.LabelField(rightLabelRect, $"{total:0.00}s");

        Rect inner = new Rect(timelineRect.x + TIMELINE_LEFT_MARGIN, timelineRect.y + 28,
            timelineRect.width - TIMELINE_LEFT_MARGIN - TIMELINE_RIGHT_MARGIN, TIMELINE_HEIGHT - 44);

        EditorGUI.DrawRect(inner, new Color(0.11f, 0.11f, 0.11f, 1f));

        var clipsRuntime = _sequencer.Clips;
        if (clipsRuntime != null)
        {
            for (int i = 0; i < clipsRuntime.Length; i++)
            {
                var clip = clipsRuntime[i];
                if (clip == null) continue;

                float start = clip.startTime;
                float dur = Mathf.Max(0.0001f, clip.duration);

                float x = inner.x + Mathf.Clamp01(start / total) * inner.width;
                float w = Mathf.Clamp01(dur / total) * inner.width;
                float y = inner.y + 6 + i * (CLIP_BAR_HEIGHT + CLIP_BAR_PADDING);

                if (y + CLIP_BAR_HEIGHT > inner.y + inner.height)
                {
                    y = inner.y + inner.height - CLIP_BAR_HEIGHT - 2;
                }

                Rect barRect = new Rect(x, y, Mathf.Max(6f, w), CLIP_BAR_HEIGHT);

                // main bar
                Color col = GetColorForClip(clip);
                EditorGUI.DrawRect(barRect, col);

                // edge handles
                Rect leftHandle = new Rect(barRect.x - EDGE_HANDLE_WIDTH / 2f, barRect.y, EDGE_HANDLE_WIDTH, CLIP_BAR_HEIGHT);
                Rect rightHandle = new Rect(barRect.xMax - EDGE_HANDLE_WIDTH / 2f, barRect.y, EDGE_HANDLE_WIDTH, CLIP_BAR_HEIGHT);
                EditorGUI.DrawRect(leftHandle, new Color(0, 0, 0, 0.35f));
                EditorGUI.DrawRect(rightHandle, new Color(0, 0, 0, 0.35f));

                // outline selected clip
                if (i == _selectedIndex)
                {
                    Handles.DrawSolidRectangleWithOutline(barRect, new Color(0, 0, 0, 0), Color.yellow);
                }

                // label
                Rect lblRect = new Rect(barRect.x + 4, barRect.y + 2, barRect.width - 8, 14);
                EditorGUI.LabelField(lblRect, clip.clipType.ToString(), EditorStyles.whiteLabel);

                // handle mouse events: prioritize handles then body
                Vector2 mouse = Event.current.mousePosition;

                // Resize left
                if (Event.current.type == EventType.MouseDown && leftHandle.Contains(mouse))
                {
                    BeginDrag(i, DragMode.ResizeLeft);
                    Event.current.Use();
                }
                // Resize right
                else if (Event.current.type == EventType.MouseDown && rightHandle.Contains(mouse))
                {
                    BeginDrag(i, DragMode.ResizeRight);
                    Event.current.Use();
                }
                // Move body
                else if (Event.current.type == EventType.MouseDown && barRect.Contains(mouse))
                {
                    BeginDrag(i, DragMode.Move);
                    // select clip
                    _selectedIndex = i;
                    // set progress to clip start
                    _progress = Mathf.Clamp01(start / total);
                    ScrubToProgress(_progress);
                    Event.current.Use();
                }

                // while dragging, update
                if (_draggingIndex == i && _dragMode != DragMode.None && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp))
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
            }
        }

        // scrub handle
        float scrubX = inner.x + Mathf.Clamp01(_progress) * inner.width;
        Rect scrubRect = new Rect(scrubX - 1, inner.y - 4, 2, inner.height + 8);
        EditorGUI.DrawRect(scrubRect, Color.yellow);

        // dragging scrub
        if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && inner.Contains(Event.current.mousePosition))
        {
            // If dragging a clip already, don't override
            if (_dragMode == DragMode.None)
            {
                float timeAtMouse = Mathf.Clamp01((Event.current.mousePosition.x - inner.x) / inner.width) * total;
                _progress = Mathf.Clamp01(total == 0 ? 0 : timeAtMouse / total);
                ScrubToProgress(_progress);
                Event.current.Use();
            }
        }
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

        if (_dragMode == DragMode.Move)
        {
            float newStart = _dragOriginalStart + deltaTime;
            newStart = Mathf.Max(0f, newStart);
            startProp.floatValue = newStart;
        }
        else if (_dragMode == DragMode.ResizeLeft)
        {
            float newStart = _dragOriginalStart + deltaTime;
            float newDuration = _dragOriginalDuration - deltaTime;
            // clamp
            if (newDuration < 0.01f)
            {
                newDuration = 0.01f;
                newStart = _dragOriginalStart + (_dragOriginalDuration - newDuration);
            }
            startProp.floatValue = Mathf.Max(0f, newStart);
            durProp.floatValue = newDuration;
        }
        else if (_dragMode == DragMode.ResizeRight)
        {
            float newDuration = _dragOriginalDuration + deltaTime;
            newDuration = Mathf.Max(0.01f, newDuration);
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
        EditorGUILayout.LabelField("Selected Clip", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(elem, true);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Capture From"))
        {
            var clipObj = _sequencer.Clips[_selectedIndex];
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
        menu.AddItem(new GUIContent("Scale"), false, () => AddClip(typeof(ScaleClip)));
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

        try { DOTweenEditorPreview.Stop(); } catch { }

        _sequencer.ApplyAllFromStates();
        _sequencer.KillAllClips();
        _sequencer.BuildSequence();

        _previewSequence = _sequenceField?.GetValue(_sequencer) as Sequence;
        if (_previewSequence == null)
        {
            Debug.LogWarning("No sequence built - check clips configuration.");
            return;
        }

        if (_autoReplay)
        {
            try { _previewSequence.SetLoops(-1, LoopType.Restart); } catch { }
        }
        else
        {
            try { _previewSequence.SetLoops(1, LoopType.Restart); } catch { }
        }

        try
        {
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            DOTweenEditorPreview.PrepareTweenForPreview(_previewSequence, true, true, true);
        }
        catch (Exception e)
        {
            Debug.LogWarning("DOTweenEditorPreview prepare failed: " + e.Message);
        }

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
        try { _previewSequence?.Kill(); } catch { }
        _previewSequence = null;

        try { DOTweenEditorPreview.Stop(); } catch { }

        _sequencer.KillAllClips();
        _sequencer.ApplyAllFromStates();

        _isPlaying = false;
        _isPreviewing = false;

        EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene);
        SceneView.RepaintAll();
    }

    private void ScrubToProgress(float progress)
    {
        if (_sequencer == null) return;

        float total = ComputeTotalDuration();
        _sequencer.BuildSequence();
        _previewSequence = _sequenceField?.GetValue(_sequencer) as Sequence;
        if (_previewSequence == null) return;

        float time = Mathf.Clamp01(progress) * Mathf.Max(0.0001f, total);
        _previewSequence.Goto(time, andPlay: false);

        EditorSceneManager.MarkSceneDirty(_sequencer.gameObject.scene);
        SceneView.RepaintAll();
    }

    private float ComputeTotalDuration()
    {
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

    private Color GetColorForClip(TweenClipBase clip)
    {
        switch (clip.clipType)
        {
            case TweenClipType.Move: return new Color(0.15f, 0.6f, 0.9f, 1f);
            case TweenClipType.Scale: return new Color(0.3f, 0.9f, 0.4f, 1f);
            case TweenClipType.Rotate: return new Color(0.9f, 0.6f, 0.15f, 1f);
            case TweenClipType.AnchorPos: return new Color(0.8f, 0.3f, 0.9f, 1f);
            case TweenClipType.Fade: return new Color(0.9f, 0.3f, 0.3f, 1f);
            default: return new Color(0.6f, 0.6f, 0.6f, 1f);
        }
    }
}
#endif
