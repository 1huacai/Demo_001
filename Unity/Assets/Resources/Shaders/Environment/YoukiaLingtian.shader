//@@@DynamicShaderInfoStart
//灵田 地块
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Environment/YoukiaLingtian" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_MainTex("纹理 (世界坐标uv-rgb: color, a: ao)", 2D) = "white" {}
        [Header(Normal Metallic)]
        [NoScaleOffset] _BumpMetaMap("法线-金属度纹理 (世界坐标uv-rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [HideInInspector][Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[HideInInspector][Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(Others)]
        _MetallicMap ("法线-透明纹理(RG: 法线, B: 颜色插值, A: alpha)", 2D) = "white" {}
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        _AOCorrect ("AO 矫正", Range(0, 4)) = 0
        [Header(Emission)]
        [NoScaleOffset] _EmissionTex ("自发光纹理", 2D) = "black" {}
        [HDR]_ColorEmission("自发光颜色", Color) = (0, 0, 0, 1)

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
		Tags { "Queue"="AlphaTest+6" "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" }

        CGINCLUDE
            #pragma multi_compile_instancing
            #pragma target 2.0

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"
            #include "../Library/YoukiaEaryZ.cginc"

            #pragma multi_compile __ _UNITY_RENDER
            #pragma multi_compile __ _SUB_LIGHT_S

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            sampler2D _EmissionTex;
            half4 _ColorEmission;

            struct appdata_t 
            {
                float4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2fbase
            {
                float4 pos : SV_POSITION;
                half4 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                half3 TtoW[3] : TEXCOORD2;
                half3 sh : TEXCOORD5;
                fixed3 viewDir : TEXCOORD6;
                fixed3 lightDir : TEXCOORD7;
                half3 giColor : TEXCOORD8;

                YOUKIA_ATMOSPERE_DECLARE(9, 10)
                YOUKIA_LIGHTING_COORDS(11, 12)
                YOUKIA_HEIGHTFOG_DECLARE(13)
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2fadd
            {
                float4 pos : SV_POSITION;
                half4 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                #if defined(UNITY_UV_STARTS_AT_TOP)
                    half3 TtoW[3] : TEXCOORD2;
                #else
                    half3 worldNormal : TEXCOORD2;
                #endif
                half3 sh : TEXCOORD5;

                UNITY_LIGHTING_COORDS(6, 7)

                fixed3 viewDir : TEXCOORD8;
                fixed3 lightDir : TEXCOORD9;

                half3 giColor : TEXCOORD10;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

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
                #pragma fragment fragbase
                #pragma multi_compile_fwdbase
                #pragma fragmentoption ARB_precision_hint_fastest
                
                // youkia height fog
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE

                v2fbase vertbase(appdata_t v) 
                {
                    v2fbase o;
                    UNITY_INITIALIZE_OUTPUT(v2fbase,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(o.worldPos.xz, _MainTex);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _MetallicMap);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);

                    fixed3 worldNormal;
                    T2W(v.normal, v.tangent, o.TtoW, worldNormal);

                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);
                    
                    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
                    // height fog
                    YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
                    // atmosphere
                    YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

                    return o;
                }

                FragmentOutput fragbase(v2fbase i) 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    
                    float3 worldPos = i.worldPos;
                    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;

                    // rgb: color, a: ao
                    fixed4 c = tex2D(_MainTex, i.uv.xy);
                    // rg: normal, b: Metallic, a: Roughness
                    fixed4 n = tex2D(_BumpMetaMap, i.uv.xy);
                    // rg: normal, b: 颜色插值, a: alpha
                    fixed4 m = tex2D(_MetallicMap, i.uv.zw);

                    fixed4 col;
                    col.rgb = lerp(c.rgb, c.rgb * _Color, m.b);
                    half alpha = m.a * _Color.a;
                    fixed3 albedo = col.rgb;

                    // 写固定值，防止美术修改乱掉
                    _Roughness = 1;
                    _Metallic = 1;
                    half metallic = MetallicCalc(n.b);
                    half smoothness = SmoothnessCalc(n.a);

                    // normal
                    fixed3 normal = UnpackNormalYoukia(m);
                    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
                    i.TtoW[0].z = normal.x;
                    i.TtoW[1].z = normal.y;
                    i.TtoW[2].z = normal.z;
                    normal = UnpackNormalYoukia(n);
                    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                    // ao
                    half3 ao = AOCalc(c.a);
                    ao = lerp(1, lerp(_AOColor.rgb, 1, ao), _AO);

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // light
                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);
                    albedo *= ao;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.diffuse *= ao;
                    gi.indirect.specular *= ao;

                    half oneMinusReflectivity;
                    half3 specColor;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // pbs
                    col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                    // sub light
                    #ifdef _SUB_LIGHT_S
                        UnityLight light = CreateUnityLight(_gVSLColor_S.rgb, _gVSLFwdVec_S);
                        col += BRDF_Unity_PBS_SUB(albedo, specColor, normal, viewDir, brdfPreData, light, atten);
                    #endif

                    // emission
                    half4 emissionTex = tex2D(_EmissionTex, i.uv);
                    col.rgb += _ColorEmission * emissionTex;
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

            Blend [_SrcBlend] One
			ZWrite Off
			ZTest Equal
			Cull[_cull]
            
            CGPROGRAM
                #pragma vertex vertadd
                #pragma fragment fragadd
                #pragma multi_compile_fwdadd
                #pragma fragmentoption ARB_precision_hint_fastest

                v2fadd vertadd(appdata_t v) 
                {
                    v2fadd o;
                    UNITY_INITIALIZE_OUTPUT(v2fadd, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.xy = TRANSFORM_TEX(o.worldPos.xz, _MainTex);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _MetallicMap);
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    #if defined(UNITY_UV_STARTS_AT_TOP)
                        half3 worldNormal = 0;
                        T2W(v.normal, v.tangent, o.TtoW, worldNormal);
                    #else
                        half3 worldNormal = o.worldNormal = UnityObjectToWorldNormal(v.normal);  
                    #endif
                    
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                    return o;
                }

                fixed4 fragadd(v2fadd i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
                    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;

                    // tex
                    // rgb: color, a: ao
                    fixed4 c = tex2D(_MainTex, i.uv.xy);
                    // rg: normal, b: Metalli, a: Roughness
                    fixed4 n = tex2D(_BumpMetaMap, i.uv.xy);
                    // rg: normal, a: alpha
                    fixed4 m = tex2D(_MetallicMap, i.uv.zw);

                    fixed4 col;
                    col.rgb = c.rgb * _Color.rgb;
                    half alpha = m.a * _Color.a;
                    fixed3 albedo = col.rgb;
                    
                    // gamma correct
                    // 写固定值，防止美术修改乱掉
                    _Roughness = 1;
                    _Metallic = 1;
                    half metallic = MetallicCalc(n.b);
                    half smoothness = SmoothnessCalc(n.a);

                    // normal
                    fixed3 worldNormal;
                    #if defined(UNITY_UV_STARTS_AT_TOP)
                        worldNormal = fixed3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z);
                        fixed3 normal = UnpackNormalYoukia(n);
                    #else
                        fixed3 normal = worldNormal = i.worldNormal;
                    #endif
                    
                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;

                    #if defined(UNITY_UV_STARTS_AT_TOP)
                        normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
                    #endif
                    
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;
                    
                    half oneMinusReflectivity;
                    half3 specColor;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    gi.indirect.specular *= colShadow;
                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);
                    
                    // pbs
                    #if defined(UNITY_UV_STARTS_AT_TOP)
                        col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
                    #else
                        col = BRDF_Unity_PBS_Add(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
                    #endif

                    col.a = alpha;

                    return col;
                }
                
            ENDCG
        }

        // Pass 
        // {
        //     Name "ShadowCaster"
		// 	Tags{ "LightMode" = "ShadowCaster" }

		// 	ZWrite On ZTest LEqual
            
        //     CGPROGRAM
        //     #pragma vertex vertShadow
        //     #pragma fragment fragShadowDetailCutOff
        //     #pragma multi_compile_instancing
        //     #pragma multi_compile_shadowcaster
        //     #include "UnityCG.cginc"
        //     #include "Lighting.cginc"

        //     #pragma fragmentoption ARB_precision_hint_fastest

        //     ENDCG
        // }

	}
	FallBack "VertexLit"
    // CustomEditor "YoukiaPBRDetialCutOff"
}
