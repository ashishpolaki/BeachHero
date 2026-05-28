using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BeachHero
{
    [System.Serializable]
    public struct TextStyleData
    {
        public TextMeshProUGUI[] texts;

        [Header("Outline")]
        public bool useOutline;
        [Show(nameof(useOutline))] public Color outlineColor;
        [Show(nameof(useOutline))] public float outlineWidth;

        [Header("Underlay")]
        public bool useUnderlay;
        [Show(nameof(useUnderlay))] public Color underlayColor;
        [Show(nameof(useUnderlay))] public float underlayOffsetX;
        [Show(nameof(useUnderlay))] public float underlayOffsetY;
        [Show(nameof(useUnderlay))] public float underlayThickness; //Dilate
        [Show(nameof(useUnderlay))] public float underlaySoftness;
    }
    public class UiScreenTextStyler : MonoBehaviour
    {
        [SerializeField] private TextStyleData[] textStyles;
        private Dictionary<int, Material> styleMaterialCache = new();

        private Material GetOrCreateMaterial(int index, TextStyleData style)
        {
            if (styleMaterialCache.TryGetValue(index, out var mat))
                return mat;

            var baseText = style.texts[0];
            if (baseText == null) return null;

            mat = new Material(baseText.fontMaterial);

            // Apply Outline style ONCE
            if (style.useOutline)
            {
                mat.SetFloat("_OutlineWidth", style.outlineWidth);
                mat.SetColor("_OutlineColor", style.outlineColor);
            }

            // Aply Underlay
            if (style.useUnderlay)
            {
                mat.EnableKeyword("UNDERLAY_ON");
                mat.SetColor("_UnderlayColor", style.underlayColor);
                mat.SetFloat("_UnderlayOffsetX", style.underlayOffsetX);
                mat.SetFloat("_UnderlayOffsetY", style.underlayOffsetY);
                mat.SetFloat("_UnderlaySoftness", style.underlaySoftness);
                mat.SetFloat("_UnderlayDilate", style.underlayThickness);
            }

            styleMaterialCache[index] = mat;
            return mat;
        }
        private void OnDestroy()
        {
            foreach (var kvp in styleMaterialCache)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value);
            }
            styleMaterialCache.Clear();
        }
        public void ApplyStyle()
        {
            if (textStyles != null)
            {
                for (int i = 0; i < textStyles.Length; i++)
                {
                    int index = i;
                    var style = textStyles[index];
                    if (style.texts == null || style.texts.Length == 0) continue;
                    var mat = GetOrCreateMaterial(i, style);
                    for (int j = 0; j < style.texts.Length; j++)
                    {
                        var text = style.texts[j];
                        if (text == null) continue;

                        text.fontMaterial = mat;
                    }
                }
            }
        }
    }
}
