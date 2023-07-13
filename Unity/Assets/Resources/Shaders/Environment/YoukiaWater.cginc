#ifndef YOUKIAWATER_INCLUDED
#define YOUKIAWATER_INCLUDED

const static half SPECMAX = 12;

half4 _SpecularColor;
sampler2D _Wave1Tex;
sampler2D _Wave2Tex;
half4 _Wave1Tex_ST;
half4 _Wave2Tex_ST;

half _UVWorldSpace;

half _Distort;
half _Shinniess;
half4 _Dir;
half _TextureScale;

// ref
fixed4 _RefColor;
sampler2D _gGrabTex;
#ifdef _REFLECT
    sampler2D _gReflectTex;
#endif
half _RefFactor, _Fresnel;

half _Envir;
half _MinNdL;

// post process fog
sampler2D _HeightFogTex;

half3 _SpecularColor2;
half _Shinniess2, _Specular2, _SpecularScale;
half4 _CustomLightDir;
//half _CameraLight, _CustomLight;

// foam
half _FoamFade, _FoamWidth, _FoamNoise, _FoamCut, _FoamSurface;

// Caustics
half4 _CausticsColor;
sampler2D _CausticsTex;
half _CausticsScale, _CausticsSpeed, _CausticsDistort;

struct appdata
{
    float4 vertex : POSITION;
    fixed2 texcoord : TEXCOORD0;
    half3 normal : NORMAL;
    half4 tangent : TANGENT;
    half4 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 pos : SV_POSITION;
    float4 uvWave : TEXCOORD0;
    float4 worldPos : TEXCOORD1;
    float4 proj : TEXCOORD2;
    half4 TtoW[3] : TEXCOORD3;

    half4 sh : TEXCOORD6;
    half4 vertColor : TEXCOORD7;

    YOUKIA_ATMOSPERE_DECLARE(8, 9)
    YOUKIA_HEIGHTFOG_DECLARE(10)

