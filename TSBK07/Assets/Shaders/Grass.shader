Shader "Custom/Grass" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Size("Size", Float) = 1.0
    }
    SubShader{
        Tags { "RenderType" = "Opaque" }
        ZWrite On
        Cull Off
       
        Pass{
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
 
            #include "UnityCG.cginc"
 
            #pragma target 5.0
 
            sampler2D _MainTex;
            fixed4 _Color;
            float _Size;
 
            struct v2g {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tex0 : TEXCOORD0;
            };
            struct g2f {
                float4 pos : SV_POSITION;
                float2 tex0 : TEXCOORD0;
            };
 
            v2g vert(appdata_base v) {
                v2g output;
           
                output.vertex = v.vertex;
                output.normal = v.normal;
                output.tex0 = v.texcoord;
           
                return output;
            }
 
            float4x4 axisangle(float3 axis, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                float oc = 1.0 - c;
 
                return transpose(float4x4(
                    oc * axis.x * axis.x + c,          oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
                    oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c,          oc * axis.y * axis.z - axis.x * s, 0.0,
                    oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c,          0.0,
                    0.0, 0.0, 0.0, 1.0));
            }
 
            [maxvertexcount(4)]
            void geom(point v2g p[1], inout TriangleStream<g2f> triStream) {
                v2g i = p[0];
 
                float3 up = i.normal;
                // texcoord0.x is a random value between -1 and 1
                float4x4 m = axisangle(up, i.tex0.x * 3.1415926);
                float3 right = mul(m, float3(1,0,0));
 
                up *= _Size;
                right *= .5f * _Size;
 
                g2f v[4];
 
                v[0].pos = float4(i.vertex.xyz + right,      0);
                v[1].pos = float4(i.vertex.xyz + right + up, 0);
                v[2].pos = float4(i.vertex.xyz - right,      0);
                v[3].pos = float4(i.vertex.xyz - right + up, 0);
 
                v[0].pos = UnityObjectToClipPos(v[0].pos);
                v[0].tex0 = float2(1.0f, 0.0f);
 
                v[1].pos = UnityObjectToClipPos(v[1].pos);
                v[1].tex0 = float2(1.0f, 1.0f);
 
                v[2].pos = UnityObjectToClipPos(v[2].pos);
                v[2].tex0 = float2(0.0f, 0.0f);
 
                v[3].pos = UnityObjectToClipPos(v[3].pos);
                v[3].tex0 = float2(0.0f, 1.0f);
 
                triStream.Append(v[0]);
                triStream.Append(v[1]);
                triStream.Append(v[2]);
                triStream.Append(v[3]);
            }
 
            float4 frag(g2f i) : SV_Target {
                float4 col = tex2D(_MainTex, i.tex0.xy) * _Color;
                clip(col.a - .9);
                return col;
            }
 
            ENDCG
        }
    }
	FallBack "Diffuse"
}