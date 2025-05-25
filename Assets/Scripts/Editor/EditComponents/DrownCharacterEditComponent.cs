#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BeachHero
{
    public class DrownCharacterEditComponent : MonoBehaviour
    {
        [Range(0, 1f)] public float waitTimePercentage;
        [ReadOnly]
        [SerializeField] private float waitTime;
        private float levelTime;
        private DrownCharacterUI savedCharacterUI;
        private bool canDrawGizmos;

        public void Init(Vector3 _position, float _waitTimePercentage, float _levelTime)
        {
            transform.position = _position;
            waitTimePercentage = _waitTimePercentage;
            levelTime = _levelTime;
            waitTime = (levelTime * waitTimePercentage * 100) / 100f;
            savedCharacterUI = GetComponent<DrownCharacterUI>();
            canDrawGizmos = true;
        }
        private void OnDrawGizmos()
        {
            if (!canDrawGizmos) return;
            waitTime = (levelTime * waitTimePercentage * 100) / 100f;
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
