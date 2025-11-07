Shader "Hidden/VisionFogURP"
{
    Properties {
        _FogStrength("Fog Strength", Range(0,1)) = 0.6
        _PlayerPos("Player Pos", Vector) = (0,0,0,0)
        _Radius("Fog Radius", Float) = 12
    }

    SubShader {
        Tags { "RenderType"="Overlay" "Queue"="Overlay" }
        Pass {
            ZTest Always ZWrite Off Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            float _FogStrength;
            float4 _PlayerPos;
            float _Radius;

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = OUT.positionHCS.xy;
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv / _ScreenParams.xy;
                half3 color = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv).rgb;

                float3 worldPos = ComputeWorldSpacePosition(uv, UNITY_MATRIX_I_VP);
                float dist = distance(worldPos.xz, _PlayerPos.xz);
                float fogFactor = saturate(dist / _Radius);

                color = lerp(color, color * (1 - _FogStrength), fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
