// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// https://forum.unity3d.com/threads/billboard-geometry-shader.169415/
Shader "Custom/BillboardMod1"
{
	Properties
	{
		_SpriteTex ("Base (RGB)", 2D) = "red" {}
		_Size ("Size", Range(0, 3)) = 0.5
		_MinSizeFactor ("MinSizeFactor", Range(0.001,0.01)) = 0.001
	}

	SubShader
	{
		Pass
		{
			ColorMask RGBA
			//Cull Off
			Lighting Off
			//ZWrite Off
			//Blend OneMinusDstColor One//
			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

			Tags 
			{ 
				"RenderType"="Transparent" 
				"Queue" = "AlphaTest"
			"IgnoreProjector" = "True"

			}
			LOD 200

			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc"

				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct GS_INPUT
				{
					float4	pos		: POSITION;
					float3	normal	: NORMAL;
					float2  tex0	: TEXCOORD0;
					float4 tex1		: TEXCOORD1; // The second UV coordinate.
					float4 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
					float4	col		: COLOR;
				};

				struct FS_INPUT
				{
					float4	pos		: POSITION;
					float2  tex0	: TEXCOORD0;
					float4	col		: COLOR;
				};

				struct appdata_v
				{
					float4	pos		: POSITION;
					float2  tex0	: TEXCOORD0;
					float4	color	: COLOR;
				};

				// **************************************************************
				// Vars															*
				// **************************************************************

				float _Size;
				float _MinSizeFactor;
				float4x4 _VP;
				Texture2D _SpriteTex;
				SamplerState sampler_SpriteTex;

				// **************************************************************
				// Shader Programs												*
				// **************************************************************

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main(appdata_full v)
				{
					GS_INPUT output = (GS_INPUT)0;

					output.pos =  mul(unity_ObjectToWorld, v.vertex);
					output.normal = v.normal;
					output.tex0 = float2(0, 0);
					output.col = v.color;//float4(0.1f, 0.2f, 0.6f, 0.5f);
					return output;
				}

				float rand(float3 myVector)  {
             return frac(sin( dot(myVector ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
         }

				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(4)]
				void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
				{
					//float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
					float3 up = UNITY_MATRIX_IT_MV[1].xyz;
					float3 right = -UNITY_MATRIX_IT_MV[0].xyz;
					float dist = length(ObjSpaceViewDir(p[0].pos));
					
					//float3 look = _WorldSpaceCameraPos - p[0].pos;
					//look.y = 0;
					//look = normalize(look);
					//float3 right = cross(up, look);
					float halfS = 0.5f * (_Size + (dist * _MinSizeFactor));// *right);// p[0].pos.z);
					float4 v[4];
					v[0] = float4(p[0].pos + halfS * right - halfS * up, 1.0f);
					v[1] = float4(p[0].pos + halfS * right + halfS * up, 1.0f);
					v[2] = float4(p[0].pos - halfS * right - halfS * up, 1.0f);
					v[3] = float4(p[0].pos - halfS * right + halfS * up, 1.0f);

					FS_INPUT pIn;

					pIn.col = p[0].col;

					pIn.pos = UnityObjectToClipPos(v[0]);
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = UnityObjectToClipPos(v[1]);
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos = UnityObjectToClipPos(v[2]);
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = UnityObjectToClipPos(v[3]);
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
				}



				// Fragment Shader -----------------------------------------------
				float4 FS_Main(FS_INPUT input) : COLOR
				{
					//return input.col;
					return _SpriteTex.Sample(sampler_SpriteTex, input.tex0) * input.col;
				}

			ENDCG
		}
	}
}
