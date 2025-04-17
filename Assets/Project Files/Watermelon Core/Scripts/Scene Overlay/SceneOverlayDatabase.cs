#pragma warning disable 414
using UnityEngine;

namespace Bokka
{
    [CreateAssetMenu(fileName = "Scene Overlay Database", menuName = "Data/Core/Editor/Scene Overlay Database")]
    public class SceneOverlayDatabase : ScriptableObject
    {
        [SerializeField] SearchGroup[] gizmoGroups;
        [SerializeField] SearchGroup[] pickabilityGroups;
        [SerializeField] SearchGroup[] visibilityGroups;
        [SerializeField] bool useShowNawMesh = true;
    }

    [System.Serializable]
    public class SearchGroup
    {
        [SerializeField] string name;
        [SerializeField] string[] queries;
    }
}
