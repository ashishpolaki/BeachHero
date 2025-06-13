using System;
using UnityEngine;

namespace BeachHero
{
    public class MapClicker :MonoBehaviour
    {
        private void OnMouseDown()
        {
            MapController.Instance.ZoomIn(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }
}
