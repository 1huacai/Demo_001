//@@@DynamicShaderInfoStart
//Lod1 头发
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-CharacterPBR-Hair" 
{
	Properties 
    { 
        _Color("颜色", Color) = (1, 1, 1, 1)
        _AlbedoTex("颜色纹理", 2D) = "white" {}
		_MainTex("纹理(rg: 各向异性高光贴图 b:ao )", 2D) = "white" {}
        [Header(Normal)]
        _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Metallic Gloss)]
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0.2
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        [Header(Anisotropic)]
        // [Toggle(_ANISOTROPIC)]_Toggle_ANISOTROPIC_ON("各项异性高光", Float) = 0
        _AnisoColor_0 ("Color 0", Color) = (1, 1, 1, 1)
        _AnisoColor_1 ("Color 1", Color) = (1, 1, 1, 1)
        _AnisoShift_0 ("Anisotropy bias R", Range(-10, 10)) = 0
        _AnisoShift_1 ("Anisotropy bias G", Range(-10, 10)) = 0
        _AnisoExpo_0 ("Anisotropy rang 0", Range(1, 1000)) = 1
        _AnisoExpo_1 ("Anisotropy rang 1", Range(1, 1000)) = 1
        [Space(5)]
        _HLFrePower ("高光菲涅尔系数", Range(0, 5)) = 1
        [Space(5)]
        _SpecularInDark("Specular(阴影高光矫正)", Color) = (0.4720924, 0.4571259, 0.6544118,0)
        [Space(5)]
        _PbrIntensity ("PBR instensity", Range(0, 1)) = 1

        [Space(30)]
        [HideInInspector][Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[HideInInspector][Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
	}

	SubShader 
    {		
        Tags { "Queue"="Geometry" "RenderType" = "Opaque" "Reflection" = "RenderReflectionHair" "ShadowType" = "ST_CharacterPBR_Lod1" }
        
        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 3.0
            #define _LOD1_HAIR 1
            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "YoukiaCharacterLod1.cginc"

            // #pragma shader_feature _ANISOTROPIC
            // youkia height fog
            // #pragma multi_compile __ _HEIGHTFOG
            // #pragma multi_compile __ _SKYENABLE

            #pragma multi_compile __ _SUB_LIGHT_C
            #pragma multi_compile __ _UNITY_RENDER

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK
        
        ENDCG

        Pass 
        {
			Tags { "LightMode"="ForwardBase" "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }

            // Blend SrcAlpha OneMinusSrcAlpha
			// ZWrite Off
            // ZTest Equal
            // ColorMask RGB
			// Cull Off

            ZWrite On
			ZTest LEqual
            Cull Back

			CGPROGRAM
			
                #pragma vertex vert_forward
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                FragmentOutput frag(v2f_forward i)
                {
                    half3 normal = 0;
                    half atten = 0;
                    half4 color = frag_forward(i, normal, atten);
                    return OutPutCharacterLod1(color);
                }
			
			ENDCG
		}

        Pass
        {
            Tags { "LightMode"="ForwardAdd" "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }

            Blend One One
            ZWrite Off
			ZTest Equal

            CGPROGRAM
                #pragma vertex vert_add
                #pragma fragment frag_add
                #pragma multi_compile_fwdadd

            ENDCG
        }

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
    CustomEditor "YoukiaCharacterHairInspector"
}
