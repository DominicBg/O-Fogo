Shader "PostProcessing/Magic"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_Spread("Standard Deviation (Spread)", Float) = 0
		_GridSize("Grid Size", Integer) = 1
    }
    SubShader
    {
        Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}

		HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		#define E 2.71828f

		sampler2D _MainTex;

		CBUFFER_START(UnityPerMaterial)
			float _Threshold;
			float _LumPow;
		CBUFFER_END

		struct appdata
		{
			float4 positionOS : Position;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 positionCS : SV_Position;
			float2 uv : TEXCOORD0;
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
			o.uv = v.uv;
			return o;
		}

		float invLerp(float from, float to, float value) 
		{
			return (value - from) / (to - from);
		}
		float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
		{
			float rel = invLerp(origFrom, origTo, value);
			return lerp(targetFrom, targetTo, rel);
		}
		float luminosity(float3 col)
		{
			return col.r * 0.216 + col.g * 0.7152 + col.b * 0.0722;
		}
		ENDHLSL

		Pass
		{
			Name "MAGIC"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag_magic

			float4 frag_magic(v2f i) : SV_Target
			{		
				float4 col = tex2D(_MainTex, i.uv);
				float lum = pow(luminosity(col), _LumPow);
				if (lum < _Threshold)
				{
					col = 0;
				}
				else
				{
					//lum = remap(_Threshold, 1., 0., 1., lum);
					//col *= lum;
				}

				return col;
			}
            ENDHLSL
        }
    }
}
