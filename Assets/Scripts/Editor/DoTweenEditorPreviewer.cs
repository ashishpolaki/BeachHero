#if UNITY_EDITOR
using UnityEngine;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BeachHero
{
    public class DoTweenEditorPreviewer : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private Vector3 fromScale = Vector3.one;
        [SerializeField] private Vector3 toScale = Vector3.one * 1.5f;
        [SerializeField] private float duration = 1f;
        [SerializeField] private Ease ease = Ease.Linear;
        #endregion

        #region Private Variables
        private Tween scaleTween;
        #endregion

        #region Properties
        public Tween PreviewTween
        {
            get
            {
                if (scaleTween == null || !scaleTween.IsActive())
                {
                    scaleTween = transform.DOScale(toScale, duration).SetEase(ease).SetAutoKill(false).Pause();
                }
                return scaleTween;
            }
        }
        public float Duration => Mathf.Max(0f, duration);
        #endregion

        #region Methods
        public void ResetState()
        {
            transform.localScale = fromScale;
        }
        public void KillTween()
        {
            if (scaleTween != null && scaleTween.active)
            {
                scaleTween.Kill();
                scaleTween = null;
            }
        }
        /// <summary>
        /// Kills all active tweens on this GameObject, including those targeting any component.
        /// </summary>
        public void KillAllTweensOnGameObject()
        {
            foreach (var component in GetComponents<Component>())
            {
                DOTween.Kill(component);
            }
        }
        #endregion

        private void PlayTweening()
        {
            PreviewTween.Restart();
        }
    }

    #region Editor
    [CustomEditor(typeof(DoTweenEditorPreviewer))]
    public class DoTweenEditorPreviewerEditor : Editor
    {
        private DoTweenEditorPreviewer editorPreviewer;
        private Tween previewTween;
        private bool isPreviewing = false;
        private float progress = 0f;

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }
            editorPreviewer = (DoTweenEditorPreviewer)target;
            // ensure any previous editor preview is stopped
            try { DOTweenEditorPreview.Stop(); } catch { }
        }

        void OnDisable()
        {
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
                DrawDefaultInspector();
                return;
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            // Draw all other properties, except "m_Script"
            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;
            progress = EditorGUILayout.Slider("Progress", progress, 0f, 1f);
            while (prop.NextVisible(enterChildren))
            {
                if (prop.name == "m_Script") continue; // skip script field
                EditorGUILayout.PropertyField(prop, true);
                enterChildren = false;
            }

            // Handle changes to progress slider 
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editorPreviewer, "Change progress");
                ScrubToProgress(progress);
                EditorUtility.SetDirty(editorPreviewer);
            }
            serializedObject.ApplyModifiedProperties();

            //Preview
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

            // Reset
            if (GUILayout.Button("Reset"))
            {
                Undo.RecordObject(editorPreviewer, "Reset preview");
                ApplyInstantFrom();
                EditorUtility.SetDirty(editorPreviewer);
            }
            EditorGUILayout.EndHorizontal();

            // Snap to end
            if (GUILayout.Button("Snap to End"))
            {
                CompleteAndSnap();
            }
        }

        void StartPreview()
        {
            if (editorPreviewer == null) return;

            // Cleanup any existing preview tween
            if (previewTween != null)
            {
                previewTween.Kill();
                previewTween = null;
            }

            // set object to 'from' state first
            editorPreviewer.ResetState();

            // create tween (non-looping) and keep it (autoKill=false)
            previewTween = editorPreviewer.PreviewTween; // don't play via runtime loop

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

        private void StopPreviewAndCleanup()
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
            editorPreviewer.KillTween();

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
                previewTween = editorPreviewer.PreviewTween;

                try
                {
                    DOTweenEditorPreview.PrepareTweenForPreview(previewTween, true, true, false);
                }
                catch { }
            }

            // scrub by Goto (time)
            previewTween.Goto(Mathf.Clamp01(p) * editorPreviewer.Duration, andPlay: false);

            // repaint and mark dirty so visual updates are visible
            EditorSceneManager.MarkSceneDirty(editorPreviewer.gameObject.scene);
            UnityEditor.SceneView.RepaintAll();
        }

        void ApplyInstantFrom()
        {
            if (editorPreviewer == null) return;
            editorPreviewer.ResetState();
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