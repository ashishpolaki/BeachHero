using UnityEngine;

public class PathTrail : MonoBehaviour
{
    [SerializeField] private TrailRenderer trailRenderer;

    public void ResetTrail(Vector3 position)
    {
        transform.position = position;
        trailRenderer.Clear();
    }
}
