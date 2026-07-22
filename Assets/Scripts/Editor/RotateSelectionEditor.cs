#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    [CustomEditor(typeof(EditorSceneController))]
    public class RotateSelectionEditor : Editor
    {
        private float startY = 0f;
        private float gapY = 10f;
        private bool isInspectorLocked = false;

        public override void OnInspectorGUI()
        {
            // Keep existing inspector
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotate Selected Coins", EditorStyles.boldLabel);

            startY = EditorGUILayout.FloatField("Start Y (deg)", startY);
            gapY = EditorGUILayout.FloatField("Gap Y (deg)", gapY);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Rotation To Selection", GUILayout.Height(24)))
            {
                ApplyRotationToSelection();
            }
            EditorGUILayout.EndHorizontal();

            // Start Coins Selection (toggle lock)
            GUILayout.Space(6);
            if (GUILayout.Button(isInspectorLocked ? "Unlock Selection" : "Start Coins Selection", GUILayout.Height(24)))
            {
                var controller = (EditorSceneController)target;

                if (!isInspectorLocked)
                {
                    // START: select controller + coin children and lock inspector
                    var containerField = typeof(EditorSceneController).GetField("container", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    GameObject container = containerField != null ? containerField.GetValue(controller) as GameObject : null;
                    if (container == null) container = controller.gameObject;

                    var list = new System.Collections.Generic.List<Object>();

                    for (int i = 0; i < container.transform.childCount; i++)
                    {
                        var child = container.transform.GetChild(i);
                        if (child == null) continue;
                        var collectable = child.GetComponentInChildren<Collectable>();
                        if (collectable != null && collectable.CollectableType == CollectableType.GameCurrency)
                            list.Add(child.gameObject);
                    }

                    if (list.Count > 1)
                    {
                        Selection.activeGameObject = controller.gameObject;
                        EditorGUIUtility.PingObject(controller.gameObject);

                        LockInspectorTo(controller.gameObject);
                        Selection.objects = list.ToArray();
                        isInspectorLocked = true;

                        SceneView.RepaintAll();
                        Debug.Log($"Selected {list.Count - 1} coin(s) under '{controller.gameObject.name}'.");
                    }
                    else
                    {
                        Debug.LogWarning("No coin (GameCurrency) children found under the controller's container.");
                    }
                }
                else
                {
                    // STOP: unlock inspector and clear selection (keep controller active)
                    UnlockInspector();
                    isInspectorLocked = false;

                    Selection.activeGameObject = controller.gameObject;
                    Selection.objects = new Object[] { controller.gameObject };
                    EditorGUIUtility.PingObject(controller.gameObject);
                    SceneView.RepaintAll();
                }
            }
        }

        private void LockInspectorTo(GameObject go)
        {
            if (go == null) return;

            var editorAsm = typeof(Editor).Assembly;

            // Try InspectorWindow.SetLocked(bool)
            var inspectorType = editorAsm.GetType("UnityEditor.InspectorWindow");
            if (inspectorType != null)
            {
                var inspector = EditorWindow.GetWindow(inspectorType);
                var setLocked = inspectorType.GetMethod("SetLocked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                try
                {
                    if (setLocked != null)
                    {
                        setLocked.Invoke(inspector, new object[] { true });
                    }
                    else
                    {
                        // fallback: set internal m_Locked field if present
                        var lockedField = inspectorType.GetField("m_Locked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (lockedField != null)
                        {
                            lockedField.SetValue(inspector, true);
                        }
                    }
                    inspector.Repaint();
                }
                catch
                {
                    // ignore reflection failures
                }
            }

            // Also try to lock ActiveEditorTracker sharedTracker (best-effort)
            var trackerType = editorAsm.GetType("UnityEditor.ActiveEditorTracker");
            if (trackerType != null)
            {
                var sharedProp = trackerType.GetProperty("sharedTracker", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (sharedProp != null)
                {
                    var shared = sharedProp.GetValue(null);
                    if (shared != null)
                    {
                        var isLockedProp = trackerType.GetProperty("isLocked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        try
                        {
                            if (isLockedProp != null)
                                isLockedProp.SetValue(shared, true);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }
        }

        private void UnlockInspector()
        {
            var editorAsm = typeof(Editor).Assembly;

            // Try InspectorWindow.SetLocked(false)
            var inspectorType = editorAsm.GetType("UnityEditor.InspectorWindow");
            if (inspectorType != null)
            {
                var inspector = EditorWindow.GetWindow(inspectorType);
                var setLocked = inspectorType.GetMethod("SetLocked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                try
                {
                    if (setLocked != null)
                    {
                        setLocked.Invoke(inspector, new object[] { false });
                    }
                    else
                    {
                        var lockedField = inspectorType.GetField("m_Locked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (lockedField != null)
                        {
                            lockedField.SetValue(inspector, false);
                        }
                    }
                    inspector.Repaint();
                }
                catch
                {
                    // ignore reflection failures
                }
            }

            // Also unset ActiveEditorTracker.sharedTracker.isLocked if available
            var trackerType = editorAsm.GetType("UnityEditor.ActiveEditorTracker");
            if (trackerType != null)
            {
                var sharedProp = trackerType.GetProperty("sharedTracker", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (sharedProp != null)
                {
                    var shared = sharedProp.GetValue(null);
                    if (shared != null)
                    {
                        var isLockedProp = trackerType.GetProperty("isLocked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        try
                        {
                            if (isLockedProp != null)
                                isLockedProp.SetValue(shared, false);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }
        }

        private void ApplyRotationToSelection()
        {
            var transforms = Selection.transforms;
            if (transforms == null || transforms.Length == 0)
            {
                Debug.LogWarning("[RotateSelectionEditor] No objects selected.");
                return;
            }

            // Keep only selected objects that are coins (Collectable with CollectableType.GameCurrency)
            var coins = new System.Collections.Generic.List<Transform>();
            foreach (var t in transforms)
            {
                if (t == null) continue;
                var collectable = t.GetComponentInChildren<Collectable>();
                if (collectable != null && collectable.CollectableType == CollectableType.GameCurrency)
                    coins.Add(t);
            }

            if (coins.Count == 0)
            {
                Debug.LogWarning("[RotateSelectionEditor] No coin objects (GameCurrency) found in the current selection.");
                return;
            }

            // Apply rotations (preserve selection order)
            Undo.RecordObjects(coins.ToArray(), "Rotate Coins Y");
            for (int i = 0; i < coins.Count; i++)
            {
                var tr = coins[i];
                if (tr == null) continue;
                Vector3 e = tr.eulerAngles;
                e.y = startY + gapY * i;
                tr.eulerAngles = e;
                EditorUtility.SetDirty(tr);
            }

            Debug.Log($"[RotateSelectionEditor] Applied Y rotation starting {startY}°, gap {gapY}° to {coins.Count} coins.");

            // Frame the scene to the coin bounds
            if (SceneView.lastActiveSceneView != null)
            {
                Bounds bounds = new Bounds(coins[0].position, Vector3.zero);
                for (int i = 1; i < coins.Count; i++)
                    bounds.Encapsulate(coins[i].position);

                SceneView.lastActiveSceneView.Frame(bounds, false);
                SceneView.RepaintAll();
            }

            // After applying rotation, unlock inspector if it was locked
            if (isInspectorLocked)
            {
                UnlockInspector();
                isInspectorLocked = false;
                // keep controller selected if present
                // (optional) you could clear selection or keep coins selected
            }
        }
    }
}
#endif
