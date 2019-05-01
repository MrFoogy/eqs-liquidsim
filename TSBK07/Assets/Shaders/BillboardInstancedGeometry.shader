Shader "Unlit/BillboardInstancedGeometry"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_Radius("Radius", Float) = 1
	}

		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#include "UnityCG.cginc"

			// Variables
			float _Radius;
			float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
				float2 tex0 : TEXCOORD0;
            };

			struct g2f 
			{
				float4 pos : SV_POSITION;
				float2 tex0 : TEXCOORD0;
			};

            struct v2g
            {
                float4 pos : SV_POSITION;
				float2 tex0 : TEXCOORD0;
            };
           
            v2g vert(appdata v)
            {
                v2g o;
				o.pos = v.vertex;
				o.tex0 = float2(0, 0);
				return o;
            }

			[maxvertexcount(4)]
			void geom(point v2g p[1], inout TriangleStream<g2f> triStream) 
			{
				float3 up = UNITY_MATRIX_IT_MV[1].xyz;
				float3 right = -UNITY_MATRIX_IT_MV[0].xyz;

				float4 v[4];
				v[0] = float4(p[0].pos + _Radius * (right - up), 1.0f);
				v[1] = float4(p[0].pos + _Radius * (right + up), 1.0f);
				v[2] = float4(p[0].pos + _Radius * (-right - up), 1.0f);
				v[3] = float4(p[0].pos + _Radius * (-right + up), 1.0f);

				g2f out1;
				out1.pos = UnityObjectToClipPos(v[0]);
				out1.tex0 = float2(1.0f, 0.0f);

				g2f out2;
				out2.pos = UnityObjectToClipPos(v[1]);
				out2.tex0 = float2(1.0f, 1.0f);

				g2f out3;
				out3.pos = UnityObjectToClipPos(v[2]);
				out3.tex0 = float2(0.0f, 0.0f);

				g2f out4;
				out4.pos = UnityObjectToClipPos(v[3]);
				out4.tex0 = float2(0.0f, 1.0f);

				triStream.Append(out1);
				triStream.Append(out2);
				triStream.Append(out3);
				triStream.Append(out4);
			}
           
            fixed4 frag(g2f i) : SV_Target
            {
				return _Color;
            }
            ENDCG
        }
    }
}