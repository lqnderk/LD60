Shader "UI/CRTMonitor"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Curvature ("Curvature", Range(0.0, 1.0)) = 0.28
        _CornerRoundness ("Corner Roundness", Range(0.01, 0.3)) = 0.08
        _CornerSmoothness ("Corner Smoothness", Range(0.001, 0.05)) = 0.015
        _ScanlineIntensity ("Scanline Intensity", Range(0.0, 1.0)) = 0.35
        _ScanlineCount ("Scanline Count", Float) = 340.0
        _ScanlineSpeed ("Scanline Speed", Float) = 0.6
        _VignetteIntensity ("Vignette Intensity", Range(0.0, 2.0)) = 0.55
        _VignetteRoundness ("Vignette Roundness", Range(0.5, 4.0)) = 1.8
        _GlassReflection ("Glass Reflection", Range(0.0, 1.0)) = 0.18
        _PhosphorIntensity ("Phosphor Intensity", Range(0.0, 0.5)) = 0.12
        _Aberration ("Chromatic Aberration", Range(0.0, 0.04)) = 0.006
        _Flicker ("Flicker", Range(0.0, 0.1)) = 0.015
        _Brightness ("Brightness", Range(0.5, 2.0)) = 1.15
        _BezelColor ("Bezel Color", Color) = (0.04, 0.04, 0.06, 1.0)
        [Toggle] _DistortTexture ("Distort Texture", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+2001"
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
            float4 _MainTex_ST;
            float _Curvature;
            float _CornerRoundness;
            float _CornerSmoothness;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _ScanlineSpeed;
            float _VignetteIntensity;
            float _VignetteRoundness;
            float _GlassReflection;
            float _PhosphorIntensity;
            float _Aberration;
            float _Flicker;
            float _Brightness;
            fixed4 _BezelColor;
            float _DistortTexture;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float2 centered = i.uv - 0.5;
                float2 aspectCentered = float2(centered.x * aspect, centered.y);
                float r2 = dot(aspectCentered, aspectCentered);

                float distortionFactor = 1.0 + _Curvature * r2;
                float2 curvedUV = centered * distortionFactor + 0.5;

                float2 d = abs(curvedUV - 0.5) * 2.0;
                float2 cornerDelta = max(d - (1.0 - _CornerRoundness * 2.0), 0.0);
                float cornerDist = length(cornerDelta) - _CornerRoundness * 2.0;
                float screenMask = 1.0 - smoothstep(-_CornerSmoothness, _CornerSmoothness, cornerDist);

                float scanlineY = curvedUV.y * _ScanlineCount + _Time.y * _ScanlineSpeed;
                float scanline = sin(scanlineY * 6.2831853);
                float scanlineFactor = 1.0 - _ScanlineIntensity * (0.5 + 0.5 * scanline);

                float pixelX = curvedUV.x * _ScreenParams.x;
                int subpixel = int(fmod(pixelX, 3.0));
                fixed3 phosphor = fixed3(1.0, 1.0, 1.0);
                if (subpixel == 0)
                {
                    phosphor = fixed3(1.0, 1.0 - _PhosphorIntensity, 1.0 - _PhosphorIntensity);
                }
                else if (subpixel == 1)
                {
                    phosphor = fixed3(1.0 - _PhosphorIntensity, 1.0, 1.0 - _PhosphorIntensity);
                }
                else
                {
                    phosphor = fixed3(1.0 - _PhosphorIntensity, 1.0 - _PhosphorIntensity, 1.0);
                }

                float vig = 16.0 * curvedUV.x * curvedUV.y * (1.0 - curvedUV.x) * (1.0 - curvedUV.y);
                vig = saturate(pow(vig, _VignetteRoundness * 0.25));
                float vignetteFactor = lerp(1.0 - _VignetteIntensity, 1.0, vig);

                float2 glareCenter = float2(0.35, 0.7);
                float glareDist = length((i.uv - glareCenter) * float2(aspect, 1.0));
                float glare = smoothstep(0.8, 0.0, glareDist) * _GlassReflection * 0.15;

                float flicker = 1.0 + sin(_Time.y * 120.0) * _Flicker;

                fixed4 finalColor;

                if (_DistortTexture > 0.5)
                {
                    float2 redUV = centered * (distortionFactor + _Aberration * r2) + 0.5;
                    float2 blueUV = centered * (distortionFactor - _Aberration * r2) + 0.5;

                    fixed r = tex2D(_MainTex, redUV).r;
                    fixed g = tex2D(_MainTex, curvedUV).g;
                    fixed b = tex2D(_MainTex, blueUV).b;
                    fixed a = tex2D(_MainTex, curvedUV).a;

                    fixed4 texColor = fixed4(r, g, b, a) * _Brightness;
                    texColor.rgb *= scanlineFactor;
                    texColor.rgb *= phosphor;
                    texColor.rgb *= vignetteFactor;
                    texColor.rgb *= flicker;
                    texColor.rgb += glare;

                    finalColor = lerp(_BezelColor, texColor, screenMask);
                }
                else
                {
                    float insideDarkness = 1.0 - (scanlineFactor * vignetteFactor * phosphor.g);
                    float insideAlpha = insideDarkness * 0.65 + glare;
                    fixed3 insideTint = lerp(fixed3(0.0, 0.0, 0.0), fixed3(1.0, 1.0, 1.0), glare / max(insideAlpha, 0.001));

                    finalColor.rgb = lerp(_BezelColor.rgb, insideTint, screenMask);
                    finalColor.a = lerp(_BezelColor.a, saturate(insideAlpha), screenMask);
                }

                return finalColor;
            }
            ENDCG
        }
    }
}
