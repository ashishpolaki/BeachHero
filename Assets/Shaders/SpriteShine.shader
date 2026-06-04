Shader "Custom/SpriteShine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineLocation ("Shine Location", Range(0,1)) = 0.5
        _ShineRotate ("Shine Rotation", Range(0,6.28)) = 0
        _ShineWidth ("Shine Width", Range(0.01,1)) = 0.2
        _ShineGlow ("Shine Intensity", Range(0,10)) = 1
        _ShineMask ("Shine Mask", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _ShineMask;

            float4 _MainTex_ST;
            float4 _Color;

            float4 _ShineColor;
            float _ShineLocation;
            float _ShineRotate;
            float _ShineWidth;
            float _ShineGlow;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 uvRect = uv;

                fixed4 col = tex2D(_MainTex, uv) * i.color * _Color;

                // ===== SHINE =====
                float2 uvShine = uvRect;

                float cosA = cos(_ShineRotate);
                float sinA = sin(_ShineRotate);
                float2x2 rot = float2x2(cosA, -sinA, sinA, cosA);

                uvShine -= float2(0.5, 0.5);
                uvShine = mul(rot, uvShine);
                uvShine += float2(0.5, 0.5);

                float shineMask = tex2D(_ShineMask, uv).a;

                float proj = (uvShine.x + uvShine.y) * 0.5;
                float width = max(_ShineWidth, 0.0001);
                float whitePower = saturate(1 - abs(proj - _ShineLocation) / width);

                float shine = smoothstep(_ShineLocation - width, _ShineLocation, proj) *
              smoothstep(_ShineLocation + width, _ShineLocation, proj);

                col.rgb += col.a * whitePower * _ShineGlow * shine * _ShineColor.rgb * shineMask;

                return col;
            }
            ENDCG
        }
    }
}