#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    public class DrownCharacterEditTool : MonoBehaviour
    {
        [Range(0, 1f)] public float waitTimePercentage;
        [SerializeField] private float waitTime;
        private float levelTime;
        private DrownCharacterUI savedCharacterUI;
        private bool canDrawGizmos;
        private float prevWaitTime = -1f;
        private float prevWaitTimePercentage = -1f;

        public void Init(Vector3 _position, float _waitTimePercentage, float _levelTime)
        {
            transform.position = _position;
            waitTimePercentage = _waitTimePercentage;
            levelTime = _levelTime;
            waitTime = (levelTime * waitTimePercentage * 100) / 100f;
            savedCharacterUI = GetComponent<DrownCharacterUI>();
            canDrawGizmos = true;
        }

        private void OnValidate()
        {
            // only run in editor and when levelTime is meaningful
            const float eps = 1e-5f;

            // If levelTime is zero or negative we cannot compute percentage from time reliably.
            if (levelTime <= 0f)
            {
                prevWaitTime = waitTime;
                prevWaitTimePercentage = waitTimePercentage;
                return;
            }

            // detect which field changed since last validate
            bool waitTimeChanged = !Mathf.Approximately(prevWaitTime, waitTime);
            bool percentageChanged = !Mathf.Approximately(prevWaitTimePercentage, waitTimePercentage);

            if (waitTimeChanged && !percentageChanged)
            {
                // user edited waitTime -> update percentage
                waitTime = Mathf.Clamp(waitTime, 0f, levelTime);
                waitTimePercentage = Mathf.Clamp01(waitTime / levelTime);
            }
            else if (percentageChanged && !waitTimeChanged)
            {
                // user edited percentage -> update waitTime
                waitTimePercentage = Mathf.Clamp01(waitTimePercentage);
                waitTime = Mathf.Clamp(waitTimePercentage * levelTime, 0f, levelTime);
            }
            else if (waitTimeChanged && percentageChanged)
            {
                // both changed (unlikely) -> prefer last edited: choose larger relative change
                float relA = Mathf.Abs(waitTime - prevWaitTime) / (Mathf.Abs(prevWaitTime) + eps);
                float relB = Mathf.Abs(waitTimePercentage - prevWaitTimePercentage) / (Mathf.Abs(prevWaitTimePercentage) + eps);
                if (relA >= relB)
                    waitTimePercentage = Mathf.Clamp01(waitTime / levelTime);
                else
                    waitTime = Mathf.Clamp(waitTimePercentage * levelTime, 0f, levelTime);
            }

            // clamp and store previous values
            waitTime = Mathf.Clamp(waitTime, 0f, levelTime);
            waitTimePercentage = Mathf.Clamp01(waitTimePercentage);
            prevWaitTime = waitTime;
            prevWaitTimePercentage = waitTimePercentage;
        }

        private void OnDrawGizmos()
        {
            if (!canDrawGizmos) return;

            // keep values in valid ranges
            if (levelTime > 0f)
            {
                waitTime = Mathf.Clamp(waitTime, 0f, levelTime);
                waitTimePercentage = Mathf.Clamp01(waitTimePercentage);
            }
            else
            {
                waitTime = Mathf.Max(0f, waitTime);
                waitTimePercentage = Mathf.Clamp01(waitTimePercentage);
            }

            // update UI if available
            if (savedCharacterUI != null)
                savedCharacterUI.UpdateTimer(waitTimePercentage);
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false; // Disable editing
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true; // Re-enable editing
        }
    }
}
#endif
