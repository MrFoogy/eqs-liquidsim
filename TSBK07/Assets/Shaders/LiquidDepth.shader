Shader "Custom/LiquidDepth"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_Radius("Radius", Float) = 1
		_Shininess("Shininess", Float) = 10
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			ZWrite On
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			// Variables
			float _Radius, _Shininess;
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
				float4 worldPos : TEXCOORD1;
			};

            struct v2g
            {
                float4 pos : SV_POSITION;
				float2 tex0 : TEXCOORD0;
            };

			struct fragOut
			{
				half4 color : COLOR;
				float depth : DEPTH;
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
				float4 worldPos = mul(unity_ObjectToWorld, p[0].pos);

				g2f out1;
				out1.pos = UnityObjectToClipPos(v[0]);
				out1.tex0 = float2(1.0f, -1.0f);
				out1.worldPos = worldPos;

				g2f out2;
				out2.pos = UnityObjectToClipPos(v[1]);
				out2.tex0 = float2(1.0f, 1.0f);
				out2.worldPos = worldPos;

				g2f out3;
				out3.pos = UnityObjectToClipPos(v[2]);
				out3.tex0 = float2(-1.0f, -1.0f);
				out3.worldPos = worldPos;

				g2f out4;
				out4.pos = UnityObjectToClipPos(v[3]);
				out4.tex0 = float2(-1.0f, 1.0f);
				out4.worldPos = worldPos;

				triStream.Append(out1);
				triStream.Append(out2);
				triStream.Append(out3);
				triStream.Append(out4);
			}
           
            fragOut frag(g2f i) 
            {
				// Calculate normal from texCoord (which is the normalized position on the quad)
				float r = dot(i.tex0, i.tex0);
				if (r > 1) discard;
				float3 objectNormal = float3(-i.tex0[0], i.tex0[1], (1 - r));
				float3 worldNormal = mul(transpose((float3x3) UNITY_MATRIX_V), objectNormal);
				float4 worldPos = float4(i.worldPos.xyz + _Radius * worldNormal, 1.0);
				float4 clipPos = mul(UNITY_MATRIX_VP, worldPos);

				// Lighting
				float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
				float3 vert2LightSource = _WorldSpaceLightPos0.xyz - worldPos.xyz;
				float oneOverDistance = 1.0 / length(vert2LightSource);
				float attenuation = lerp(1.0, oneOverDistance, _WorldSpaceLightPos0.w);
				float3 lightDirection = _WorldSpaceLightPos0.xyz - worldPos.xyz * _WorldSpaceLightPos0.w;

				float3 ambientLighting = UNITY_LIGHTMODEL_AMBIENT.rgb * _Color.rgb;
				float3 diffuseReflection = attenuation * _LightColor0.rgb * _Color.rgb * max(0.0, dot(worldNormal, lightDirection));
				float3 specularReflection = float3(0.0, 0.0, 0.0);
				if (dot(worldNormal, lightDirection) >= 0.0) {
					specularReflection = attenuation * _LightColor0.rgb * pow(max(0.0, dot(reflect(-lightDirection, worldNormal), viewDirection)), _Shininess);
				}
				float3 color = ambientLighting + diffuseReflection + specularReflection;

				fragOut o;
				float depth = clipPos.z / clipPos.w;
				o.depth = depth;
				float drawDepth = Linear01Depth(depth);
				o.color = float4(drawDepth, drawDepth, drawDepth, 1.0);
				return o;
            }
            ENDCG
        }
    }
	Fallback "Standard"
}