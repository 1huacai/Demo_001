//@@@DynamicShaderInfoStart
//Lod3 怪物PBR 无法线 gpu动画-2bone
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod3-CharacterPBR-monster" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Metallic Roughness)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (R: Metallic, G: Roughness, B: 自发光)", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 1

        [Header(Emission)]
        [HDR]_ColorEmission("自发光颜色#金属度光滑度贴图 B 通道", Color) = (0, 0, 0, 1)

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
        [Header(GPU)]
        [Toggle(_GPUAnimation)] _GPUAnimation("Enable GPUAnimation", Float) = 0
		_AnimMap("AnimMap", 2D) = "white" {}
		_AnimControl("AnimControl", vector) = (0,0,0,0)
		_AnimStatus("_AnimStatus", vector) = (0,0,0,0)
        
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
		Tags { "Queue"="AlphaTest-10" "RenderType"="Opaque" "ShadowType" = "ST_GPUSkinning" "Reflection" = "RenderReflectionOpaque" }

        // Stencil
        // {
        //     Ref 1
        //     Pass Replace
        // }
        
        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0
            #pragma shader_feature _GPUAnimation
            #define _MONSTER 1

            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "../../Library/YoukiaEffect.cginc"
            #include "../../Library/YoukiaMrt.cginc"
            #include "YoukiaCharacterLod3.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_GPUSkinning" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertbase
                #pragma fragment fragbase_monster
                #pragma multi_compile_fwdbase
                
                #pragma multi_compile __ _UNITY_RENDER
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
                #pragma multi_compile __ _EM
                
			ENDCG
		}

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend [_SrcBlend] One
			ZWrite Off
			ZTest[_zTest]
			Cull[_cull]
            // ColorMask RGB

            CGPROGRAM
                #pragma vertex vertadd
                #pragma fragment fragadd
                #pragma multi_compile_fwdadd

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
                #pragma fragment fragShadowCharacter
                #pragma multi_compile_shadowcaster
                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                #pragma fragmentoption ARB_precision_hint_fastest
                
            ENDCG
        }

	}
	FallBack "VertexLit"
    CustomEditor "Lod3CharacterPBRMonsterInspector"
}