    half3 giColor : TEXCOORD11;
    half4 lightDir : TEXCOORD12;
    half4 viewDir : TEXCOORD13;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

inline half2 AlignWithGrabTexel(half2 uv) 
{
    #if UNITY_UV_STARTS_AT_TOP
        if (YoukiaDepthTextureTexelSize().y < 0)
            uv.y = 1 - uv.y;
    #endif

    uv = (floor(uv * YoukiaDepthTextureTexelSize().zw) + 0.5) * abs(YoukiaDepthTextureTexelSize().xy);

    return uv;
}

inline half DepthDiff(float4 proj, half2 uvOffset, in out half2 uv, in out float depth)
{
    half surfaceDepth = proj.z;

    uv = AlignWithGrabTexel((proj.xy + uvOffset) / proj.w);
    depth = YoukiaDepth(uv);
    half linearEyeDepth = LinearEyeDepth(depth);
    half depthDifference = linearEyeDepth - surfaceDepth;

    return depthDifference;
}

inline float CalcDepthEdge(half2 uvScreen, half2 normal, half distort, float4 proj, half vertexAlpha, out half linearDepth, out half2 uv, out half edgeFade, out half depthDifference, EMData data = (EMData)0)
{
    half2 uvOffset = normal.xy * distort;
    float depth;
    depthDifference = DepthDiff(proj, uvOffset, uv, depth);
    uvOffset *= saturate(depthDifference);
    depthDifference = DepthDiff(proj, uvOffset, uv, depth);

    linearDepth = 1 - saturate(exp2(-GetWaterFadeDepthPower(data) * (depthDifference + GetWaterFadeDepth(data))));
	edgeFade = 1 - saturate(exp2(-_FoamFade * depthDifference));
    edgeFade = edgeFade * vertexAlpha;

    return depth;
}

inline half3 Caustics(half2 uvScreen, half2 normal, float depth)
{
    #if _CAUSTICS
        // reconstruct ws pos
        float3 wsPos = GetWorldPositionFromLinearDepthValue(uvScreen, Linear01Depth(depth)).xyz;
        half textureScale = FastSign(_TextureScale);
        half distort = normal * _CausticsDistort * 2 - 1;

        float2 speed = textureScale * _Time.xx * _Dir.xy * _CausticsSpeed;
        half3 caustics = tex2D(_CausticsTex, wsPos.xz * _CausticsScale * _Wave1Tex_ST + distort + speed);

        speed = textureScale * _Time.xx * _Dir.zw * _CausticsSpeed;
        caustics += tex2D(_CausticsTex, wsPos.xz * _CausticsScale * _Wave2Tex_ST + distort + speed);
        
        return caustics * _CausticsColor;
    #else
        return 0;
    #endif
}

inline half4 PostHeightFog(float3 wsPos, half4 colorFinal)
{
    #if _PP_HEIGHTFOG
        half4 fog = PostHeightFog(wsPos);
        // half4 fog = tex2D(_HeightFogTex, uvScreen);
        colorFinal.rgb = lerp(colorFinal.rgb, fog.rgb, fog.a);
    #endif

    return colorFinal;
}

// 补高光
inline void SecondSpecular(in out half3 waterSpecular, half3 normal1, half3 normal2, half3 lightDir, half3 viewDir, half3 TtoW[3])
{
    UNITY_BRANCH
    if (_Specular2 > 0)
    {
        half3 normalSpec = normal1 + normal2;
        normalSpec.xy *= _SpecularScale;
        normalSpec.z = sqrt(1.0 - saturate(dot(normalSpec.xy, normalSpec.xy)));
        normalSpec = normalize(half3(dot(TtoW[0].xyz, normalSpec), dot(TtoW[1].xyz, normalSpec), dot(TtoW[2].xyz, normalSpec)));
        
        //half3 customDir = normalize(_CustomLightDir.xyz);
        //lightDir = lerp(lightDir, camFwd, _CustomLight);

        half3 h = normalize(lightDir + viewDir);

        half nh2 = max(0, dot(normalSpec, h));
        half spec2 = max(0, pow(nh2, _Shinniess2 * 128));

        waterSpecular += _SpecularColor2.rgb * spec2;
    }
}

inline void SecondSpecular(in out half3 waterSpecular, half3 normal, half3 lightDir, half3 viewDir, half3 TtoW[3])
{
    UNITY_BRANCH
    if (_Specular2 > 0)
    {
        half3 normalSpec = normal;
        normalSpec.xy *= _SpecularScale;
        normalSpec.z = sqrt(1.0 - saturate(dot(normalSpec.xy, normalSpec.xy)));
        normalSpec = normalize(half3(dot(TtoW[0].xyz, normalSpec), dot(TtoW[1].xyz, normalSpec), dot(TtoW[2].xyz, normalSpec)));
        
        //half3 customDir = normalize(_CustomLightDir.xyz);
        //lightDir = lerp(lightDir, camFwd, _CustomLight);

        half3 h = normalize(lightDir + viewDir);

        half nh2 = max(0, dot(normalSpec, h));
        half spec2 = max(0, pow(nh2, _Shinniess2 * 128));

        waterSpecular += _SpecularColor2.rgb * spec2;
    }
}

inline void SecondSpecular(in out half3 waterSpecular, half3 normal1, half3 normal2, half3 lightDir, half3 viewDir, half4 TtoW[3])
{
    half3 t2w[3];
    t2w[0] = TtoW[0].xyz;
    t2w[1] = TtoW[1].xyz;
    t2w[2] = TtoW[2].xyz;
    SecondSpecular(waterSpecular, normal1, normal2, lightDir, viewDir, t2w);
}

inline void SecondSpecular(in out half3 waterSpecular, half3 normal, half3 lightDir, half3 viewDir, half4 TtoW[3])
{
    half3 t2w[3];
    t2w[0] = TtoW[0].xyz;
    t2w[1] = TtoW[1].xyz;
    t2w[2] = TtoW[2].xyz;
    SecondSpecular(waterSpecular, normal, lightDir, viewDir, t2w);
}

inline void SecondSpecularBigmap(in out half3 waterSpecular, half3 normal, half3 viewDir, half3 lightDir)
{
    half3 h = normalize(lightDir + viewDir);

    half nh2 = max(0, dot(normal, h));
    half spec2 = max(0, pow(nh2, _Shinniess2 * 128));

    waterSpecular += _SpecularColor2.rgb * spec2;
}

// foam
inline half4 Foam(in out half foam, half depthDifference, half noise)
{
    foam = 1 - saturate(exp2(-_FoamWidth * depthDifference));

    half foamNoise = noise * _FoamNoise;

    foam = lerp(foam, foamNoise, foam); 
    foam = saturate(step(_FoamCut, foam));

    half4 colFoam = foam * _FoamColor;

    return colFoam;
}


inline half4 Foam_waterfall(in out half foam, half depthDifference, half noise, half3 vertColor)
{
    foam = 1 - saturate(exp2(-_FoamWidth * depthDifference));

    half foamNoise = noise * _FoamNoise;

    foam = lerp(foam, foamNoise, foam); 
    half foam_edge = saturate(step(_FoamCut, foam));
    half foam_surface = saturate(step(_FoamSurface, foam)) * vertColor.g;

    half foam_total = saturate(foam_edge + foam_surface);    

    half4 colFoam = foam_edge * _FoamColor;

    return colFoam;
}


#endif