Shader "ManaCycle/Unlit/HPBarShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HpColor ("HP Color", Color) = (0, 1, 0, 1)
        _BackColor ("Back Color", Color) = (0, 0, 0, 1)
        _DamageStartColor ("Damage Start Color", Color) = (1, 0.75, 0, 1)
        _DamageEndColor ("Damage End Color", Color) = (1, 0.25, 0, 1)
        _DamageFinalColor ("Damage Final Color", Color) = (1, 0, 0, 1)
        _HpPercentage ("Current HP Percentage", Float) = 0.6
        _Height ("Height", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
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
                float3 objPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _HpColor;
            fixed4 _BackColor;
            fixed4 _DamageStartColor;
            fixed4 _DamageEndColor;
            fixed4 _DamageFinalColor;
            float _HpPercentage;
            float _IncomingDamage[6] = {0, 0.02, 0, 0.1, 0.05, 0.3};
            float _Height;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                o.objPos = v.vertex;
                return o;
            }

            fixed4 frag(v2f _i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, _i.uv);
                float y = (_i.objPos.y / _Height) + 0.5;
                // return fixed4(y-0.5,y-0.5,y-0.5,1.0);

                fixed4 hpBarColor = (y <= _HpPercentage) ? _HpColor : _BackColor;

                float totalDamage = 0;
                for (int i = 5; i >= 0; i--) {
                    totalDamage += _IncomingDamage[i];
                    if (y <= _HpPercentage && y > _HpPercentage - totalDamage) {
                        hpBarColor = (i == 5) ? _DamageFinalColor : lerp(_DamageStartColor, _DamageFinalColor, i/5.0);
                        break;
                    }
                }

                return baseCol * hpBarColor;
            }
            ENDCG
        }
    }
}
