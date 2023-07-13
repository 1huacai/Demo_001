#ifndef YOUKIAIMAGEEFFECT_INCLUDED
#define YOUKIAIMAGEEFFECT_INCLUDED

struct appdataImg
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2fImg
{
    float4 pos: POSITION;
    float2 uv: TEXCOORD0;	
};

v2fImg vertImg(appdataImg v, float4 texelSize, float4 mainTex_ST, sampler2D mainTex)
{
    v2fImg o;
    o.pos = UnityObjectToClipPos(v.vertex);

    #if UNITY_UV_STARTS_AT_TOP
        if (texelSize.y < 0)
        v.uv.y = 1 - v.uv.y;
    #endif

    o.uv = TRANSFORM_TEX(v.uv, mainTex);

    return o;
}


#endif