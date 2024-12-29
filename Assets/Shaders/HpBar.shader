Shader "ManaCycle/Unlit/HPBarShader"
{
    Properties
    {
        _HpColor ("HP Color", Color) = (0, 1, 0, 1)
        _BackColor ("Back Color", Color) = (0, 0, 0, 1)
        _HpPercentage ("Current HP Percentage", Float) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            fixed4 _HpColor;
            fixed4 _BackColor;
            float _HpPercentage;
            float _IncomingDamage[6];

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float yUV = i.uv.y;
                fixed4 hpBarColor = (yUV <= _HpPercentage) ? _HpColor : _BackColor;
                return hpBarColor;
            }
            ENDCG
        }
    }
}
