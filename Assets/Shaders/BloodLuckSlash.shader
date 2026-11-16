Shader "BloodLuck/SlashEffect"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Progress ("Progress (0..1)", Range(0, 1)) = 0.5
        _Tail ("Tail Length", Range(0.05, 1)) = 0.6
        _Alpha ("Alpha", Range(0, 1)) = 1
        _Intensity ("Head Intensity", Range(0, 8)) = 3
        _Thickness ("Max Thickness", Range(0.1, 1)) = 0.85
        _StreakCount ("Streak Count", Range(2, 40)) = 14
        _StreakStretch ("Streak Stretch (U)", Range(0.5, 12)) = 3
        _StreakStrength ("Streak Strength", Range(0, 1)) = 0.7
        _EdgeJitter ("Edge Jitter", Range(0, 0.4)) = 0.14
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Slash"
            Tags { "LightMode" = "Universal2D" }

            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Progress;
                float _Tail;
                float _Alpha;
                float _Intensity;
                float _Thickness;
                float _StreakCount;
                float _StreakStretch;
                float _StreakStrength;
                float _EdgeJitter;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float u = input.uv.x; // 0..1 along the swing direction
                float v = input.uv.y; // 0 = inner edge, 1 = outer edge

                // Visible window: [head - tail, head]
                float head = lerp(0.0, 1.0 + _Tail, _Progress);
                float tailStart = head - _Tail;
                float t = saturate((u - tailStart) / max(_Tail, 1e-4)); // 0 at tail, 1 at head
                float inWindow = step(u, head) * step(tailStart, u);

                // Crescent: outer edge stays on the arc, inner edge cuts in. Tail is thin, head is thick.
                float jitter = (valueNoise(float2(u * 18.0, v * 3.0)) - 0.5) * 2.0 * _EdgeJitter;
                float thickness = _Thickness * pow(t, 0.8) + jitter * t;
                float band = smoothstep(1.0 - thickness - 0.02, 1.0 - thickness + 0.02, v);

                // Streaks running along the swing direction
                float n = valueNoise(float2(u * _StreakStretch, v * _StreakCount));
                n = lerp(1.0, smoothstep(0.15, 0.85, n), _StreakStrength);

                // Brighter towards the head and towards the inner edge of the band
                float coreGlow = lerp(0.35, 1.0, saturate((v - (1.0 - thickness)) / max(thickness, 1e-3)));
                float headGlow = 1.0 + pow(t, 4.0) * _Intensity;

                float a = inWindow * band * n * t * _Alpha;
                half3 rgb = _Color.rgb * a * coreGlow * headGlow;
                return half4(rgb, 1.0);
            }
            ENDHLSL
        }
    }
}
