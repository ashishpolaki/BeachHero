using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Bokka;

namespace Bokka.BeachRescue
{
    public class StartPointHandles : MonoBehaviour
    {
        [Header("Disk")]
        public Color diskColor;
        public float diskRadius;

        [Header("Text")]
        public Vector3 textPositionOffset;
        public Color textColor;
        public bool displayText;
        [InfoBox("If \"useTextVariable\" is false then name of gameobject will be displayed.", InfoBoxType.Normal)]
        public bool useTextVariable;
        public string text;

    }
}