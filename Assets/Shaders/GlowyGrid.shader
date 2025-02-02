Shader "ManaCycle/Unlit/GlowyGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Texture", 2D) = "white" {}
        _TimeScale ("TimeScale", Float) = 1.0
        _GridSize ("Grid Size", Float) = 1.0
        _NoiseSize ("Noise Size", Float) = 5.0
        _Brightness ("Brightness", Float) = 2.5
        _BaseCol ("BaseColor", Color) = (0.1, 0.1, 0.2,1.0)
        _Tint1 ("Color 1", Color) = (1.0, 0.2, 0.9 ,1.0)
        _Tint2 ("Color 2", Color) = (0.9, 0.6, 1.0, 1.0)
        _e1 ("Edge 1", Float) = 0.1
        _e2 ("Edge 2", Float) = 0.3
        _Power ("Glow Exponent", Float) = 1.4
        _PowerOffset ("Glow Exponent Pulse", Float) = 0.1
        _BrightnessPulse ("Brightness Pulse", Float) = 0.3
        _NoiseMix ("Noise Mix", Float) = 0.5

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float _GridSize;
            float _NoiseSize;
            float _Brightness;
            fixed4 _Tint1;
            fixed _Tint2;
            float _e1; 
            float _e2; 
            float _Power;
            float _PowerOffset;
            float _BrightnessPulse;
            float _TimeScale;
            float _NoiseMix;
            fixed4 _BaseCol;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 uv = i.uv;
                fixed4 n = tex2D(_NoiseTex, uv * _GridSize * _NoiseSize);
                fixed4 c = tex2D(_NoiseTex, fmod(uv + _Time.y * 0.1 * _TimeScale + n.rg * 0.3, 1.0) * _GridSize * _NoiseSize);
                float v = c.r * (_NoiseMix) - (1.0 - _NoiseMix);
                

                fixed4 col = _BaseCol;

                float GridHalf = _GridSize / 2.0;
                fixed2 grid = abs(fmod(uv + GridHalf, _GridSize) - GridHalf);
                col += (smoothstep(
                    _e1, 
                    _e2, 
                    pow(max(grid.x, grid.y), _Power + (v * _PowerOffset)) / _GridSize)) 
                        * (_Brightness + abs(sin(_Time.y * _TimeScale)) * _BrightnessPulse)
                        * ((_Tint1 - _BaseCol) * (1.0-v) + (_Tint2 - _BaseCol) * v);

                return col;
            }

            ENDCG
        }
    }
}
