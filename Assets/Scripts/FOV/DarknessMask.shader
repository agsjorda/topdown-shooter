Shader "Unlit/DarknessMask"
{
    Properties
    {
        _DarknessColor("Darkness Color", Color) = (0,0,0,0.8)
        _MaskTex("Mask Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MaskTex;
            float4 _DarknessColor;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v) {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Mask: white -> cone/light, black -> dark
                fixed mask = tex2D(_MaskTex, i.uv).r; // read red channel (R8 RT)
                // darkness alpha should be applied only outside the cone:
                float darknessAlpha = _DarknessColor.a * (1.0 - mask);
                fixed4 col = _DarknessColor;
                col.a = darknessAlpha;
                return col;
            }
            ENDHLSL
        }
    }
}
