using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bokka;
using System.Linq;

namespace Bokka.BeachRescue
{
    public class TrailBehaviour : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] TrailRenderer trailRendererRef;
        public TrailRenderer TrailRenderer { get => trailRendererRef; }
        [SerializeField] Material normalMaterial;
        [SerializeField] Material transparentMaterial;

        private Transform transformRef;
        public Transform Transform { get => transformRef; }

        private Vector3[] pathPointsList;
        public Vector3[] PathPointsList { get => pathPointsList; set => pathPointsList = value; }

        public bool PathAvailable { get => pathPointsList != null && pathPointsList.Length > 2; }

        private void Awake()
        {
            transformRef = transform;
        }

        public void Init(bool isMainTrail)
        {
            trailRendererRef.Clear();
            pathPointsList = null;

            if (isMainTrail)
            {
                trailRendererRef.material = normalMaterial;
            }
            else
            {
                trailRendererRef.material = transparentMaterial;
            }
        }

        public void CopyPointsFrom(TrailBehaviour originTrail)
        {
            transformRef.position = originTrail.transformRef.position;
            trailRendererRef.Clear();
            Vector3[] positions = new Vector3[originTrail.trailRendererRef.positionCount];
            originTrail.trailRendererRef.GetPositions(positions);

            trailRendererRef.AddPositions(positions);
        }

        public void RemovePointOnTheBeginning()
        {
            if (trailRendererRef.positionCount > 0)
            {
                Vector3[] positions = new Vector3[trailRendererRef.positionCount];

                trailRendererRef.GetPositions(positions);
                positions = positions.SubArray(1, positions.Length - 1);

                trailRendererRef.Clear();
                trailRendererRef.AddPositions(positions);
            }
        }

        public void Reset(Vector3 position)
        {
            transformRef.position = position;
            trailRendererRef.Clear();
            pathPointsList = null;
        }
    }
}