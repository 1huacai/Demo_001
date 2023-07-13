#ifndef YOUKIACHARACTERLOD1_INCLUDED
#define YOUKIACHARACTERLOD1_INCLUDED

#include "YoukiaCharacter.cginc"
#include "YoukiaHair.cginc"

// 顶点alpha
half _VertAlpha;

//--------------------------------------------------------------------------------------------------
// shadow
half _ShadowAlpha;
half4 fragShadowCharacter(UNITY_POSITION(vpos), v2fShadow i) : COLOR 
{
    half4 color = tex2D(_MainTex, TRANSFORM_TEX(i.tex, _MainTex));
    clip(color.a * _Color.a - _Cutoff - _ShadowAlpha);

    half dissAmount = 0;
    half test = 0;
    #if _USE_DISSOLVE
        DissolveClip(i.tex.zw, dissAmount, test);
    #endif

    SHADOW_CASTER_FRAGMENT(i)
}

//--------------------------------------------------------------------------------------------------
// Sparkle
sampler2D _SparkleTex;
half4 _SparkleColor1, _SparkleColor2;
half _SparkleColor01;
half _SparkleScale;
half _SparkleSize;
half _SparkleSizeMin;
half _SparkleSpeedU, _SparkleSpeedV;
half _SparkleShine, _SparkleShineColor;
half _SparkleRange;
half4 _SparkleUVScale;

inline half ShineRandom(half f, half random)
{
    half shine = sin(_Time.y * f * (1 - random) + random) * 0.5 + 0.5;
    shine = lerp(1, shine, saturate(ceil(f)));

    return shine;
}

inline half4 SparkleCacul(half2 coord, half2 coordFloor, half2 coordCenter, half2 uvOffset, half intensity)
{
    half random = unity_noise_randomValue(coordFloor + uvOffset);
    half flg = ceil(intensity - random);

    half2 offset = random + half2(_Time.y * random * _SparkleSpeedU, _Time.y * random * _SparkleSpeedV);
    offset = frac(offset);

    half fU = saturate(sin(offset.x * UNITY_PI));
    half fV = saturate(sin(offset.y * UNITY_PI));

    // 重映射范围
    coordCenter += offset * 2 - 1 + uvOffset;

    // scale
    half fScale = ShineRandom(_SparkleShine, random);
    fScale *= fU * fV;

    half sparkleSize = lerp(min(_SparkleSize, _SparkleSizeMin), _SparkleSize, random) * fScale;
    half sparkle = 1 - saturate(length(coord - coordCenter) / sparkleSize);
    sparkle = sparkle * sparkle;// * sparkle * sparkle;

    // color
    half cFlg = ceil(_SparkleColor01 - random);
    half fColor = ShineRandom(_SparkleShineColor, random);
    fColor *= fU * fV;
    half3 sparkleColor = lerp(_SparkleColor1, _SparkleColor2, cFlg);
    // sparkleColor *= sparkle * flg * fColor; 

    return half4(sparkleColor, sparkle * flg * fColor);
}

inline half4 SparkleCacul(half2 coord, half2 coordFloor, half2 coordCenter, half intensity)
{
    half4 sparkle = SparkleCacul(coord, coordFloor, coordCenter, half2(0, 0), intensity);

    // 判断相邻3个区域
    half xFlg = sign(coord.x - 0.5f);
    half yFlg = sign(coord.y - 0.5f);
    sparkle += SparkleCacul(coord, coordFloor, coordCenter, half2(xFlg, 0), intensity);
    sparkle += SparkleCacul(coord, coordFloor, coordCenter, half2(0, yFlg), intensity);
    sparkle += SparkleCacul(coord, coordFloor, coordCenter, half2(xFlg, yFlg), intensity);

    sparkle.rgb *= 0.25f;
    return sparkle;
}

inline half3 Sparkle(half3 albedo, half2 uv, half3 normal, half3 viewDir)
{
    half4 sparkleTex = tex2D(_SparkleTex, uv);
    half nv = abs(dot(normal, viewDir));
    half intensity = sparkleTex.a * pow(nv, _SparkleRange);

    half2 coord = half2(uv.x * _SparkleScale, uv.y * _SparkleScale);
    coord *= _SparkleUVScale.xy;
    half2 coordFloor = floor(coord);
    coord -= coordFloor;
    half2 coordCenter = 0.5f;
    half4 sparkle = SparkleCacul(coord, coordFloor, coordCenter, intensity);
    sparkle.rgb *= sparkleTex.rgb;
    
    return lerp(albedo.rgb, sparkle.rgb, sparkle.a);
}

inline half3 Sparkle(half3 albedo, half2 uv, half3 normal, half3 viewDir, half mask)
{
    half nv = abs(dot(normal, viewDir));
    half intensity = mask * pow(nv, _SparkleRange);

    half2 coord = half2(uv.x * _SparkleScale, uv.y * _SparkleScale);
    coord *= _SparkleUVScale.xy;
    half2 coordFloor = floor(coord);
    coord -= coordFloor;
    half2 coordCenter = 0.5f;
    half4 sparkle = SparkleCacul(coord, coordFloor, coordCenter, intensity);
    
    return lerp(albedo.rgb, sparkle.rgb, sparkle.a);
}

#endif