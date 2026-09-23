#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BeachHero
{
    [InitializeOnLoad]
    internal static class CoinSceneToolOverlay
    {
        private const float PanelWidth = 310f;
        private const float PanelHeight = 300f;
        private const float HeaderHeight = 24f;

        private static readonly Color PanelColor = new Color(0.18f, 0.18f, 0.18f, 0.96f);

        private static Rect panelRect = new Rect(10f, 10f, PanelWidth, PanelHeight);
        private static EditorSceneController controller;
        private static bool isOpen;
        private static bool isDragging;
        private static Vector2 dragOffset;

        private static float offsetX = 1f;
        private static float offsetZ;
        private static float startY;
        private static float gapY = 10f;
        private static int selectedCoinPreviewIndex;
        private static readonly List<Transform> selectedCoinsInSelectionOrder = new List<Transform>();

        public static bool IsOpen => isOpen && controller != null;

        static CoinSceneToolOverlay()
        {
            Selection.selectionChanged -= HandleSelectionChanged;
            Selection.selectionChanged += HandleSelectionChanged;
            EditorApplication.delayCall += SynchronizeSelectedCoinOrder;
        }

        public static void Toggle(EditorSceneController source)
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open(source);
            }
        }

        public static void Open(EditorSceneController source)
        {
            if (source == null)
            {
                return;
            }

            controller = source;
            isOpen = true;

            SceneView.duringSceneGui -= DrawSceneGUI;
            SceneView.duringSceneGui += DrawSceneGUI;
            SynchronizeSelectedCoinOrder();
            SceneView.RepaintAll();
        }

        public static void Close()
        {
            SceneView.duringSceneGui -= DrawSceneGUI;
            controller = null;
            isOpen = false;
            isDragging = false;
            SceneView.RepaintAll();
        }

        private static void DrawSceneGUI(SceneView sceneView)
        {
            if (!isOpen)
            {
                return;
            }

            if (controller == null)
            {
                Close();
                return;
            }

            Event currentEvent = Event.current;
            HandlePanelDragging(currentEvent, sceneView);

            if (panelRect.Contains(currentEvent.mousePosition))
            {
                int controlId = GUIUtility.GetControlID("CoinSceneToolOverlay".GetHashCode(), FocusType.Passive);
                HandleUtility.AddDefaultControl(controlId);
            }

            List<Transform> coins = GetSelectedCoins();
            bool closeRequested = false;

            Handles.BeginGUI();
            EditorGUI.DrawRect(panelRect, PanelColor);
            GUILayout.BeginArea(panelRect);

            GUI.Box(new Rect(0f, 0f, PanelWidth, HeaderHeight), "Coin Tool - drag header to move");
            if (GUI.Button(new Rect(PanelWidth - 23f, 2f, 20f, 20f), "X", EditorStyles.miniButton))
            {
                closeRequested = true;
            }

            GUILayout.Space(HeaderHeight + 5f);
            EditorGUILayout.LabelField($"Selected coins: {coins.Count}", EditorStyles.boldLabel);

            if (coins.Count == 0)
            {
                EditorGUILayout.HelpBox("Select one or more coin objects.", MessageType.Info);
            }
            else
            {
                string[] coinNames = new string[coins.Count];
                for (int index = 0; index < coins.Count; index++)
                {
                    string anchorSuffix = index == 0 ? " (Anchor)" : string.Empty;
                    coinNames[index] = $"{index + 1}. {coins[index].name}{anchorSuffix}";
                }

                selectedCoinPreviewIndex = Mathf.Clamp(selectedCoinPreviewIndex, 0, coins.Count - 1);
                selectedCoinPreviewIndex = EditorGUILayout.Popup(
                    "Selected Coins",
                    selectedCoinPreviewIndex,
                    coinNames);
                EditorGUILayout.LabelField("Fixed first coin", coins[0].name);
            }

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Arrange Selected Coins", EditorStyles.boldLabel);
            offsetX = EditorGUILayout.FloatField("Offset X", offsetX);
            offsetZ = EditorGUILayout.FloatField("Offset Z", offsetZ);

            using (new EditorGUI.DisabledScope(coins.Count == 0))
            {
                if (GUILayout.Button("Arrange Selected Coins", GUILayout.Height(24f)))
                {
                    ArrangeSelectedCoins(coins);
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Rotate Selected Coins", EditorStyles.boldLabel);
            startY = EditorGUILayout.FloatField("Start Y (deg)", startY);
            gapY = EditorGUILayout.FloatField("Gap Y (deg)", gapY);

            using (new EditorGUI.DisabledScope(coins.Count == 0))
            {
                if (GUILayout.Button("Apply Y Rotation", GUILayout.Height(24f)))
                {
                    RotateSelectedCoins(coins);
                }
            }

            GUILayout.EndArea();
            Handles.EndGUI();

            if (closeRequested)
            {
                Close();
            }
        }

        private static void HandlePanelDragging(Event currentEvent, SceneView sceneView)
        {
            Rect dragRect = new Rect(
                panelRect.x,
                panelRect.y,
                panelRect.width - 27f,
                HeaderHeight);

            if (currentEvent.type == EventType.MouseDown &&
                currentEvent.button == 0 &&
                dragRect.Contains(currentEvent.mousePosition))
            {
                isDragging = true;
                dragOffset = currentEvent.mousePosition - panelRect.position;
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseDrag && isDragging && currentEvent.button == 0)
            {
                panelRect.position = currentEvent.mousePosition - dragOffset;
                panelRect.x = Mathf.Clamp(panelRect.x, 0f, Mathf.Max(0f, sceneView.position.width - panelRect.width));
                panelRect.y = Mathf.Clamp(panelRect.y, 0f, Mathf.Max(0f, sceneView.position.height - panelRect.height));
                currentEvent.Use();
                sceneView.Repaint();
            }
            else if (currentEvent.type == EventType.MouseUp && isDragging && currentEvent.button == 0)
            {
                isDragging = false;
                currentEvent.Use();
            }
        }

        private static List<Transform> GetSelectedCoins()
        {
            SynchronizeSelectedCoinOrder();
            return new List<Transform>(selectedCoinsInSelectionOrder);
        }

        private static void SynchronizeSelectedCoinOrder()
        {
            var currentlySelectedCoins = new List<Transform>();

            foreach (Transform selectedTransform in Selection.transforms)
            {
                if (!IsCoin(selectedTransform))
                {
                    continue;
                }

                if (!currentlySelectedCoins.Contains(selectedTransform))
                {
                    currentlySelectedCoins.Add(selectedTransform);
                }
            }

            selectedCoinsInSelectionOrder.RemoveAll(
                coin => coin == null || !currentlySelectedCoins.Contains(coin));

            Transform activeTransform = Selection.activeTransform;
            if (IsCoin(activeTransform) &&
                currentlySelectedCoins.Contains(activeTransform) &&
                !selectedCoinsInSelectionOrder.Contains(activeTransform))
            {
                selectedCoinsInSelectionOrder.Add(activeTransform);
            }

            currentlySelectedCoins.Sort(CompareHierarchyOrder);
            foreach (Transform coin in currentlySelectedCoins)
            {
                if (!selectedCoinsInSelectionOrder.Contains(coin))
                {
                    selectedCoinsInSelectionOrder.Add(coin);
                }
            }
        }

        private static bool IsCoin(Transform selectedTransform)
        {
            if (selectedTransform == null)
            {
                return false;
            }

            Collectable collectable = selectedTransform.GetComponentInChildren<Collectable>();
            return collectable != null && collectable.CollectableType == CollectableType.Coin;
        }

        private static int CompareHierarchyOrder(Transform left, Transform right)
        {
            if (left == right)
            {
                return 0;
            }

            if (left.parent == right.parent)
            {
                return left.GetSiblingIndex().CompareTo(right.GetSiblingIndex());
            }

            int parentComparison = string.CompareOrdinal(GetHierarchyPath(left.parent), GetHierarchyPath(right.parent));
            return parentComparison != 0
                ? parentComparison
                : left.GetSiblingIndex().CompareTo(right.GetSiblingIndex());
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            string path = transform.GetSiblingIndex().ToString("D6");
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.GetSiblingIndex().ToString("D6") + "/" + path;
            }

            return path;
        }

        private static void ArrangeSelectedCoins(List<Transform> coins)
        {
            if (coins.Count == 0)
            {
                return;
            }

            Transform firstCoin = coins[0];
            Vector3 anchorPosition = firstCoin.position;

            if (coins.Count > 1)
            {
                Transform[] movingCoins = coins.GetRange(1, coins.Count - 1).ToArray();
                Undo.RecordObjects(movingCoins, "Arrange Selected Coins");

                for (int index = 1; index < coins.Count; index++)
                {
                    Transform coin = coins[index];
                    coin.position = anchorPosition + new Vector3(offsetX * index, 0f, offsetZ * index);
                    RecordTransformChange(coin);
                }
            }

            Debug.Log(
                $"[CoinSceneTool] Kept '{firstCoin.name}' fixed and arranged {coins.Count - 1} following coin(s) with X/Z offsets ({offsetX}, {offsetZ}).");
            controller.ReorderCollectablesFromFixedCoin(coins);
        }

        private static void RotateSelectedCoins(List<Transform> coins)
        {
            if (coins.Count == 0)
            {
                return;
            }

            Undo.RecordObjects(coins.ToArray(), "Rotate Selected Coins");

            for (int index = 0; index < coins.Count; index++)
            {
                Transform coin = coins[index];
                Vector3 eulerAngles = coin.eulerAngles;
                eulerAngles.y = startY + gapY * index;
                coin.eulerAngles = eulerAngles;
                RecordTransformChange(coin);
            }

            Debug.Log(
                $"[CoinSceneTool] Applied Y rotation starting at {startY} degrees with a {gapY} degree gap to {coins.Count} coin(s).");
            controller.ReorderCollectablesFromFixedCoin(coins);
        }

        private static void RecordTransformChange(Transform transform)
        {
            EditorUtility.SetDirty(transform);

            if (PrefabUtility.IsPartOfPrefabInstance(transform))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
            }

            if (transform.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(transform.gameObject.scene);
            }
        }

        private static void HandleSelectionChanged()
        {
            SynchronizeSelectedCoinOrder();

            if (isOpen)
            {
                SceneView.RepaintAll();
            }
        }
    }
}
#endif
