Shader "PostProcessing/Magic"
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
			float _MinRemap;
			float _MaxRemap;

			float _LumPow;
			float _Quantize;
			float _DitherRange;
			int _Diamondize;
			int _UseDither;

			float4 _ColArray[10];
			int _ColArrayCount;
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


		bool sampleDither4x4(float intensity, uint2 pixelId)
		{
			float dither4x4[4 * 4] = {
				0, 8, 2, 10,
				12, 4, 14, 6,
				3, 11, 1, 9,
				15, 7, 13, 5
			};

			const uint width = 4;
			const uint size = 16;//4*4
			uint i = pixelId.x + pixelId.y * width;
			i = i % size;

			float dither = dither4x4[i] / float(size);
			return intensity <= dither;
		}

		float3 sampleDither4x4(float3 color, uint2 pixelId)
		{
			return color * float3(
				sampleDither4x4(color.r, pixelId),
				sampleDither4x4(color.g, pixelId),
				sampleDither4x4(color.b, pixelId));
		}

		ENDHLSL

		Pass
		{
			Name "MAGIC"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag_magic

			float4 GetMagicColor(v2f input)
			{
				if(_Diamondize == 1)
				{
					input.uv = DiamondUV(input.uv);
				}
				else
				{
					input.uv = floor(input.uv * _Quantize) / _Quantize;
				}

				float4 col = tex2D(_MainTex, input.uv);
				float lum = col.x;
	
				lum = remap(_MinRemap, _MaxRemap, 0., 1., lum);

				lum = pow(abs(lum), _LumPow);
				float invCount = 1 / (float)_ColArrayCount;

				int iterationCount = _ColArrayCount - _UseDither;

				for (int i = 0; i < iterationCount; i++)
				{
					float threshold = 1. - ((float)i + 1.) * invCount;
					if (lum > threshold)
					{
						if (_UseDither == 1)
						{
							float ditherValue = remap(threshold, threshold + invCount, 0, 1, lum);
							int i0 = i;
							int i1 = i + 1;
							bool sampleDither = sampleDither4x4(ditherValue, (int2)(input.uv  * _Quantize));
							return sampleDither ? _ColArray[i1] : _ColArray[i0];
						}
						else
						{
							return _ColArray[i];
						}
					}
	
				}
				return col;
			}

			float4 frag_magic(v2f input) : SV_Target
			{
				return GetMagicColor(input);
			}
            ENDHLSL
        }
    }
}
