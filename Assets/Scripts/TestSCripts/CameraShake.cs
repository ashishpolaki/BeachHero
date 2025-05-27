using System.Collections;
using UnityEngine;

namespace BeachHero
{
    public class CameraShake : MonoBehaviour
    {
        public float duration = 1f;
        public float magnitude = 0.2f;
        public float frequency = 25f;

        private Vector3 originalPos;

        void Start()
        {
            originalPos = transform.localPosition;
        }
    }
}
