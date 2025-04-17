using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bokka.BeachRescue
{
    [System.Serializable]
    public class StartTargetPointSave
    {
        [SerializeField] Vector3 startPointPosition;
        public Vector3 StartPointPosition { get => startPointPosition; }

        [SerializeField] Vector3 startPointRotation;
        public Vector3 StartPointRotation { get => startPointRotation; }

        [SerializeField] Vector3 targetPointPosition;
        public Vector3 TargetPointPosition { get => targetPointPosition; }

        [SerializeField] Vector3 targetPointRotation;
        public Vector3 TargetPointRotation { get => targetPointRotation; }

    }
}