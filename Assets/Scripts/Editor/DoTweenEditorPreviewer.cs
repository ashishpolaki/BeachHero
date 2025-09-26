#if UNITY_EDITOR
using UnityEngine;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BeachHero
{
    [ExecuteInEditMode]
    public class DoTweenEditorPreviewer : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float progress = 0f;

        // parameters for the tween - editable in inspector
        public Vector3 fromScale = Vector3.one;
        public Vector3 toScale = Vector3.one * 1.5f;
        public float duration = 1f;
        public Ease ease = Ease.Linear;

        // Optional runtime API to set progress (used by editor)
        public void SetProgress(float t)
        {
            progress = Mathf.Clamp01(t);
            // at runtime you could optionally create and scrub a tween, but keep that separate
        }
    }

    #region Editor
    [CustomEditor(typeof(DoTweenEditorPreviewer))]
    public class DoTweenEditorPreviewerEditor : Editor
    {
        private DoTweenEditorPreviewer editorPreviewer;
        private Tween previewTween;
        private bool isPreviewing = false;

        void OnEnable()
        {
            editorPreviewer = (DoTweenEditorPreviewer)target;
            // ensure any previous editor preview is stopped
            try { DOTweenEditorPreview.Stop(); } catch { }
        }

        void OnDisable()
        {
            StopPreviewAndCleanup();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();

            // Draw all other properties, except "m_Script"
            serializedObject.Update();
            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                if (prop.name == "m_Script") continue; // skip script field
                EditorGUILayout.PropertyField(prop, true);
                enterChildren = false;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editorPreviewer, "Change progress");
                ScrubToProgress(editorPreviewer.progress);
                EditorUtility.SetDirty(editorPreviewer);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (!isPreviewing)
            {
                if (GUILayout.Button("Start Preview"))
                    StartPreview();
            }
            else
            {
                if (GUILayout.Button("Stop Preview"))
                    StopPreviewAndCleanup();
            }

            if (GUILayout.Button("Reset (to from)"))
            {
                Undo.RecordObject(editorPreviewer, "Reset preview");
                ApplyInstantFrom();
                EditorUtility.SetDirty(editorPreviewer);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Snap to To (complete)"))
            {
                CompleteAndSnap();
            }
        }

        void StartPreview()
        {
            if (editorPreviewer == null) return;

            // Cleanup any existing preview tween
            if (previewTween != null) { previewTween.Kill(); previewTween = null; }

            // set object to 'from' state first
            editorPreviewer.transform.localScale = editorPreviewer.fromScale;

            // create tween (non-looping) and keep it (autoKill=false)
            previewTween = editorPreviewer.transform.DOScale(editorPreviewer.toScale, editorPreviewer.duration)
                .SetEase(editorPreviewer.ease)
                .SetAutoKill(false)
                .Pause(); // don't play via runtime loop

            // Prepare tween for editor preview and start DOTweenEditorPreview loop
            try
            {
                DOTween.Init(false, true, LogBehaviour.ErrorsOnly);

                // Use the overload for passing the Tween (some versions)
                DOTweenEditorPreview.PrepareTweenForPreview(previewTween, true, true, true);
            }
            catch
            {
                Debug.LogError("Could not prepare tween for editor preview");
            }

            DOTweenEditorPreview.Start(() => { UnityEditor.SceneView.RepaintAll(); });
            isPreviewing = true;
        }

        void StopPreviewAndCleanup()
        {
            if (previewTween != null)
            {
                // kill the specific tween
                previewTween.Kill();
                previewTween = null;
            }

            // Stop DOTween editor preview loop
            try { DOTweenEditorPreview.Stop(); } catch { }

            // Optionally restore to 'from' state (you can change to snap-to-end instead)
            ApplyInstantFrom();

            isPreviewing = false;

            // ensure scene shows changes
            EditorSceneManager.MarkSceneDirty(editorPreviewer.gameObject.scene);
            UnityEditor.SceneView.RepaintAll();
        }

        void ScrubToProgress(float p)
        {
            if (previewTween == null)
            {
                // create non-playing tween for scrubbing (autoKill=false, paused)
                previewTween = editorPreviewer.transform.DOScale(editorPreviewer.toScale, editorPreviewer.duration)
                    .SetEase(editorPreviewer.ease)
                    .SetAutoKill(false)
                    .Pause();

                try
                {
                    DOTweenEditorPreview.PrepareTweenForPreview(previewTween, true, true, false);
                }
                catch { }
            }

            // scrub by Goto (time)
            previewTween.Goto(Mathf.Clamp01(p) * editorPreviewer.duration, andPlay: false);

            // repaint and mark dirty so visual updates are visible
            EditorSceneManager.MarkSceneDirty(editorPreviewer.gameObject.scene);
            UnityEditor.SceneView.RepaintAll();
        }

        void ApplyInstantFrom()
        {
            if (editorPreviewer == null) return;
            editorPreviewer.transform.localScale = editorPreviewer.fromScale;
        }

        void CompleteAndSnap()
        {
            if (previewTween == null)
            {
                ScrubToProgress(1f);
                return;
            }

            previewTween.Complete(true); // snap to end
            EditorSceneManager.MarkSceneDirty(editorPreviewer.gameObject.scene);
            UnityEditor.SceneView.RepaintAll();
        }
    }

    #endregion
}
#endif