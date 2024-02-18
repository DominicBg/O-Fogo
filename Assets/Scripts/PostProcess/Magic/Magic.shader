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
			float _Quantize;
			int _Diamondize;

			float4 _ColArray[10];
			int _ColArrayCount;
			/*float4 _Col1;
			float4 _Col2;
			float4 _Col3;
			float4 _Col4;*/
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

		float2 Rotate(float2 p, float a)
		{
			float c = cos(a);
			float s = sin(a);
			return float2(
				p.x * c - p.y * s,
				p.x * s + p.y * c);
		}
		float2 DiamondUV(float2 uv)
		{
			float2 diamondUV = Rotate(uv, radians(45.));
			diamondUV = floor(diamondUV * _Quantize) / _Quantize;
			diamondUV = Rotate(diamondUV, -radians(45.));

			return diamondUV;
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
				if(_Diamondize == 1)
				{
					i.uv = DiamondUV(i.uv);
				}
				else
				{
					i.uv = floor(i.uv * _Quantize) / _Quantize;
				}

				float4 col = tex2D(_MainTex, i.uv);
				float lum = col.x;
				if (lum < _Threshold)
				{
					return 0;
				}
				else
				{
					lum = remap(_Threshold, 1., 0., 1., lum);
					lum = pow(lum, _LumPow);
					float invCount = 1 / (float)_ColArrayCount;
					for (int i = _ColArrayCount - 1; i >= 0; i--)
					{
						if (lum > i * invCount)
						{
							return _ColArray[_ColArrayCount- i - 1];
						}
					}
				}

				return col;
			}
            ENDHLSL
        }
    }
}
