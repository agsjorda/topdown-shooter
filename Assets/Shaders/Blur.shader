Shader "UI/Blur"
{
    Properties
    {
        _BlurAmount("Blur Amount", Range(0, 10)) = 2
        _MainTex("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlurAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y) * _BlurAmount;
                
                fixed4 color = tex2D(_MainTex, i.uv) * 0.2270270270;
                color += tex2D(_MainTex, i.uv + float2(texelSize.x, 0.0)) * 0.1945945946;
                color += tex2D(_MainTex, i.uv - float2(texelSize.x, 0.0)) * 0.1945945946;
                color += tex2D(_MainTex, i.uv + float2(0.0, texelSize.y)) * 0.1945945946;
                color += tex2D(_MainTex, i.uv - float2(0.0, texelSize.y)) * 0.1945945946;
                
                return color;
            }
            ENDCG
        }
    }
}