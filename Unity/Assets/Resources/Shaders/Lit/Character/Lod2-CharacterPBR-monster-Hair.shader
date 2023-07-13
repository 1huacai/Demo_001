//@@@DynamicShaderInfoStart
//Lod2 怪物毛发（与角色头发类似）
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod2-CharacterPBR-monster-Hair" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Metallic Roughness)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (R: Metallic, G: Roughness, B: HairMask, A: HairMaskSpec) #注意: B通道计算毛发光照并且绘制毛发纹理, A通道只计算毛发高光不绘制毛发纹理", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
	    [Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0.5
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        [Header(Shadow)]
        [HideInInspector]_ShadowStrength ("阴影强度", Range(0, 1)) = 0.6
        [Header(Hair)]
        // [Toggle(_ANISOTROPIC)]_Toggle_ANISOTROPIC_ON("各项异性高光", Float) = 0
        _HairTex ("纹理(rg: 各向异性高光贴图 b:ao )", 2D) = "white" {}
        [Header(Anisotropic)]
        [HDR]_AnisoColor_0 ("Color 0", Color) = (0, 0, 0, 1)
        [HDR]_AnisoColor_1 ("Color 1", Color) = (0, 0, 0, 1)
        _AnisoShift_0 ("Anisotropy bias R (偏移 ", Range(-10, 10)) = 0
        _AnisoShift_1 ("Anisotropy bias G (偏移", Range(-10, 10)) = 0
        _AnisoExpo_0 ("Anisotropy ring 0 (集中度 对应Color 0", Range(1, 1000)) = 1
        _AnisoExpo_1 ("Anisotropy ring 1 (集中度 对应Color 1", Range(1, 1000)) = 1
        [Space(5)]
        _HLFrePower ("HL fre power", Range(0, 5)) = 1
        [Space(5)]
        _SpecularInDark("Specular(阴影高光矫正)", Color) = (0.4720924, 0.4571259, 0.6544118,0)
        
        [Header(Emission)]
        // [Toggle(_EMISSION)]_Toggle_EMISSION_ON("自发光", Float) = 0
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)
        [Header(Anim)]
		_AnimMap("AnimMap", 2D) = "white" {}
		_AnimControl("AnimControl", vector) = (0,0,0,0)
		_AnimStatus("_AnimStatus", vector) = (0,0,0,0)
        // [Header(CutOut)]
        // [MaterialToggle] _EnableCutOff ("CutOff", float) = 0
        // _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0
        [Header(Hit)]
        [MaterialToggle] _Hit ("Hit", float) = 0
        _HitIntensity ("Hit Intensity", Range(0, 1)) = 0

        // Ignite
        [Header(Ignite)]
        [MaterialToggle] _Ignite ("Ignite", float) = 0
        _IgniteIntensity ("Ignite intensity", Range(0, 1)) = 0
    
        [Header(GPU)]
        [Toggle(_GPUAnimation)] _GPUAnimation("Enable GPUAnimation", Float) = 0

        [Header(Others)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
	}
	SubShader 
    {
		Tags { "Queue"="AlphaTest-10" "RenderType"="Opaque" "ShadowType" = "ST_Monster_Lod2" "Reflection" = "RenderReflectionOpaque" }
        LOD 100

        Stencil
        {
            Ref 1
            Pass Replace
        }
        
        CGINCLUDE
            #pragma multi_compile_instancing
            #define _CHARACTER_LOW 1
            #define _MonsterLod2 1

            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "../../Library/YoukiaEffect.cginc"
            #include "../../Library/GPUSkinningLibrary.cginc"
            #include "../../Library/YoukiaMrt.cginc"
            #include "YoukiaCharacterLod2.cginc"

            #pragma multi_compile __ _UNITY_RENDER
            #pragma shader_feature _GPUAnimation

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            sampler2D _HairTex;
            half4 _HairTex_ST;

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
                #pragma shader_feature _USE_DISSOLVE


                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 TtoW[3] : TEXCOORD2;  
                    half3 sh : TEXCOORD5;
                    fixed3 viewDir : TEXCOORD6;
                    fixed3 lightDir : TEXCOORD7;
                    half3 giColor : TEXCOORD8;

                    YOUKIA_ATMOSPERE_DECLARE(9, 10)
                    YOUKIA_LIGHTING_COORDS(11, 12)
                    YOUKIA_HEIGHTFOG_DECLARE(13)
                    // eff
                    CHARACTER_EFFECT_DECLARE(14)

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_gpu v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    
                    fixed3 normal = v.normal;
                    half4 vertex = v.vertex;
                    half4 tangent = v.tangent;
                    #ifdef _GPUAnimation
                        vertex = AnimationSkinning(v.uv2, v.vertex, normal, tangent);
                    #endif

                    o.pos = UnityObjectToClipPos(vertex, o.worldPos);
                    o.uv.xy = v.texcoord;
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _HairTex);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    half3 worldNormal = 0;
                    T2W(normal, tangent, o.TtoW, worldNormal);
                    
                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);

                    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);

                    YOUKIA_TRANSFER_HEIGHTFOG(o, 0)

                    // atmosphere
                    YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

                    // eff
                    half nv = saturate(dot(worldNormal, o.viewDir.xyz));
                    CHARACTER_EFFECT_VERT(nv, o.effColor);

                    return o;
                }

                FragmentOutput frag(v2f i) 
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, i.uv);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, i.uv);
                    
                    // r: m, g: r
                    fixed4 m = tex2D(_MetallicMap, i.uv);
                    fixed3 hair_c = tex2D(_HairTex, i.uv.zw).xyz;
                    hair_c.rgb = lerp(1, hair_c.b, _AO);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    fixed3 abledo = col.rgb;

                    // death dissolve
                    half4 dissolveColor = DeathDissolve(i.uv);

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessCalc(m.g);
                    half hair = m.b;
                    half hair_spec = m.a;

                    // normal
                    half3 normal = WorldNormal(n, i.TtoW);
                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor.rgb);
                    #endif

                    // ao
                    half3 ao = pow(n.b, _AOCorrect);
                    ao = lerp(1, lerp(_AOColor.rgb, 1, ao), _AO);

                    // shadow
                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);
                    abledo *= ao;

                    UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor.rgb);
                    CharacterGIScale(gi, normal, lightDir, colShadow, ao);

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor = 0;
                    col.rgb = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // Anisotropic
                    YoukiaAnisoData aniso = (YoukiaAnisoData)0;
                    fixed3 b = half3(i.TtoW[0].y, i.TtoW[1].y, i.TtoW[2].y);

                    half anisoNoise1 = hair_c.r;
                    half anisoNoise2 = hair_c.g;
                    half3 hairAlbedo = lerp(hair_c.rgb * col.rgb, col.rgb, ceil(hair_spec));
                    
                    aniso = YoukiaAniso(b, _AnisoColor_0, _AnisoShift_0 * anisoNoise1, _AnisoExpo_0, _AnisoColor_1, _AnisoShift_1 * anisoNoise2, _AnisoExpo_1);

                    half hairmask = saturate(hair + hair_spec);
                    hairmask = saturate(ceil(hairmask));
                    col = BRDF_HairMix_PBS(col.rgb, hairAlbedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten, aniso, c.rgb, hairmask);

                    // sub light
                    #ifdef _SUB_LIGHT_C
                        UnityLight light = CreateUnityLight(_gVSLColor_C.rgb, _gVSLFwdVec_C);
                        col += BRDF_HairMix_PBS_SUB(col.rgb, hairAlbedo, specColor, normal, viewDir, brdfPreData, light, aniso, c.rgb, hairmask);
                    #endif

                    #if _UNITY_RENDER
                        // rim light
                        col.rgb += CharacterRimLightFrag(normal, atten);
                    #endif

                    col.rgb = Emission(col.rgb, c.rgb, n.a);

                    col.a = alpha;

                    // height fog
                    YOUKIA_HEIGHTFOG(col, i)
                    
                    YOUKIA_ATMOSPHERE(col, i)

                    CHARACTER_EFFECT_FRAG(i.uv.xy, i.effColor, col.rgb);

                    // dissolve
                    col.rgb = lerp(col.rgb, dissolveColor.rgb, dissolveColor.a);
                    
                    return OutPutCharacter(col);
                }
			
			ENDCG
		}

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend [_SrcBlend] One
			ZWrite Off
			ZTest[_zTest]
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd
                #pragma fragmentoption ARB_precision_hint_fastest

                v2fadd vert(appdata_gpu v) 
                {
                    v2fadd o;
                    UNITY_INITIALIZE_OUTPUT(v2fadd,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    fixed3 normal = v.normal;
                    half4 vertex = v.vertex;
                    half4 tangent = 0;
                    #ifdef _GPUAnimation
                        vertex = AnimationSkinning(v.uv2, v.vertex, normal, tangent);
                    #endif

                    o.pos = UnityObjectToClipPos(vertex, o.worldPos);
                    o.uv.xy = v.texcoord;
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    o.worldNormal = UnityObjectToWorldNormal(normal);  

                    o.sh = ShadeSHPerVertex(o.worldNormal, o.sh);
                    o.sh += YoukiaGI_IndirectDiffuse(o.worldNormal, o.worldPos, o.giColor);

                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    return o;
                }

                fixed4 frag(v2fadd i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    half2 uv = i.uv.xy;

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, uv);
                    // r: m, g: r
                    fixed4 m = tex2D(_MetallicMap, uv);
                    
                    fixed4 col = c * _Color;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    half metallic = MetallicCalc(m.r);
                    half smoothness = SmoothnessCalc(m.g);

                    half3 normal = i.worldNormal;
                    
                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;

                    // pbs
                    half oneMinusReflectivity;
                    half3 specColor;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_Unity_PBS_Add(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                    col.a = alpha;

                    return col;
                }
            ENDCG
        }

        // shadow
        Pass 
        {
            Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }
	}
	FallBack "VertexLit"
    CustomEditor "Lod2CharacterPBRMonsterHairInspector"
}
