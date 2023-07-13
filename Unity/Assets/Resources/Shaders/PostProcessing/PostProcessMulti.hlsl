#ifndef YOUKIA_POSTPROCESSMULTI
#define YOUKIA_POSTPROCESSMULTI
	
static const half EM_COUNT = 8;
static const half EM_IDUNIT = 10;
static const half EM_UNIT = 1 / EM_COUNT;

// height fog
// x: intensity, y: distance, z: height, w: height far
float4 _pmHFParam[EM_COUNT];
half4 _pmHFColor[EM_COUNT];
half4 _pmHFColor1[EM_COUNT];
// x: near enable, y: near, z: near scale
half4 _pmHFNearParam[EM_COUNT];
// x: scale, y: strength, zw: speed
half4 _pmHFNoiseParam[EM_COUNT];
sampler2D _pmHFLut;

// map
sampler2D _emMap;
float3 _emMapStartPos;
float3 _emMapSize;
sampler2D _emDitherTex;
half _emDitherStrength;
half _emDitherScale;
// x: 相机默认高度, y: 衰减高度
half4 _emHFCamPosYParam;

//-------------------------------------------------------------------------------
// height fog
// x: intensity, y: distance, z: height, w: height far
inline float4 GetHFParam(int index, int blendIndex, half blend)
{
	#ifdef _EM
		return _pmHFParam[blendIndex] * (1 - blend) + _pmHFParam[index] * blend;
	#else
		return _HFParam;
	#endif
}

inline float4 GetHFParam(int index)
{
	#ifdef _EM
		return _pmHFParam[index];
	#else
		return _HFParam;
	#endif
}

// x: near enable, y: near, z: near scale
inline half4 GetHFNearParam(int index, int blendIndex, half blend)
{
	#ifdef _EM
		return _pmHFNearParam[blendIndex] * (1 - blend) + _pmHFNearParam[index] * blend;
	#else
		return _HFNearParam;
	#endif
}


inline half4 GetHFNearParam(int index)
{
	#ifdef _EM
		return _pmHFNearParam[index];
	#else
		return _HFNearParam;
	#endif
}

// x: scale, y: strength, zw: speed
inline half4 GetHFNoiseParam(int index, int blendIndex, half blend)
{
	#ifdef _EM
		return _pmHFNoiseParam[blendIndex] * (1 - blend) + _pmHFNoiseParam[index] * blend;
	#else
		return _HFNoiseParam;
	#endif
}

inline half4 GetHFNoiseParam(int index)
{
	#ifdef _EM
		return _pmHFNoiseParam[index];
	#else
		return _HFNoiseParam;
	#endif
}

inline half4 GetHFColor(int index, int blendIndex, half blend)
{
	#ifdef _EM
		return _pmHFColor[blendIndex] * (1 - blend) + _pmHFColor[index] * blend;
	#else
		return _HFColor;
	#endif
}

inline half4 GetHFColor(int index)
{
	#ifdef _EM
		return _pmHFColor[index];
	#else
		return _HFColor;
	#endif
}

inline half4 GetHFColor1(int index, int blendIndex, half blend)
{
	#ifdef _EM
		return _pmHFColor1[blendIndex] * (1 - blend) + _pmHFColor1[index] * blend;
	#else
		return _HFColor1;
	#endif
}

inline half4 GetHFColor1(int index)
{
	#ifdef _EM
		return _pmHFColor1[index];
	#else
		return _HFColor1;
	#endif
}

// inline float4 GetHFLut(half v, int index, int blendIndex, half blend)
// {
// 	#ifdef _EM	
// 		half4 lut = tex2D(_pmHFLut, half2(v, index * EM_UNIT));
// 		lut += tex2D(_pmHFLut, half2(v + 0.01f, index * EM_UNIT));
// 		lut += tex2D(_pmHFLut, half2(v - 0.01f, index * EM_UNIT));
// 		lut = lut / 3;

// 		half4 lutBlend = tex2D(_pmHFLut, half2(v, blendIndex * EM_UNIT));
// 		lutBlend += tex2D(_pmHFLut, half2(v + 0.01f, blendIndex * EM_UNIT));
// 		lutBlend += tex2D(_pmHFLut, half2(v - 0.01f, blendIndex * EM_UNIT));
// 		lutBlend = lutBlend / 3;

// 		return lutBlend * (1 - blend) + lut * blend;
// 	#else
// 		return tex2D(_FogLut, half2(v, 1));
// 	#endif
// }

inline float4 GetHFLut(half v, int index)
{
	#ifdef _EM	
		half4 lut = tex2D(_pmHFLut, half2(v, index * EM_UNIT));
		return lut;
	#else
		return tex2D(_FogLut, half2(v, 1));
	#endif
}

//-------------------------------------------------------------------------------
// map
// 获取环境id
// r: em id, g: em 混合id, b: em 混合系数
inline int YoukiaEMId(float3 worldPos, out int emBlendId, out half emBlend)
{
	emBlendId = 0;
    emBlend = 1;
    #ifdef _EM
        half2 uv = ((worldPos - _emMapStartPos) / _emMapSize).xz;
		// half dither = YoukiaDither(worldPos.xz * _emDitherScale) * 2 - 1;
		half dither = tex2D(_emDitherTex, worldPos.xz * _emDitherScale) * 2 - 1;
		uv = saturate(1 - uv + dither * _emDitherStrength);
        half4 em = tex2Dlod(_emMap, half4(uv, 0, 0));
		em = saturate(em);

        int id = round(em.r * EM_IDUNIT);
        emBlendId = round(em.g * EM_IDUNIT);
        emBlend = saturate(em.b);
        return id;
    #else
        return 0;
    #endif
}

#endif
