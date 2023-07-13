//@@@DynamicShaderInfoStart
// Lod2 怪物扩展PBR，包含SSS、各项异性等效果，GPU动画-2bones。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod2-CharacterPBR-monster-Uber" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Metallic Roughness)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (R: Metallic, G: Roughness, B: SSS, A: Aniso(各项异性))#注意：B(SSS)非黑色时, G通道按Specular计算，反之则按Roughness计算。#即皮肤计算Specular。", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
	    [Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        _Spec ("Specular", Range(0, 1)) = 0.5
        _Spec2 ("Specular 2", Range(0, 4)) = 1
        _SpecScale ("Specular 强度", Range(0, 2)) = 1

        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0.5
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        _AOSkinColor ("AO 皮肤颜色", Color) = (0, 0, 0, 1)

        [Header(Skin Edge)]
        _EdgeOffset ("皮肤边缘范围", Range(0, 0.5)) = 0.2
        _EdgeStrength ("皮肤边缘强度 #皮肤边缘效果只控制SSS(皮肤)部分。", Range(0, 1)) = 0.5

        // sss 散射
        [Header(Scattering)]
        // [Toggle(_SCATTERING)]_Toggle_SCATTERING_ON("Scattering", Float) = 0
        [NoScaleOffset]_SkinMap ("SSS RampMap", 2D) = "white" {}
        _SkinToneScale("SSS scale", Range(0, 1)) = 0.5
        _SkinToneOffset("SSS offset", Range(0, 0.2)) = 0

        [Header(Anisotropic)]
        // [Toggle(_ANISOTROPIC)]_Toggle_ANISOTROPIC_ON("各项异性高光", Float) = 0
        _Anositropy ("Anositropy", Range(0, 1)) = 1
        _AnisoDir ("Anisotropy Direction", Range(0, 360)) = 0
        [HideInInspector]_SpecAnisoScale ("各项异性-固有色颜色差值", range(0, 1)) = 0
        [HideInInspector][HDR]_SpecAnisoColor ("各项异性自定义颜色", Color) = (1, 1, 1, 1)

        [Header(Emission)]
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)
        _EmissionBreath ("自发光呼吸频率", Range(0, 10)) = 0

        [Header(Rim)]
        [HDR]_RimColor("边缘光颜色", Color) = (0, 0, 0, 1)
        _RimStrength("边缘光强度", Range(0, 1)) = 0
        _RimRange("边缘光范围", Range(0, 10)) = 0
        
        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0
        [Header(Hit)]
        [MaterialToggle] _Hit ("Hit", float) = 0
        _HitIntensity ("Hit Intensity", Range(0, 1)) = 0

        // Ignite
        [Header(Ignite)]
        [MaterialToggle] _Ignite ("Ignite", float) = 0
        _IgniteIntensity ("Ignite intensity", Range(0, 1)) = 0

        [Header(Anim)]
		_AnimMap("AnimMap", 2D) = "white" {}
		_AnimControl("AnimControl", vector) = (0,0,0,0)
		_AnimStatus("_AnimStatus", vector) = (0,0,0,0)
        
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
            #define _LOD2_UBER 1
            #define _MonsterLod2 1
            #define _CHARACTER_LOW 1

            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "../../Library/YoukiaEffect.cginc"
            #include "../../Library/GPUSkinningLibrary.cginc"
            #include "../../Library/YoukiaMrt.cginc"
            #include "YoukiaCharacterLod2.cginc"

            #pragma shader_feature _GPUAnimation

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertbase
                #pragma fragment frag
                #pragma multi_compile_fwdbase
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma multi_compile __ _UNITY_RENDER
                #pragma shader_feature _SCATTERING
                #pragma shader_feature _ANISOTROPIC
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C

                FragmentOutput frag(v2fbase i) 
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    half nv = 0;
                    half4 col = fragbase(i, nv);

                    col.rgb = Rim(col.rgb, nv, i.giColor.w);

                    CHARACTER_EFFECT_FRAG(i.uv.xy, i.effColor, col.rgb);

                    return OutPutCharacter(col);
                }

			ENDCG
		}

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend [_SrcBlend] One
			ZWrite Off
			ZTest Equal
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertadd
                #pragma fragment fragadd
                #pragma multi_compile_fwdadd
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma multi_compile __ _UNITY_RENDER
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
    CustomEditor "Lod2CharacterPBRMonsterUberInspector"
}
