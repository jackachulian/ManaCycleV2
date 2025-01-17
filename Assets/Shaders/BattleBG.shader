
Shader "ManaCycle/Unlit/BattleBacgkround"
{
    Properties{
        _MainTex("MainTex",2D) = "white"{} 
        ringWidth("Sun Ring Width", float) = 0.68
        hole("Sun Hole Size", float) = 0.0
        height("Grid Height Offset", float) = -0.1
        hLines("Horz. Line Count", float) = 5
        vLines("Vert. Line Count", float) = 30
        lWidth("Line Width", float) = 0.03
        aa("Edge Smoothing", float) = 0.005
        scrollSpeed("Grid Speed", float) = 0.2
        lineSmooth("Line Smoothstep Value", float) = 10.0
        finalDarken("Final Darken Amount", float) = 0.5

        skyColorTop("Top Sky Color", Color) = (0.5, 0.05, 0.4)
        skyColorBottom("Bottom Sky Color", Color) = (0.5, 0.05, 0.6)
        sunColor("Sun Color", Color) = (1.0, 0.6, 0.1)
        gridColor("Grid Line Color", Color) = (0.65, 0.95, 0.95)
        floorColor("Grid Floor Color", Color) = (0.0, 0.0, 0.08)

    }
        SubShader
        {
        Tags { "RenderType" = "Opaque"}
        Pass
        {
        ZWrite Off
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
                
        float4 vec4(float x,float y,float z,float w){return float4(x,y,z,w);}
        float4 vec4(float x){return float4(x,x,x,x);}
        float4 vec4(float2 x,float2 y){return float4(float2(x.x,x.y),float2(y.x,y.y));}
        float4 vec4(float3 x,float y){return float4(float3(x.x,x.y,x.z),y);}

        float3 vec3(float x,float y,float z){return float3(x,y,z);}
        float3 vec3(float x){return float3(x,x,x);}
        float3 vec3(float2 x,float y){return float3(float2(x.x,x.y),y);}

        float2 vec2(float x,float y){return float2(x,y);}
        float2 vec2(float x){return float2(x,x);}

        float vec(float x){return float(x);}

        struct VertexInput {
            float4 vertex : POSITION;
            float2 uv:TEXCOORD0;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            //VertexInput
        };
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float2 uv:TEXCOORD0;
            //VertexOutput
        };
        sampler2D _MainTex; 


        VertexOutput vert (VertexInput v)
        {
            VertexOutput o;
            o.pos = UnityObjectToClipPos (v.vertex);
            o.uv = v.uv;
            //VertexFactory
            return o;
        }

        float ringWidth;
        float hole;
        float height;
        float hLines;
        float vLines;
        float lWidth;
        float aa;
        float scrollSpeed;
        float lineSmooth;
        float finalDarken;

        float4 skyColorTop;
        float4 skyColorBottom;
        float4 sunColor;
        float4 floorColor;
        float4 gridColor;

        float nsin(float x) {
            return (sin(x) + 1.0) * 0.5;
        }

        float gridDist(float2 uv)
        {
            float d = 0.0;
            
            for (float i = 0.0; i < hLines; i += 1.0)
            {
                // adding 3.0 below as quick fix
                d += smoothstep(
                lWidth, 
                lWidth - aa * lineSmooth, 
                fmod(height + abs(3.0 + _Time.y * scrollSpeed - exp(2.0 * uv.y + 0.8) - (i / hLines) - lWidth), 1.0)
                ) * float(uv.y < height);
            }

            for (float j = 0.0; j < vLines; j += 1.0)
            {
                d += smoothstep(
                lWidth, 
                lWidth - aa * lineSmooth, 
                abs(uv.x + 0.5 + (uv.y * (-0.5 + vLines / 2.0 - j)) - (j / vLines - lWidth))
                ) * float(uv.y < height);
            }

            return (d / 2.0);
        }

        // TODO branch logic bad
        fixed4 frag(VertexOutput vertex_output) : SV_Target
        {
            // Normalized pixel coordinates (from -1 to 1)
            float2 uv = (vertex_output.uv * 2.0 - 1) / 1;
            uv.x *= _ScreenParams.x / _ScreenParams.y;

            float4 noise = tex2D(_MainTex, fmod(uv + _Time.y * 0.1 + 0.5, 1.0));

            // define colors
            sunColor.r = 1.0 + nsin(uv.y + 2.0 * _Time.y) * 0.4;
            sunColor.g = 0.4 + nsin(uv.x + _Time.y) * 0.4;

            // sun and sky
            float3 col = float3(0.0, 0.0, 0.0);
            float d = length(uv - (vec2(noise.r, noise.g) - 0.5) * 0.1);
            col += smoothstep(ringWidth, ringWidth - aa, abs(hole - d)) * sunColor;
            if (col.r == 0.0) col = lerp(skyColorBottom, skyColorTop, (uv.y + 1.0) / 2.0 - height); 
            
            col += smoothstep(0.1 + d, ringWidth - aa, d) * sunColor * 0.5;

            // below horizon
            if (uv.y < height) col = floorColor;

            // horizon line
            col += smoothstep(
                lWidth * 0.5, 
                lWidth * 0.5 - aa, 
                abs(uv.y - height)
            ) * gridColor;

            // grid
            d = gridDist(uv);
            col += smoothstep(0.1, 0.1 + aa, d) * gridColor;

            // output to screen
            return vec4(col,1.0) * (1.0 - finalDarken);

        }
        ENDCG
        }
    }
}
