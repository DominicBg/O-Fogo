Shader "PostProcessing/HeatEffect"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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

		sampler2D _MainTex;

		CBUFFER_START(UnityPerMaterial)
			float _Blend;
			float2 _SinAmplitude;
			float2 _SinFrequency;
			float2 _WaveEffect;
			float _VerticalScroll;
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

		ENDHLSL

			Pass
		{
			Name "HEAT"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag_magic

			float4 frag_magic(v2f input) : SV_Target
			{
				float4 col = tex2D(_MainTex, input.uv);

				float2 uv2 = input.uv;

				float yOffset = _SinAmplitude.y * sin(_Time * _SinFrequency.y * 2 * PI + uv2.y * _WaveEffect.y);
				//uv2 += _SinAmplitude * sin(_Time * _SinFrequency * 2 * PI + uv2.yx * _WaveEffect);
				uv2.x += _SinAmplitude.x * sin(_Time * _SinFrequency.x * 2 * PI + uv2.y * _WaveEffect.x + _Time * _VerticalScroll + yOffset);

				float4 blurCol = tex2D(_MainTex, uv2);
				return lerp(col, blurCol, _Blend);
			}
			ENDHLSL
		}
	}
}
