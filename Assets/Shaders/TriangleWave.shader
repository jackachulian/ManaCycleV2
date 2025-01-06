Shader "Hidden/TriangleWaveImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeScale ("TimeScale", Float) = 1.0
        _Wavelength ("Wavelength", Float) = 0.1
        _Height ("Height", Float) = 0.6
        _Steepness ("Steepness", Float) = 1.0
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _SwapAxis ("SwapAxis", Range (0, 1)) = 1    }
    SubShader
    {
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _TimeScale;
            float4 _Color;
            float _t;
            int _SwapAxis;
            float _Wavelength;
            float _Height;
            float _Steepness;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = (1, 1, 1, 1);

                float uvx = _SwapAxis ? i.uv.x : i.uv.y;
                float uvy = _SwapAxis ? i.uv.y : i.uv.x;

                float y = abs(((uvy + _Time * _TimeScale) % _Wavelength) - _Wavelength / 2.0) * _Steepness + _Height;
                col.a = step(1.0, uvx + y);

                return col * _Color;
            }
            ENDCG
        }
    }
}