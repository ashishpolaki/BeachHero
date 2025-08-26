Shader "TransitionsPlus/TilesProgressive"
{
    Properties
    {
        [HideInInspector] _T("Progress", Range(0, 1)) = 0
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _NoiseTex ("Noise", 2D) = "white" {}
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
            Name "Tiles Progressive"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ TEXTURE GRADIENT_OPACITY GRADIENT_TIME GRADIENT_SPATIAL_RADIAL GRADIENT_SPATIAL_HORIZONTAL GRADIENT_SPATIAL_VERTICAL

            #include "TransitionsCommon.cginc"
        
            int _CellDivisions;

            #define CELL_DIVISIONS _CellDivisions
            #define DELAY   0.15
            #define T_MIN 0.25
            #define T_DOM 0.75
            #define T_SUR 0.22

            fixed4 frag (v2f i) : SV_Target
            {
                float2 fUV = i.uv * _ScreenParams.xy;
                float q = CELL_DIVISIONS / _ScreenParams.y;
                fUV *= _AspectRatio ? CELL_DIVISIONS / _ScreenParams.xy : CELL_DIVISIONS / _ScreenParams.y;
                
                fixed f = 0;
                float cT = ceil(_T);
                float fT = frac(_T);
                float tST = DELAY + Rand(cT * ceil(fUV)) * 0.5;
                float t = T_MIN + T_DOM * smoothstep(0.0, 0.175, fT - tST - T_SUR);
            
                if (fT > tST) {
                    fUV = smoothstep(q, 0.0, abs(frac(fUV) - 0.5) - t * 0.5);
                    f += fUV.x * fUV.y;
                }

                fixed4 color = ComputeOutputColor(i.uv, i.noiseUV, f);

                return color; 

            }
            ENDCG
        }
    }
}
