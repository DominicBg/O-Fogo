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

			float3 _NoiseDirection;
			float2 _NoiseScale;
			float _MinNoise;

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

		float4 taylorInvSqrt(float4 r)
		{
			return 1.79284291400159 - 0.85373472095314 * r;
		}

		float3 mod289(float3 x)
		{
			return x - floor(x * (1.0 / 289.0)) * 289.0;
		}

		float4 mod289(float4 x)
		{
			return x - floor(x * (1.0 / 289.0)) * 289.0;
		}

		float4 permute(float4 x)
		{
			return mod289((34.0 * x + 1.0) * x);
		}

		float snoise(float3 v)
		{
			float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
			float4 D = float4(0.0, 0.5, 1.0, 2.0);
			float3 i = floor(v + dot(v, C.yyy));
			float3 x0 = v - i + dot(i, C.xxx);
			float3 g = step(x0.yzx, x0.xyz);
			float3 l = 1.0 - g;
			float3 i1 = min(g.xyz, l.zxy);
			float3 i2 = max(g.xyz, l.zxy);
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - D.yyy;
			i = mod289(i);
			float4 p = permute(permute(permute(
				i.z + float4(0.0, i1.z, i2.z, 1.0))
				+ i.y + float4(0.0, i1.y, i2.y, 1.0))
				+ i.x + float4(0.0, i1.x, i2.x, 1.0));

			float n_ = 0.142857142857;
			float3 ns = n_ * D.wyz - D.xzx;
			float4 j = p - 49.0 * floor(p * ns.z * ns.z);
			float4 x_ = floor(j * ns.z);
			float4 y_ = floor(j - 7.0 * x_);
			float4 x = x_ * ns.x + ns.yyyy;
			float4 y = y_ * ns.x + ns.yyyy;
			float4 h = 1.0 - abs(x) - abs(y);
			float4 b0 = float4(x.xy, y.xy);
			float4 b1 = float4(x.zw, y.zw);
			float4 s0 = floor(b0) * 2.0 + 1.0;
			float4 s1 = floor(b1) * 2.0 + 1.0;
			float4 sh = -step(h, 0.0);
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 p0 = float3(a0.xy, h.x);
			float3 p1 = float3(a0.zw, h.y);
			float3 p2 = float3(a1.xy, h.z);
			float3 p3 = float3(a1.zw, h.w);
			float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
			p0 *= norm.x;
			p1 *= norm.y;
			p2 *= norm.z;
			p3 *= norm.w;
			float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
			m = m * m;
			return 42.0 * dot(m * m, float4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
		}
		ENDHLSL

		Pass
		{
			Name "MAGIC"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag_magic

			float GetNoiseMultiplier(float2 uv)
			{
				uv.x +=
					0.10 * sin(uv.y * 10 + _Time.y * 3) +
					0.05 * sin(uv.y * 15 + _Time.y * 5) +
					0.025 * sin(uv.y * 25 + _Time.y * 7.8) +
					0.012 * sin(uv.y * 45 + _Time.y * 12.8);

				return remap(0, 1, _MinNoise, 1, snoise(float3(uv * _NoiseScale, 0) + _NoiseDirection * _Time.y));
			}
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
				lum *= lerp(GetNoiseMultiplier(input.uv), GetNoiseMultiplier(input.uv * 2), 0.5);

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
				//return (GetMagicColor(input)) * lerp(GetNoiseMultiplier(input.uv), GetNoiseMultiplier(input.uv * 2), 0.5);
				return GetMagicColor(input);
			}
            ENDHLSL
        }
    }
}
