// _gBuffer_0 r: character mask; g: particle mask

#ifndef YOUKIA_MRT
#define YOUKIA_MRT
#include "Youkia.hlsl"

const static half MASK_OPAQUE = 0.5;
const static half MASK_TERRAIN = 0.1h;
const static half MASK_GRASS = 0.2;
const static half MASK_CHARACTER = 0.3h;

#define ALPHAFADE 2.0f

TEXTURE2D_SAMPLER2DFLOAT(_gBuffer_0, sampler_gBuffer_0);
half4 _gBuffer_0_TexelSize;

TEXTURE2D_SAMPLER2D(_gBufferOri_0, sampler_gBufferOri_0);

inline half4 GetMask(float2 texcoord)
{
	// half4 mask = SAMPLE_TEXTURE2D(_gBuffer_0, sampler_gBuffer_0, texcoord);
	half4 mask = SAMPLE_TEXTURE2D(_gBufferOri_0, sampler_gBufferOri_0, texcoord);
	return mask;
}

inline half GetMaskOpaque(half4 mask)
{
	return saturate(mask.r * 2);
}

inline half GetMaskCharacter(half4 mask)
{
	return saturate(mask.r - MASK_OPAQUE - MASK_GRASS) * 10;
}

inline half GetMaskTerrain(half4 mask)
{
	return 1 - abs(1 - (mask.r - MASK_OPAQUE) * 10);
}

inline half GetMaskGrass(half4 mask)
{
	return floor(1 - abs(mask.r - MASK_OPAQUE - MASK_GRASS));
}

inline half GetMaskParticle(half4 mask)
{
	return mask.g;
}

//--------------------------------------------------------------------------
// unity depth texture
TEXTURE2D_SAMPLER2DFLOAT(_CameraDepthTexture, sampler_CameraDepthTexture);
TEXTURE2D_SAMPLER2DFLOAT(_gSSS, sampler_gSSS);
TEXTURE2D_SAMPLER2DFLOAT(_gYoukiaDepthTexture, sampler_gYoukiaDepthTexture);

half4 _CameraDepthTexture_TexelSize;
half4 _gYoukiaDepthTexture_TexelSize;

inline float YoukiaDepth(float2 texcoord)
{
	#ifdef _UNITY_RENDER
		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, texcoord);
	#else
		float depth = SAMPLE_DEPTH_TEXTURE(_gYoukiaDepthTexture, sampler_gYoukiaDepthTexture, texcoord);
	#endif

	return depth;
}

inline float YoukiaDepth01(float2 texcoord)
{
	return Linear01Depth(YoukiaDepth(texcoord));
}

inline float YoukiaDepthLinearEye(float2 texcoord)
{
	return LinearEyeDepth(YoukiaDepth(texcoord));
}

inline half4 YoukiaDepthTextureTexelSize()
{
	#ifdef _UNITY_RENDER
		return _CameraDepthTexture_TexelSize;
	#else
		return _gYoukiaDepthTexture_TexelSize;
	#endif
}

#endif
