#if UNITY_EDITOR
using BeachHero;
using UnityEngine;

public class MapCreator : MonoBehaviour
{
    public MapController mapController;

    [Header("Level Setup")]
    public LevelVisual levelPrefab;

    [Header("Debug")]
    public bool showDebug = true;
    [Range(2, 100)] public int debugCount = 20;
    public Transform debugPrefab;
    public Transform debugParent;
}
#endif