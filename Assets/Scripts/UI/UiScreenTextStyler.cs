using UnityEngine;
using TMPro;

namespace BeachHero
{
    [System.Serializable]
    public struct TextStyleData
    {
        public TextMeshProUGUI text;

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

        public void ApplyStyle()
        {
            if (textStyles != null)
            {
                for (int i = 0; i < textStyles.Length; i++)
                {
                    int index = i;

                    if (textStyles[index].text == null)
                    {
                        continue;
                    }
                    if (textStyles[index].useOutline)
                    {
                        textStyles[index].text.outlineColor = textStyles[index].outlineColor;
                        textStyles[index].text.outlineWidth = textStyles[index].outlineWidth;
                    }
                     if (textStyles[index].useUnderlay)
                    {
                        var mat = textStyles[index].text.fontMaterial;
                        mat.EnableKeyword("UNDERLAY_ON");
                        mat.SetColor("_UnderlayColor", textStyles[index].underlayColor);
                        mat.SetFloat("_UnderlayOffsetX", textStyles[index].underlayOffsetX);
                        mat.SetFloat("_UnderlayOffsetY", textStyles[index].underlayOffsetY);
                        mat.SetFloat("_UnderlaySoftness", textStyles[index].underlaySoftness);
                    }
                }
            }
        }
    }
}
