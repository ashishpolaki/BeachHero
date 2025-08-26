Shader "TransitionsPlus/Spiral"
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
            Name "Spiral"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ TEXTURE GRADIENT_OPACITY GRADIENT_TIME GRADIENT_SPATIAL_RADIAL GRADIENT_SPATIAL_HORIZONTAL GRADIENT_SPATIAL_VERTICAL

            #include "TransitionsCommon.cginc"
        
            int _CellDivisions;
            float _Spread;

            #define CELL_DIVISIONS _CellDivisions
            #define SPREAD _Spread

            int getSpiralIndex(int x, int y) {
                // Calcula la capa (ring) actual usando la distancia manhattan máxima desde el centro
                int layer = max(abs(x), abs(y));
            
                // Índice de inicio para esta capa (ring)
                int start_index = (2 * layer - 1) * (2 * layer - 1);
            
                // Calcular el desplazamiento dentro de la capa en sentido horario
                int offset = 0;
                if (x == layer && y > -layer) {
                    // Lado derecho (subiendo)
                    offset = y + layer;
                } else if (y == layer && x < layer) {
                    // Lado superior (yendo hacia la izquierda)
                    offset = (2 * layer) + (layer - x);
                } else if (x == -layer && y < layer) {
                    // Lado izquierdo (bajando)
                    offset = (4 * layer) + (layer - y);
                } else {
                    // Lado inferior (yendo a la derecha)
                    offset = (6 * layer) + (x + layer);
                }
            
                // Índice final
                return start_index + offset;
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                float aspect = _AspectRatio ? GetAspectRatio() : 1.0;
                i.uv.x = (i.uv.x - 0.5) * aspect + 0.5;
                i.uv.y = (i.uv.y - 0.5) / aspect + 0.5;

                 // apply fade
                RotateUV(i.uv);

                float2 cellCoords = floor(i.uv * CELL_DIVISIONS);
            
                int CELL_DIVISIONS_MINUS_1 = CELL_DIVISIONS;
                if (CELL_DIVISIONS % 2) {
                    CELL_DIVISIONS_MINUS_1--;
                }

                int2 cellDiff = (int2)(cellCoords - CELL_DIVISIONS_MINUS_1 / 2.0);
                float cellIndex = getSpiralIndex(cellDiff.x, cellDiff.y);

                float cellCount = CELL_DIVISIONS * CELL_DIVISIONS;
                float cellWidth = 1.0 / cellCount;

                cellIndex = cellIndex * cellWidth;

                float t0 = _T * (1.0 + cellWidth * SPREAD) - cellIndex; 
                fixed fade = saturate( t0 / (cellWidth * SPREAD) );

                fixed4 color = ComputeOutputColor(i.uv, i.noiseUV, fade);

                return color; 

            }
            ENDCG
        }
    }
}
