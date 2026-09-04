#if UNITY_EDITOR
using System;
using System.Globalization;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BeachHero
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Transform))]
    public sealed class TransformClipboardEditor : TransformClipboardEditorBase
    {
        protected override Type BuiltInInspectorType
        {
            get { return typeof(Editor).Assembly.GetType("UnityEditor.TransformInspector"); }
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(RectTransform))]
    public sealed class RectTransformClipboardEditor : TransformClipboardEditorBase
    {
        protected override Type BuiltInInspectorType
        {
            get { return typeof(Editor).Assembly.GetType("UnityEditor.RectTransformEditor"); }
        }
    }

    public abstract class TransformClipboardEditorBase : Editor
    {
        private const string ClipboardHeader = "BeachHeroTransformClipboardV1";

        private enum CopyMode
        {
            None,
            LocalTransform,
            WorldTransform,
            Component
        }

        private Editor builtInEditor;
        private static CopyMode lastCopyMode;

        protected abstract Type BuiltInInspectorType { get; }

        protected virtual void OnEnable()
        {
            Type inspectorType = BuiltInInspectorType;
            if (inspectorType != null)
            {
                builtInEditor = CreateEditor(targets, inspectorType);
            }
        }

        protected virtual void OnDisable()
        {
            if (builtInEditor != null)
            {
                DestroyImmediate(builtInEditor);
            }
        }

        public override void OnInspectorGUI()
        {
            if (builtInEditor != null)
            {
                builtInEditor.OnInspectorGUI();
            }
            else
            {
                DrawDefaultInspector();
            }

            GUILayout.Space(2f);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Local Transform"))
            {
                CopyLocalTransformText();
            }

            if (GUILayout.Button("Copy World Transform"))
            {
                CopyWorldTransformText();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Component"))
            {
                CopyComponent();
            }

            EditorGUI.BeginDisabledGroup(lastCopyMode == CopyMode.None);
            if (GUILayout.Button("Paste"))
            {
                PasteRecentCopy();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private void CopyLocalTransformText()
        {
            Transform transform = GetTargetTransform();

            if (transform == null)
            {
                return;
            }

            EditorGUIUtility.systemCopyBuffer = SerializeTransform(transform, CopyMode.LocalTransform);
            lastCopyMode = CopyMode.LocalTransform;
            Debug.Log($"Copied local transform values from '{transform.name}' to text clipboard.");
        }

        private void CopyWorldTransformText()
        {
            Transform transform = GetTargetTransform();

            if (transform == null)
            {
                return;
            }

            EditorGUIUtility.systemCopyBuffer = SerializeTransform(transform, CopyMode.WorldTransform);
            lastCopyMode = CopyMode.WorldTransform;
            Debug.Log($"Copied world transform values from '{transform.name}' to text clipboard.");
        }

        private void CopyComponent()
        {
            Transform transform = GetTargetTransform();

            if (transform == null)
            {
                return;
            }

            ComponentUtility.CopyComponent(transform);
            lastCopyMode = CopyMode.Component;
            Debug.Log($"Copied {transform.GetType().Name} component from '{transform.name}'.");
        }

        private void PasteRecentCopy()
        {
            switch (lastCopyMode)
            {
                case CopyMode.LocalTransform:
                case CopyMode.WorldTransform:
                    PasteTransformValues(lastCopyMode);
                    break;
                case CopyMode.Component:
                    PasteComponentValues();
                    break;
            }
        }

        private void PasteTransformValues(CopyMode copyMode)
        {
            if (!TryDeserializeTransform(EditorGUIUtility.systemCopyBuffer, out TransformSnapshot snapshot))
            {
                Debug.LogWarning("Could not paste transform values because the transform clipboard data is unavailable.");
                return;
            }

            bool pasteWorld = copyMode == CopyMode.WorldTransform;
            Undo.RecordObjects(targets, pasteWorld ? "Paste World Transform" : "Paste Local Transform");

            foreach (UnityEngine.Object selectedTarget in targets)
            {
                Transform transform = selectedTarget as Transform;
                if (transform == null)
                {
                    continue;
                }

                if (pasteWorld)
                {
                    ApplyWorldSnapshot(transform, snapshot);
                }
                else
                {
                    ApplyLocalSnapshot(transform, snapshot);
                }

                EditorUtility.SetDirty(transform);
            }
        }

        private void PasteComponentValues()
        {
            Undo.RecordObjects(targets, "Paste Transform Component");

            foreach (UnityEngine.Object selectedTarget in targets)
            {
                Transform transform = selectedTarget as Transform;
                if (transform == null)
                {
                    continue;
                }

                if (ComponentUtility.PasteComponentValues(transform))
                {
                    EditorUtility.SetDirty(transform);
                    continue;
                }

                Debug.LogWarning($"Could not paste copied component values onto '{transform.name}'.");
            }
        }

        private struct TransformSnapshot
        {
            public bool IsRectTransform;
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
            public Vector2 AnchoredPosition;
            public Vector2 SizeDelta;
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
        }

        private Transform GetTargetTransform()
        {
            Transform activeTransform = Selection.activeTransform;
            if (activeTransform != null)
            {
                foreach (UnityEngine.Object selectedTarget in targets)
                {
                    if (selectedTarget == activeTransform)
                    {
                        return activeTransform;
                    }
                }
            }

            return target as Transform;
        }

        private static string SerializeTransform(Transform transform, CopyMode copyMode)
        {
            bool isLocal = copyMode == CopyMode.LocalTransform;
            RectTransform rectTransform = transform as RectTransform;
            string header = isLocal ? "local" : "world";

            if (!isLocal)
            {
                return string.Join(
                    Environment.NewLine,
                    ClipboardHeader,
                    "mode=" + header,
                    "type=" + (rectTransform != null ? "rect" : "transform"),
                    "position=" + FormatVector3(transform.position),
                    "rotation=" + FormatVector3(transform.eulerAngles),
                    "scale=" + FormatVector3(transform.lossyScale));
            }

            if (rectTransform != null)
            {
                return string.Join(
                    Environment.NewLine,
                    ClipboardHeader,
                    "mode=" + header,
                    "type=rect",
                    "anchoredPosition=" + FormatVector2(rectTransform.anchoredPosition),
                    "sizeDelta=" + FormatVector2(rectTransform.sizeDelta),
                    "anchorMin=" + FormatVector2(rectTransform.anchorMin),
                    "anchorMax=" + FormatVector2(rectTransform.anchorMax),
                    "pivot=" + FormatVector2(rectTransform.pivot),
                    "position=" + FormatVector3(rectTransform.localPosition),
                    "rotation=" + FormatVector3(rectTransform.localEulerAngles),
                    "scale=" + FormatVector3(rectTransform.localScale));
            }

            return string.Join(
                Environment.NewLine,
                ClipboardHeader,
                "mode=" + header,
                "type=transform",
                "position=" + FormatVector3(transform.localPosition),
                "rotation=" + FormatVector3(transform.localEulerAngles),
                "scale=" + FormatVector3(transform.localScale));
        }

        private static void ApplyLocalSnapshot(Transform transform, TransformSnapshot snapshot)
        {
            RectTransform rectTransform = transform as RectTransform;
            if (snapshot.IsRectTransform && rectTransform != null)
            {
                rectTransform.anchorMin = snapshot.AnchorMin;
                rectTransform.anchorMax = snapshot.AnchorMax;
                rectTransform.pivot = snapshot.Pivot;
                rectTransform.sizeDelta = snapshot.SizeDelta;
                rectTransform.anchoredPosition = snapshot.AnchoredPosition;
            }

            transform.localPosition = snapshot.Position;
            transform.localEulerAngles = snapshot.Rotation;
            transform.localScale = snapshot.Scale;
        }

        private static void ApplyWorldSnapshot(Transform transform, TransformSnapshot snapshot)
        {
            transform.SetPositionAndRotation(snapshot.Position, Quaternion.Euler(snapshot.Rotation));
            SetWorldScale(transform, snapshot.Scale);
        }

        private static void SetWorldScale(Transform transform, Vector3 worldScale)
        {
            Transform parent = transform.parent;
            if (parent == null)
            {
                transform.localScale = worldScale;
                return;
            }

            Vector3 parentScale = parent.lossyScale;
            transform.localScale = new Vector3(
                SafeDivide(worldScale.x, parentScale.x),
                SafeDivide(worldScale.y, parentScale.y),
                SafeDivide(worldScale.z, parentScale.z));
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
        }

        private static bool TryDeserializeTransform(string copyBuffer, out TransformSnapshot snapshot)
        {
            snapshot = default(TransformSnapshot);

            if (string.IsNullOrEmpty(copyBuffer))
            {
                return false;
            }

            string[] lines = copyBuffer.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 5 || lines[0] != ClipboardHeader)
            {
                return false;
            }

            string typeValue;
            snapshot.IsRectTransform = TryGetValue(lines, "type=", out typeValue) && typeValue == "rect";

            if (snapshot.IsRectTransform)
            {
                TryGetVector2(lines, "anchoredPosition=", out snapshot.AnchoredPosition);
                TryGetVector2(lines, "sizeDelta=", out snapshot.SizeDelta);
                TryGetVector2(lines, "anchorMin=", out snapshot.AnchorMin);
                TryGetVector2(lines, "anchorMax=", out snapshot.AnchorMax);
                TryGetVector2(lines, "pivot=", out snapshot.Pivot);
            }

            return TryGetVector3(lines, "position=", out snapshot.Position)
                && TryGetVector3(lines, "rotation=", out snapshot.Rotation)
                && TryGetVector3(lines, "scale=", out snapshot.Scale);
        }

        private static bool TryGetVector2(string[] lines, string prefix, out Vector2 value)
        {
            value = default(Vector2);

            string rawValue;
            if (!TryGetValue(lines, prefix, out rawValue))
            {
                return false;
            }

            string[] parts = rawValue.Split(',');
            if (parts.Length != 2)
            {
                return false;
            }

            float x;
            float y;
            bool parsedX = float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
            bool parsedY = float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);

            if (!parsedX || !parsedY)
            {
                return false;
            }

            value = new Vector2(x, y);
            return true;
        }

        private static bool TryGetVector3(string[] lines, string prefix, out Vector3 value)
        {
            value = default(Vector3);

            string rawValue;
            if (!TryGetValue(lines, prefix, out rawValue))
            {
                return false;
            }

            string[] parts = rawValue.Split(',');
            if (parts.Length != 3)
            {
                return false;
            }

            float x;
            float y;
            float z;
            bool parsedX = float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
            bool parsedY = float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
            bool parsedZ = float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z);

            if (!parsedX || !parsedY || !parsedZ)
            {
                return false;
            }

            value = new Vector3(x, y, z);
            return true;
        }

        private static bool TryGetValue(string[] lines, string prefix, out string value)
        {
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith(prefix, StringComparison.Ordinal))
                {
                    value = lines[i].Substring(prefix.Length);
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        private static string FormatVector2(Vector2 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:R}, {1:R}",
                value.x,
                value.y);
        }

        private static string FormatVector3(Vector3 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:R}, {1:R}, {2:R}",
                value.x,
                value.y,
                value.z);
        }
    }
}
#endif
