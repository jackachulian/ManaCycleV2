Shader "ManaCycle/Unlit/Scanlines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _TimeScale ("Time Scale", float) = 1.0
        _OffsetMult ("Offset Multiplier", float) = 1.0
        _NoiseSampleScale ("Noise Sample Scale", float) = 1.0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

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
            float4 _Color;
            sampler2D _NoiseTex;
            float _TimeScale;
            float _OffsetMult;
            float _NoiseSampleScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample noise
                float lineOffset = tex2D(
                    _NoiseTex, 
                    float2(fmod(i.uv.y * _NoiseSampleScale + _Time.y * _TimeScale, 1.0), 0.0)
                ).r * _OffsetMult;

                // sample the texture
                fixed4 col = tex2D(_MainTex, float2(fmod(i.uv.x + lineOffset, 1.0), i.uv.y));
                col.a = 1.0;

                return col * _Color;
            }
            ENDCG
        }
    }
}
