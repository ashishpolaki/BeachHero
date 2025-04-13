using UnityEngine;
using UnityEditor;
using Watermelon.BeachRescue;

namespace Watermelon
{
    public class MovingObstacleSavableItem : MonoBehaviour
    {
        [SerializeField] private MovingObstacleSave save;
        public MovingObstacleSave Save { get => save; set => save = value; }


        public void OnDrawGizmos()
        {
#if UNITY_EDITOR

            if(save.type == MovingObstacleType.Circle)
            {
                Handles.DrawWireDisc(save.circlarMovementCenter, Vector3.up, save.circlarMovementRadius, 3f);
            }
            else
            {
                Handles.SphereHandleCap(0, save.linearMovementStartPosition, Quaternion.identity, 0.7f, EventType.Repaint);
                Handles.SphereHandleCap(0, save.linearMovementFinishPosition, Quaternion.identity, 0.7f, EventType.Repaint);
                Handles.DrawLine(save.linearMovementStartPosition, save.linearMovementFinishPosition, 5f);
            }
#endif
        }
    }
}