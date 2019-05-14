Shader "Custom/LiquidPostProcess"
{
    Properties
    {
        _ColorTex ("ColorTex", 2D) = "black" {}
    }
    SubShader
    {
		// inside SubShader
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass
        {
			Cull Off ZWrite Off ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _CameraDepthTexture;
            sampler2D _ColorTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_ColorTex, i.uv);
				float depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
				fixed4 merge = 0.5 * col + 0.5 * fixed4(depth, depth, depth, 1.0);
				merge.a = col.r;
				//return merge;
                return fixed4(depth, depth, depth, 1.0);
				//return col;
            }
            ENDCG
        }
    }
}