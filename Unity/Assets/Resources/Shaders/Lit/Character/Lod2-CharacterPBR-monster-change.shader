//@@@DynamicShaderInfoStart
//Lod2 变身怪物，噪声效果（如燃烧等），GPU动画-2bones。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod2-CharacterPBR-monster-change" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Metallic Roughness)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (R: Metallic, G: Roughness)", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
	    [Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0.5
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1

        [Header(Emission)]
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)
        _EmissionBreath ("自发光呼吸频率", Range(0, 10)) = 0

        [Header(Rim)]
        [HDR]_RimColor("边缘光颜色#根据顶点色计算边缘光范围。#黑色不显示边缘光。", Color) = (0, 0, 0, 1)
        _RimStrength("边缘光强度", Range(0, 1)) = 0
        _RimRange("边缘光范围", Range(0, 10)) = 0
        
        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0
        
        // change
        [Header(Change)]
        _ChangeIntensity ("变身系数", Range(0, 1)) = 0
        [HDR]_ChangeColor0 ("变身颜色0", Color) = (1, 1, 1, 1)
        [HDR]_ChangeColor1 ("变身颜色1", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_ChangeTex ("变身噪声纹理", 2D) = "white" {}
        _ChangeTexScale ("变身纹理缩放", Range(0, 5)) = 1
        _ChangeTexPow ("变身纹理 Pow", Range(0, 5)) = 1
        _ChangeBreathSpeed("breath speed", Range(0, 10)) = 1
        _ChangeBreathRange("breath range", Range(0, 1)) = 1
        _ChangeBreathOffset("breath offset", Range(0, 1)) = 0

        // hit
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
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C

                // change
                // UNITY_INSTANCING_BUFFER_START(ChangeProps)

                // UNITY_DEFINE_INSTANCED_PROP(half, _ChangeIntensity)

                // UNITY_INSTANCING_BUFFER_END(ChangeProps)

                // half4 _ChangeColor0, _ChangeColor1;
                // sampler2D _ChangeTex;
                // half _ChangeTexScale, _ChangeTexPow;
                // half _ChangeBreathSpeed, _ChangeBreathRange, _ChangeBreathOffset;

                FragmentOutput frag(v2fbase i) 
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    half nv = 0;
                    half4 col = fragbase(i, nv);

                    col.rgb = Rim(col.rgb, nv, i.giColor.w);

                    CHARACTER_EFFECT_FRAG(i.uv.xy, i.effColor, col.rgb);

                    // change 
                    // half intensity = UNITY_ACCESS_INSTANCED_PROP(ChangeProps, _ChangeIntensity);
                    // col.rgb = Ignite(intensity, i.uv, col.rgb, _ChangeTex, 
                    //     _ChangeTexScale, _ChangeTexPow, _ChangeColor0, _ChangeColor1,
                    //     _ChangeBreathSpeed, _ChangeBreathRange, _ChangeBreathOffset);

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
    CustomEditor "Lod2CharacterPBRMonsterChangeInspector"
}
