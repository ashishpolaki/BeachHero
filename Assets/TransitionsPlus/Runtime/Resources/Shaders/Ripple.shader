Shader "TransitionsPlus/Ripple"
{
    Properties
    {
        [HideInInspector] _T("Progress", Range(0, 1)) = 0
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _MaskTex ("Texture", 2D) = "white" {}
        [HideInInspector] _NoiseTex ("Noise", 2D) = "white" {}
        [HideInInspector] _FirstCameraTex ("First Camera Tex", 2D) = "white" {}
        [HideInInspector] _GradientTex ("Gradient Tex", 2D) = "white" {}
        [HideInInspector] _Color("Color", Color) = (0,0,0)
        [HideInInspector] _VignetteIntensity("Vignette Intensity", Range(0,1)) = 0.5
        [HideInInspector] _NoiseIntensity("Noise Intensity", Range(0,1)) = 0.5
        [HideInInspector] _ToonIntensity("Toon Intensity", Float) = 1
        [HideInInspector] _ToonDotIntensity("Toon Dot Intensity", Float) = 1
        [HideInInspector] _AspectRatio("Aspect Ratio", Float) = 1
        [HideInInspector] _Distortion("Distortion", Float) = 1
        [HideInInspector] _ToonDotRadius("Toon Dot Radius", Float) = 0
        [HideInInspector] _ToonDotCount("Toon Dot Count", Float) = 0
        [HideInInspector] _Contrast("Constrast", Float) = 1
        [HideInInspector] _CellDivisions("Cell Divisions", Float) = 32
        [HideInInspector] _Spread("Spread", Float) = 64
        [HideInInspector] _RotationMultiplier("Rotation", Float) = 0
        [HideInInspector] _Rotation("Rotation", Float) = 0
        [HideInInspector] _Splits("Splits", Float) = 2
        [HideInInspector] _Seed("Seed", Float) = 0
        [HideInInspector] _CentersCount("Seed", Int) = 1
        [HideInInspector] _Center("Center", Vector) = (0,0,0)
        [HideInInspector] _TimeMultiplier("Time Multiplier", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "Ripple"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ TEXTURE GRADIENT_OPACITY GRADIENT_TIME GRADIENT_SPATIAL_RADIAL GRADIENT_SPATIAL_HORIZONTAL GRADIENT_SPATIAL_VERTICAL

            #include "TransitionsCommon.cginc"
        
            #define WAVES 8.0
            #define FADE 4.0
            #define DISPLACEMENT 0.25

            fixed4 frag (v2f i) : SV_Target {
                float2 suv = 2.0 * (i.uv.xy - 0.5);
                suv.x *= GetAspectRatio();
                float len = length(suv);
       
                float splitTime = _T * WAVES;
                float pt = max(0.0, splitTime - len);
                float s, c;
                sincos(pt * WAVES, s, c);
                float wh = 0.5 + 0.5 * c;
                float ws = max(0.0, 1.0 - pt / FADE);
                float2 uv = i.uv.xy - suv * (s * ws * DISPLACEMENT / (len * FADE));
                float t = wh * ws;

                fixed4 color = tex2D(_FirstCameraTex, uv);
                fixed4 finalColor = ComputeOutputColor(uv, i.noiseUV, 1.0);

                return lerp(finalColor, color, t);
            }
            ENDCG
        }
    }
}
