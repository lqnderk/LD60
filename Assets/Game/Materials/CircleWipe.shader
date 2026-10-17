Shader "UI/CircleWipe"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Fade Color", Color) = (0.07, 0.07, 0.1, 1)
        _AccentColor ("Accent Color", Color) = (0.95, 0.35, 0.45, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        _CellCount ("Cell Count", Float) = 22
        _WaveWidth ("Wave Width", Range(0.05, 0.6)) = 0.28
        _BorderWidth ("Border Width", Range(0, 0.25)) = 0.08
        _AspectRatio ("Aspect Ratio", Float) = 1.777778
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+2000"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _AccentColor;
            float _Progress;
            float _CellCount;
            float _WaveWidth;
            float _BorderWidth;
            float _AspectRatio;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_Progress <= 0.0001)
                {
                    return fixed4(0, 0, 0, 0);
                }

                if (_Progress >= 0.9999)
                {
                    return _Color;
                }

                float aspect = max(_AspectRatio, 0.001);
                float2 centered = i.uv - float2(0.5, 0.5);
                centered.x *= aspect;
                float delay = saturate(length(centered) * 0.85);

                float spread = max(_WaveWidth, 0.02);
                float startTime = delay * (1.0 - spread);
                float localT = saturate((_Progress - startTime) / spread);

                if (localT <= 0.0)
                {
                    return fixed4(0, 0, 0, 0);
                }

                float2 cellUV = frac(float2(i.uv.x * _CellCount * aspect, i.uv.y * _CellCount)) - 0.5;
                float dist = abs(cellUV.x) + abs(cellUV.y);
                float currentRadius = localT * 1.05;

                float alpha = smoothstep(currentRadius + 0.02, currentRadius, dist);
                if (alpha <= 0.001)
                {
                    return fixed4(0, 0, 0, 0);
                }

                float borderEdge = max(0.0, currentRadius - _BorderWidth);
                float borderFactor = smoothstep(borderEdge - 0.02, borderEdge, dist) * (1.0 - smoothstep(currentRadius, currentRadius - 0.02, dist));
                borderFactor *= (1.0 - smoothstep(0.85, 0.98, localT));

                fixed4 finalColor = lerp(_Color, _AccentColor, saturate(borderFactor));
                return fixed4(finalColor.rgb, finalColor.a * alpha);
            }
            ENDCG
        }
    }
}
