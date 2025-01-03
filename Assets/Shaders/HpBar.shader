Shader "ManaCycle/Unlit/HPBarShader"
{
    Properties
    {
        _HpColor ("HP Color", Color) = (0, 1, 0, 1)
        _BackColor ("Back Color", Color) = (0, 0, 0, 1)
        _DamageStartColor ("Damage Start Color", Color) = (1, 0.75, 0, 1)
        _DamageEndColor ("Damage End Color", Color) = (1, 0.25, 0, 1)
        _DamageFinalColor ("Damage Final Color", Color) = (1, 0, 0, 1)
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
            fixed4 _DamageStartColor;
            fixed4 _DamageEndColor;
            fixed4 _DamageFinalColor;
            float _HpPercentage;
            float _IncomingDamage[6] = {0, 0.02, 0, 0.1, 0.05, 0.3};

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

                float totalDamage = 0;
                for (int i = 5; i >= 0; i--) {
                    totalDamage += _IncomingDamage[i];
                    if (yUV <= _HpPercentage && yUV > _HpPercentage - totalDamage) {
                        hpBarColor = (i == 5) ? _DamageFinalColor : lerp(_DamageStartColor, _DamageFinalColor, i/5.0);
                        break;
                    }
                }

                return hpBarColor;
            }
            ENDCG
        }
    }
}
