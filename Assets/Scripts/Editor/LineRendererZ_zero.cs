using System;
using UnityEngine;

namespace BeachHero
{
    public class LineRendererZ_zero : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        public bool run;

        private void OnValidate()
        {
            if (!run)
            {
                return;
            }
            lineRenderer = GetComponent<LineRenderer>();
            run = !run;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                var pos = lineRenderer.GetPosition(i);
                pos.z = 0;
                lineRenderer.SetPosition(i, pos);
            }
        }
    }
}
