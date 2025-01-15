// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ScreenSpace ScrollingStripes"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _TimeScale ("TimeScale", float) = 1.0
        _Width ("Width", float) = 0.33
        _XFactor ("XFactor", float) = 5.0
        _Darken ("Darken", float) = 0.5

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _TimeScale;
            float4 _Color;
            float _Width;
            float _XFactor;
            float _Darken;

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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // float aspect = _ScreenParams.x / _ScreenParams.y;
                // i.uv.x = i.uv.x * aspect;

                fixed4 col = tex2D(_MainTex, i.uv);
                fixed2 pos = ComputeScreenPos(i.vertex) / 800;
                col = (0.0, 0.0, 0.0, col.a);
                // accessing col.rgb is causing weird channel shifting?
                // col = pos.x / 100;
                col.r += step((1.0 - (pos.x * _XFactor + pos.y + _Time * _TimeScale) % 1.0) * 0.5, _Width) * _Darken;
                col.g += step((1.0 - (pos.x * _XFactor + pos.y + _Time * _TimeScale) % 1.0) * 0.5, _Width) * _Darken;
                col.b += step((1.0 - (pos.x * _XFactor + pos.y + _Time * _TimeScale) % 1.0) * 0.5, _Width) * _Darken;

                return col * _Color;
            }
            ENDCG
        }
    }
}
