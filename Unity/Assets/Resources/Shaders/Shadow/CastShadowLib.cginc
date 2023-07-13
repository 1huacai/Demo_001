#ifndef CASTSHADOWLIB_INCLUDED
#define CASTSHADOWLIB_INCLUDED

#include "UnityCG.cginc" 
#include "../Library/ShadowLibrary.cginc"
#include "../Library/YoukiaCommon.cginc"
#include "../Library/YoukiaEnvironment.cginc"
#include "../Library/YoukiaEffect.cginc"

half _ShadowAlpha;
half _gCSMBiasScale;

half ShadowDepth(inout float2 pos)
{
    #ifndef _SHADOW_SSS
        half bias = GetBias(_bias) * _gCSMBiasScale;
        #if defined(UNITY_REVERSED_Z)
            pos.x += max(-1, min(bias / pos.y, 0));
        #else
            // 移动端 bias 额外加最小值 .001f
            pos.x += saturate(bias / pos.y) + 0.001f;
        #endif
    #endif

    return 0;

    // half depth = DepthHelper(pos);
    // return depth;
}

half4 EncodeShadow(half depth)
{
    return depth;
    // return EncodeFloatRGBA(depth);
}

struct a2v
{
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 vertex : POSITION;
    half4 texcoord : TEXCOORD0;
    #if _UV2_FADE
        half4 texcoord1 : TEXCOORD1;
    #endif
    half3 normal : NORMAL;
};

struct a2v_color
{
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 vertex : POSITION;
    half4 texcoord : TEXCOORD0;
    half3 color : COLOR;
    half3 normal : NORMAL;
};

struct v2f
{
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 pos : SV_POSITION;
    half3 depth : TEXCOORD0;

    #if _USE_DISSOLVE
        half2 uvDissolve : TEXCOORD1;
    #endif

    #if _UV2_FADE
        half2 uv1 : TEXCOORD2;
        float4 worldPos : TEXCOORD3;
    #endif
};

struct a2vGPU
{
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 vertex : POSITION;
    half4 texcoord : TEXCOORD0;
    half4 uv2 : TEXCOORD1;
    half4 uv3 : TEXCOORD2;
    half3 normal : NORMAL;
};

inline float4 NormalBias(float4 vertex, half3 normal, out float4 wPos)
{
    wPos = mul(unity_ObjectToWorld, vertex);

    #ifndef _SHADOW_SSS
        float3 wNormal = UnityObjectToWorldNormal(normal);
        float3 wLight = normalize(UnityWorldSpaceLightDir(wPos.xyz));
        float shadowCos = dot(wNormal, wLight);
        float shadowSine = sqrt(1 - shadowCos * shadowCos);
        float normalBias = _normalBias * shadowSine;
        wPos.xyz -= wNormal * normalBias;
    #endif

    return mul(UNITY_MATRIX_VP, wPos);
}

v2f vert(a2v v)
{
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    float4 worldPos;
    o.pos = NormalBias(v.vertex, v.normal, worldPos);
    o.depth.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

    o.depth.z = ShadowDepth(o.pos.zw);
    // o.depth.z = COMPUTE_DEPTH_01;

    #if _USE_DISSOLVE
        o.uvDissolve = TRANSFORM_TEX(v.texcoord, _DissolveTex);
    #endif

    #if _UV2_FADE
        o.uv1 = v.texcoord1;
        o.worldPos = worldPos;
    #endif

    return o;
}

v2f vertPlant(a2v_color v)
{
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    #if defined (_TREE)
        #ifdef _GWIND
            v.vertex.xyz += YoukiaWind(v.color, v.vertex.xyz);
        #endif
    #elif defined (_GRASS)
            #ifdef _GWIND
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            v.vertex.xyz += YoukiaGrassWind_Ver02(worldPos, v.color.rgb);
        #endif
    #else
        v.vertex.xyz = YoukiaWind_BigmapPlant(v.color, 1);
    #endif
    
    float4 wsPos;
    o.pos = NormalBias(v.vertex, v.normal, wsPos);
    o.depth.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

    o.depth.z = ShadowDepth(o.pos.zw);

    #if _USE_DISSOLVE
        o.uvDissolve = TRANSFORM_TEX(v.texcoord, _DissolveTex);
    #endif

    return o;
}

v2f vertGPU(a2vGPU v)
{
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    
    fixed3 normal = v.normal;
    float4 vertex = v.vertex;
    half4 tangent = 0;
    #ifdef _GPUAnimation
        #if defined (_4BONES)
            vertex = AnimationSkinningFourBoneWeight(v.uv2, v.uv3, v.vertex, normal, tangent);
        #elif defined (_BIGMAPUV2)
            vertex = AnimationSkinning(v.uv3, v.vertex);
        #else
            vertex = AnimationSkinning(v.uv2, v.vertex);
        #endif
    #endif

    float4 worldPos;
    o.pos = NormalBias(vertex, normal, worldPos);

    #if defined(_BIGMAPUV2) || defined(_MonsterLod2)
        o.depth.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
    #endif
    o.depth.z = ShadowDepth(o.pos.zw);

    #if _USE_DISSOLVE
        o.uvDissolve = TRANSFORM_TEX(v.texcoord, _DissolveTex);
    #endif

    return o;
}


// frag
half4 frag(v2f i) : SV_Target
{	
    UNITY_SETUP_INSTANCE_ID(i);

    #if _MonsterLod2
        DeathDissolveClip(i.depth.xy);
    #endif

    #if defined(_UV2_FADE)
		// dissolve
        #if _USE_DISSOLVE
            half dissolveAlpha = lerp(i.uv1.y, 1 - i.uv1.y, _DissolveReverse);
            half dissolveStrength = clamp(_DissolveStrength - dissolveAlpha, 0.0f, 1.1f);
            DissolveNoColor(i.worldPos.xz * _DissolveTexScale, dissolveStrength, _DissolveNoiseStrength);
        #endif
	#endif

    return 0;

    // half depth = i.depth.z;
    // return EncodeShadow(depth);
}

half4 fragCharacterPBR(v2f i) : SV_Target
{	
    fixed4 texcol = tex2D(_MainTex, i.depth.xy);
    clip(texcol.a * _Color.a - _Cutoff - _ShadowAlpha);

    half dissAmount = 0;
    half test = 0;
    #if _USE_DISSOLVE
        DissolveClip(i.uvDissolve, dissAmount, test);
    #endif

    return 0;

    // half depth = i.depth.z;
    // return EncodeShadow(depth);
}

half4 fragPlant(v2f i) : SV_Target
{	
    UNITY_SETUP_INSTANCE_ID(i);
    
    fixed4 texcol = tex2D(_MainTex, i.depth.xy);
    clip(texcol.a * _Color.a - _Cutoff);

    return 0;

    // half depth = i.depth.z;
    // return EncodeShadow(depth);
}

half4 fragBigMapUV2(v2f i) : SV_Target
{	
    UNITY_SETUP_INSTANCE_ID(i);

    fixed4 c = tex2D(_MainTex, i.depth.xy);
    half alpha = c.a * _Color.a;
    clip(alpha - _Cutoff);

    return 0;

    // half depth = i.depth.z;
    // return EncodeShadow(depth);
}

#endif