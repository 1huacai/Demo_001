//@@@DynamicShaderInfoStart
//场景PBR反射（镜面反射）Shader。
//@@@DynamicShaderInfoEnd
// 反射参考天刀的做法 https://zhuanlan.zhihu.com/p/358074633

Shader "YoukiaEngine/Lit/YoukiaPBR-Reflect" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_MainTex("纹理 (rgb: color, a: AO(顶面贴花Mask))", 2D) = "white" {}
        [Header(Normal Metallic)]
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        [HideInInspector][Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse threshold", Range(0, 0.5)) = 0
        [Header(Others)]
        [NoScaleOffset] _MetallicMap ("(R: 透明度)", 2D) = "white" {}
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        _AOCorrect ("AO 矫正", Range(0, 4)) = 0
        
        [Header(Reflect)]
        _ReflectStrength ("Reflect Strength", Range(0, 1)) = 0.36
        _ReflectLod ("Reflect Blur", Range(0, 1)) = 0.22
        _ReflectFresnel ("Reflect fresnel", Range(4, 10)) = 8.5
        _ReflectContrast("对比度", Range(0, 5)) = 2.7
        _ReflectOffset ("偏移", Range(-1, 1)) = 0.25

        // [Header(Others)]
        // _Ref("Stencil Reference", Range(0, 255)) = 0
        // [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
        // [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0
	}
	SubShader 
    {
		Tags { "Queue"="AlphaTest+5" "RenderType"="Opaque" "ShadowType" = "ST_Terrain" }

        // Stencil
        // {
        //     Ref [_Ref]
        //     Comp [_Comp]
        //     Pass [_Pass]
        // }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"
            #include "../Library/YoukiaEaryZ.cginc"
            #include "YoukiaPBR.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

        ENDCG
        
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            ZWrite On
			ZTest LEqual
			Cull Back

			CGPROGRAM
                #pragma vertex vertbase
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _SUB_LIGHT_S

                // youkia height fog
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE

                static const half MIPMAPMAX = 10;
                #define REFLECTVALUE 0.999f

                half _ReflectStrength;
                half _ReflectLod;
                half _ReflectFresnel;
                half _ReflectContrast;
                half _ReflectOffset;
            
                sampler2D _gGrabTex;
                float4x4 _gReflectVP;

                FragmentOutput frag(v2fbase i)
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    
                    float3 worldPos = i.worldPos;
				    // half3 viewDir = i.viewDir;
                    half3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                    half3 lightDir = i.lightDir;
                    
                    // tex
                    // rgb: color, a: AO
                    fixed4 c = tex2D(_MainTex, i.uv);
                    // rg: normal, b: Metallic, a: Roughness
                    fixed4 n = tex2D(_BumpMetaMap, i.uv);
                    // r: transparent, g: lighting, b: sss
                    fixed4 m = tex2D(_MetallicMap, i.uv);
        
                    fixed4 col;
                    col.rgb = c.rgb * _Color.rgb;
                    half alpha = m.r * _Color.a;
                    fixed3 abledo = col.rgb;

                    // 写固定值，防止美术修改乱掉
                    _Roughness = 1;
                    _Metallic = 1;
                    half metallic = MetallicCalc(n.b);
                    half smoothness = SmoothnessCalc(n.a);

                    // normal
                    _NormalStrength = 1;
                    half3 normal = WorldNormal(n, i.TtoW);

                    // ao
                    // ao 矫正
                    // 固有色 srgb，ao不需要srgb，可以做gamma矫正
                    half3 ao = AOCalc(c.a);
                    ao = lerp(1, lerp(_AOColor.rgb, 1, ao), _AO);

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // light
                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);
                    abledo *= ao;
                    
                    UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.diffuse *= ao;
                    gi.indirect.specular *= ao;

                    half oneMinusReflectivity;
                    half3 specColor;
                    abledo = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // pbs
                    col = BRDF_Unity_PBS(abledo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                    // sub light
                    #ifdef _SUB_LIGHT_S
                        UnityLight light = CreateUnityLight(_gVSLColor_S.rgb, _gVSLFwdVec_S);
                        col += BRDF_Unity_PBS_SUB(abledo, specColor, normal, viewDir, brdfPreData, light, atten);
                    #endif

                    half up = saturate(dot(normal, half3(0, 1, 0)));
                    [branch]
                    if (up > REFLECTVALUE)
                    {
                        half2 reflectUV = ProjReflectCoord(_gReflectVP, worldPos);
                        half4 reflect = tex2D(_gGrabTex, reflectUV);
                        reflect = lerp(reflect, 0, saturate(reflectUV.y * reflectUV.y));

                        half f = saturate(pow(1 - brdfPreData.nv, _ReflectFresnel) * _ReflectStrength);
                        col.rgb += reflect * f * colShadow;
                    }

                    col.a = alpha;

                    // height fog
                    YOUKIA_HEIGHTFOG(col, i)
                    // atmosphere
                    YOUKIA_ATMOSPHERE(col, i)
                    
                    return OutPutDefault(col);
                }
			
			ENDCG
		}

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }

            Blend One One
			ZWrite Off
			ZTest LEqual
            Cull Back

            CGPROGRAM
                #pragma vertex vertadd
                #pragma fragment fragadd
                #pragma multi_compile_fwdadd

                #pragma multi_compile __ _UNITY_RENDER
                
            ENDCG
        }

        Pass 
        {
            Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadowDetail
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            ENDCG
        }

	}
	FallBack "VertexLit"
    CustomEditor "YoukiaPBRReflect"
}
