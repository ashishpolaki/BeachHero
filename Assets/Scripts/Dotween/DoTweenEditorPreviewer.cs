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
        #region Inspector Variables (Scale)
        [Header("Scale Tween")]
        [SerializeField] private bool enableScale = true;
        [SerializeField] private Vector3 fromScale = Vector3.one;
        [SerializeField] private Vector3 toScale = Vector3.one * 1.5f;
        [SerializeField] private float scaleDuration = 1f;
        [SerializeField] private float scaleStartTime = 0f;
        [SerializeField] private Ease scaleEase = Ease.Linear;
        #endregion

        #region Inspector Variables (Move)
        [Header("Move Tween")]
        [SerializeField] private bool enableMove = true;
        [SerializeField] private Vector3 fromPos = Vector3.zero;
        [SerializeField] private Vector3 toPos = Vector3.right;
        [SerializeField] private bool moveIsLocal = true;
        [SerializeField] private float moveDuration = 1f;
        [SerializeField] private float moveStartTime = 0f;
        [SerializeField] private Ease moveEase = Ease.Linear;
        #endregion

        #region Private Variables
        // Individual tweens (kept for safety but main preview uses sequence)
        private Tween scaleTween;
        private Tween moveTween;
        private Sequence previewSequence;
        #endregion

        #region Properties
        /// <summary>
        /// Total duration of the sequence (computed from tweens startTime + duration)
        /// </summary>
        public float Duration
        {
            get
            {
                float max = 0f;
                if (enableScale) max = Mathf.Max(max, scaleStartTime + Mathf.Max(0f, scaleDuration));
                if (enableMove) max = Mathf.Max(max, moveStartTime + Mathf.Max(0f, moveDuration));
                return Mathf.Max(0f, max);
            }
        }

        /// <summary>
        /// Build (or return cached) preview Sequence composed of configured tweens.
        /// Sequence is created with AutoKill = false and paused so inspector can scrub.
        /// </summary>
        public Sequence PreviewSequence
        {
            get
            {
                if (previewSequence != null && previewSequence.IsActive()) return previewSequence;

                // Cleanup any previous cached tweens/sequence
                try { previewSequence?.Kill(); } catch { }
                previewSequence = DOTween.Sequence();
                previewSequence.SetAutoKill(false);
                previewSequence.Pause();

                // Build scale tween if enabled
                if (enableScale)
                {
                    // ensure from state set later via ResetState
                    scaleTween = transform.DOScale(toScale, Mathf.Max(0.0001f, scaleDuration)).SetEase(scaleEase).SetAutoKill(false).Pause();
                    // Insert at start time
                    previewSequence.Insert(scaleStartTime, scaleTween);
                }

                // Build move tween if enabled
                if (enableMove)
                {
                    if (moveIsLocal)
                        moveTween = transform.DOLocalMove(toPos, Mathf.Max(0.0001f, moveDuration)).SetEase(moveEase).SetAutoKill(false).Pause();
                    else
                        moveTween = transform.DOMove(toPos, Mathf.Max(0.0001f, moveDuration)).SetEase(moveEase).SetAutoKill(false).Pause();

                    previewSequence.Insert(moveStartTime, moveTween);
                }

                // if no tweens were added, create an empty sequence to avoid nulls
                return previewSequence;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Put components into their configured 'from' states (used before previewing / when resetting).
        /// </summary>
        public void ResetState()
        {
            if (enableScale) transform.localScale = fromScale;
            if (enableMove)
            {
                if (moveIsLocal) transform.localPosition = fromPos;
                else transform.position = fromPos;
            }
        }

        public void KillTweens()
        {
            try { scaleTween?.Kill(); } catch { }
            try { moveTween?.Kill(); } catch { }
            try { previewSequence?.Kill(); } catch { }
            scaleTween = null;
            moveTween = null;
            previewSequence = null;
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
    }

    #region Editor
    [CustomEditor(typeof(DoTweenEditorPreviewer))]
    public class DoTweenEditorPreviewerEditor : Editor
    {
        private DoTweenEditorPreviewer editorPreviewer;
        private Sequence previewSequence;
        private bool isPreviewing = false;
        private float progress = 0f;

        void OnEnable()
        {
            if (Application.isPlaying) return;
            editorPreviewer = (DoTweenEditorPreviewer)target;
            // ensure any previous editor preview is stopped
            try { DOTweenEditorPreview.Stop(); } catch { }
        }

        void OnDisable()
        {
            if (Application.isPlaying) return;
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

            // Scrub slider
            progress = EditorGUILayout.Slider("Progress", progress, 0f, 1f);

            while (prop.NextVisible(enterChildren))
            {
                if (prop.name == "m_Script") continue; // skip script field
                EditorGUILayout.PropertyField(prop, true);
                enterChildren = false;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(editorPreviewer, "Change preview properties");
                ScrubToProgress(progress);
                EditorUtility.SetDirty(editorPreviewer);
            }
            serializedObject.ApplyModifiedProperties();

            // Preview controls
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

            if (GUILayout.Button("Reset"))
            {
                Undo.RecordObject(editorPreviewer, "Reset preview");
                ApplyInstantFrom();
                EditorUtility.SetDirty(editorPreviewer);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Snap to End"))
            {
                CompleteAndSnap();
            }
        }

        void StartPreview()
        {
            if (editorPreviewer == null) return;

            // Cleanup existing sequence/tweens
            if (previewSequence != null)
            {
                previewSequence.Kill();
                previewSequence = null;
            }
            editorPreviewer.KillTweens();

            // ensure object is at 'from' state
            editorPreviewer.ResetState();

            // Build sequence
            previewSequence = editorPreviewer.PreviewSequence;

            // Prepare for editor preview loop
            try
            {
                DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
                // prepare the sequence for preview. Some versions expect Tween, Sequence inherits Tween.
                DOTweenEditorPreview.PrepareTweenForPreview(previewSequence, true, true, true);
            }
            catch
            {
                Debug.LogError("Could not prepare tween/sequence for editor preview");
            }

            // Start the editor preview loop (repaints SceneView)
            DOTweenEditorPreview.Start(() => { UnityEditor.SceneView.RepaintAll(); });
            isPreviewing = true;
        }

        private void StopPreviewAndCleanup()
        {
            if (previewSequence != null)
            {
                try { previewSequence.Kill(); } catch { }
                previewSequence = null;
            }

            try { DOTweenEditorPreview.Stop(); } catch { }

            ApplyInstantFrom();
            editorPreviewer.KillTweens();

            isPreviewing = false;

            EditorSceneManager.MarkSceneDirty(editorPreviewer.gameObject.scene);
            UnityEditor.SceneView.RepaintAll();
        }

        void ScrubToProgress(float p)
        {
            // Lazily create sequence for scrubbing if needed
            if (previewSequence == null)
            {
                previewSequence = editorPreviewer.PreviewSequence;
                try
                {
                    DOTweenEditorPreview.PrepareTweenForPreview(previewSequence, true, true, false);
                }
                catch { }
            }

            // clamp and goto time (p * total duration)
            float t = Mathf.Clamp01(p) * editorPreviewer.Duration;
            previewSequence.Goto(t, andPlay: false);

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
            if (previewSequence == null)
            {
                ScrubToProgress(1f);
                return;
            }

            previewSequence.Complete(true); // snap to end
            EditorSceneManager.MarkSceneDirty(editorPreviewer.gameObject.scene);
            UnityEditor.SceneView.RepaintAll();
        }
    }
    #endregion
}
#endif
