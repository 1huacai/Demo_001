Shader "YoukiaEngine/Obsolete/YoukiaTerrain001"
{
    Properties 
    {
        [NoScaleOffset]_BumpMetaMap ("法线-金属度纹理 (rg: normal, b: Metallic, a: Roughness)", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1
        [Header(Blend)]
        [NoScaleOffset]_Control_0 ("混合贴图 0", 2D) = "white" {}
        [NoScaleOffset]_Control_1 ("混合贴图 1", 2D) = "white" {}

        [Header(Splat)]
        _Color0("Color 0", Color) = (0.5, 0.5, 0.5, 1)
        _Color1("Color 1", Color) = (0.5, 0.5, 0.5, 1)
        _Color2("Color 2", Color) = (0.5, 0.5, 0.5, 1)
        _Color3("Color 3", Color) = (0.5, 0.5, 0.5, 1)
        _Color4("Color 4", Color) = (0.5, 0.5, 0.5, 1)
        _Color5("Color 5", Color) = (0.5, 0.5, 0.5, 1)
        _Color6("Color 6", Color) = (0.5, 0.5, 0.5, 1)
        _Splat0("纹理 0 (rgb: color, a: AO)", 2D) = "white" {}
        [NoScaleOffset] _BumpSplat0("法线-金属度纹理 0 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        _NormalStrength_0 ("Normal Strength 0", Range(0, 2)) = 1
        _Splat1("纹理 1 (rgb: color, a: AO)", 2D) = "white" {}
        [NoScaleOffset] _BumpSplat1("法线-金属度纹理 1 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        _NormalStrength_1 ("Normal Strength 1", Range(0, 2)) = 1
        _Splat2("纹理 2 (rgb: color, a: AO)", 2D) = "white" {}
        [NoScaleOffset] _BumpSplat2("法线-金属度纹理 2 (rg: normal, b: Metallic, a: Roughness)", 2D) = "white" {}
        _NormalStrength_2 ("Normal Strength 2", Range(0, 2)) = 1

        [Header(Normal Metallic)]
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        _AOCorrect ("AO 矫正", Range(0, 4)) = 0
	}
	SubShader 
    {
		Tags { "Queue"="AlphaTest+5" "RenderType"="Opaque" "ShadowType" = "ST_Terrain" "TerrainBlending" = "TB_T001"}

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"
            #include "YoukiaObsolete.cginc"

            // #pragma shader_feature _SHADOW_BLUR
            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            sampler2D _Splat0, _Splat1, _Splat2;
            half4 _Splat0_ST, _Splat1_ST, _Splat2_ST;

            sampler2D _BumpSplat0, _BumpSplat1, _BumpSplat2;
            half _NormalStrength_0, _NormalStrength_1, _NormalStrength_2;
            // half _NormalStrength, _NormalStrength_0, _NormalStrength_1, _NormalStrength_2;

            sampler2D _Control_0, _Control_1;
            half4 _Control_0_ST, _Control_1_ST;

            half4 _Color0, _Color1, _Color2, _Color3, _Color4, _Color5, _Color6;
            struct appdata_t 
            {
                float4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            inline void SampleTexs(half4 uv, half4 uv1, out fixed4 blend, out fixed4 blend_n, out fixed3 normal)
            {
                half4 blendmask0 = tex2D(_Control_0, uv.xy);
                half4 blendmask1 = tex2D(_Control_1, uv.xy);
                half4 lay0 = 0;
                half4 lay1 = 0;
                half4 lay2 = 0;
                half4 lay0_n = 0;
                half4 lay1_n = 0;
                half4 lay2_n = 0;
                fixed3 normal0 = 0;
                fixed3 normal1 = 0;
                fixed3 normal2 = 0;

                // UNITY_BRANCH
                // if (blendmask.r > 0)
                {
                    lay0 = tex2D(_Splat0, uv.zw);
                    lay0_n = tex2D(_BumpSplat0, uv.zw);
                    normal0 = UnpackNormalYoukia(lay0_n, _NormalStrength_0);
                }

                // UNITY_BRANCH
                // if (blendmask.g > 0)
                {
                    lay1 = tex2D(_Splat1, uv1.xy);
                    lay1_n = tex2D(_BumpSplat1, uv1.xy);
                    normal1 = UnpackNormalYoukia(lay1_n, _NormalStrength_1);
                }

                // UNITY_BRANCH
                // if (blendmask.b > 0)
                {
                    lay2 = tex2D(_Splat2, uv1.zw);
                    lay2_n = tex2D(_BumpSplat2, uv1.zw);
                    normal2 = UnpackNormalYoukia(lay2_n, _NormalStrength_2);
                }

                // albedo
                half lerpValue = saturate(blendmask0.b * 2.0 - 1.0);

                blend = lay0 * lerp(_Color0, 1, blendmask0.r);
                blend = lerp(blend, _Color1, blendmask1.r);
                blend = lerp(blend, _Color2, blendmask1.g);
                blend = lerp(blend, lay1, blendmask1.b);

                half4 blendTmp = lay2 * lerp(_Color3, 1, blendmask0.r);
                blendTmp = lerp(blendTmp, _Color4, blendmask1.r);
                blendTmp = lerp(blendTmp, _Color5, blendmask1.g);
                blendTmp = lerp(blendTmp, _Color6, blendmask1.b);

                blend = lerp(blend, blendTmp, lerpValue);

                // normal
                normal = lerp(lerp(normal0, normal1, blendmask1.b), normal2, lerpValue);
                blend_n = lerp(lerp(lay0_n, lay1_n, blendmask1.b), lay2_n, lerpValue);
            }

        ENDCG
        
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

           ZWrite On
			ZTest LEqual
			Cull Back

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _SUB_LIGHT_S
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    half4 uv : TEXCOORD0;
                    half4 uv1 : TEXCOORD1;
                    float4 worldPos : TEXCOORD2;
                    half3 TtoW0 : TEXCOORD3;  
                    half3 TtoW1 : TEXCOORD4;  
                    half3 TtoW2 : TEXCOORD5;
                    half3 sh : TEXCOORD6;

                    half3 giColor : TEXCOORD7;

                    #ifdef _SKYENABLE
                        half3 inscatter : TEXCOORD9;
                        half3 extinction : TEXCOORD10;
                    #endif

                    #ifdef _UNITY_RENDER
                        UNITY_LIGHTING_COORDS(11, 12)
                    #else
                        fixed4 screenPos : TEXCOORD11;
                    #endif

                    #ifdef _HEIGHTFOG
                        half4 heightfog : TEXCOORD13;
                    #endif

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _Control_0);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _Splat0);
                    o.uv1.xy = TRANSFORM_TEX(v.texcoord, _Splat1);
                    o.uv1.zw = TRANSFORM_TEX(v.texcoord, _Splat2);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                    #ifdef _UNITY_RENDER
                        UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    #else
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif

                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    o.TtoW0 = half3(worldTangent.x, worldBinormal.x, worldNormal.x);
                    o.TtoW1 = half3(worldTangent.y, worldBinormal.y, worldNormal.y);
                    o.TtoW2 = half3(worldTangent.z, worldBinormal.z, worldNormal.z);

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    #endif
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);
                    
                    // height fog
                    #ifdef _HEIGHTFOG
                        half fog = 0;
                        o.heightfog.rgb = YoukiaHeightFog(o.worldPos, 0, fog);
                        o.heightfog.a = fog;
                    #endif

                    // atmosphere
                    #ifdef _SKYENABLE
                        half3 extinction = 0;
                        // half3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
                        o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, extinction);
                        o.extinction = extinction;
                    #endif

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    return OBSOLETECOLOR;

                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                    half3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));

                    fixed4 blend = 0;
                    fixed4 blend_n = 0;
                    fixed3 normal = 0;
                    half4 n = tex2D(_BumpMetaMap, i.uv.xy);
                    half3 bump = UnpackNormalYoukia(n, _NormalStrength);
                    SampleTexs(i.uv, i.uv1, blend, blend_n, normal);

                    fixed4 col;
                    col.rgb = blend.rgb;
                    col.a = 1;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    // Metallic R: Metallic, G: Roughness
                    half metallic = MetallicCalc(blend_n.b) * n.b;
                    half smoothness = SmoothnessCalc(blend_n.a) * n.a;

                    // normal
                    half3 worldNormal = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
                    i.TtoW0.z = worldNormal.x;
                    i.TtoW1.z = worldNormal.y;
                    i.TtoW2.z = worldNormal.z;
                    normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));

                    // normal = worldNormal;
                    
                    // light
                    #ifdef _UNITY_RENDER
                        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                        fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                    #else
                        half atten = 0;
                        fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten);
                    #endif

                    // ao
                    half3 ao = AOCalc(blend.a);
                    ao = lerp(1, lerp(_AOColor.rgb, 1, blend.a), _AO);

                    albedo *= ao;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.diffuse *= ao;
                    gi.indirect.specular *= ao;

                    half oneMinusReflectivity;
                    half3 specColor;
                    col.rgb = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

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

                    col.a = alpha;

                    // height fog
                    #ifdef _HEIGHTFOG
                        col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
                    #endif

                    #ifdef _SKYENABLE
                        col.rgb = col.rgb * i.extinction + i.inscatter;
                    #endif
                    
                    return col;
                }
			
			ENDCG
		}

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }

            Blend [_SrcBlend] One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]
            // ColorMask RGB

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd

                #pragma multi_compile __ _UNITY_RENDER
                
                struct v2f 
                {
                    float4 pos : SV_POSITION;
                    half4 uv : TEXCOORD0;
                    half4 uv1 : TEXCOORD1;
                    float4 worldPos : TEXCOORD2;
                    half3 TtoW0 : TEXCOORD3;  
                    half3 TtoW1 : TEXCOORD4;  
                    half3 TtoW2 : TEXCOORD5;
                    half3 sh : TEXCOORD6;
                    UNITY_LIGHTING_COORDS(7, 8)
                    fixed3 viewDir : TEXCOORD9;
                    fixed3 lightDir : TEXCOORD10;

                    half3 giColor : TEXCOORD11;

                    #ifndef _UNITY_RENDER
                        fixed4 screenPos : TEXCOORD12;
                    #endif

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _Control_0);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _Splat0);
                    o.uv1.xy = TRANSFORM_TEX(v.texcoord, _Splat1);
                    o.uv1.zw = TRANSFORM_TEX(v.texcoord, _Splat2);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));
                    
                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    o.TtoW0 = half3(worldTangent.x, worldBinormal.x, worldNormal.x);
                    o.TtoW1 = half3(worldTangent.y, worldBinormal.y, worldNormal.y);
                    o.TtoW2 = half3(worldTangent.z, worldBinormal.z, worldNormal.z);
                    
                     #if defined (UNITY_UV_STARTS_AT_TOP)
                        o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    #endif
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    #ifndef _UNITY_RENDER
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif
                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    // half3 vertColor = half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                    
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;

                    fixed4 blend = 0;
                    fixed4 blend_n = 0;
                    fixed3 normal = 0;
                    half4 n = tex2D(_BumpMetaMap, i.uv.xy);
                    half3 bump = UnpackNormalYoukia(n, _NormalStrength);
                    SampleTexs(i.uv, i.uv1, blend, blend_n, normal);

                    fixed4 col;
                    col.rgb = blend.rgb;
                    col.a = 1;
                    // return col;
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    // Metallic R: Metallic, G: Roughness
                    half metallic = MetallicCalc(blend_n.b) * n.b;
                    half smoothness = SmoothnessCalc(blend_n.a) * n.a;

                    // normal
                    half3 worldNormal = normalize(half3(dot(i.TtoW0.xyz, bump), dot(i.TtoW1.xyz, bump), dot(i.TtoW2.xyz, bump)));
                    i.TtoW0.z = worldNormal.x;
                    i.TtoW1.z = worldNormal.y;
                    i.TtoW2.z = worldNormal.z;
                    normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));

                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;
                    
                    // UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;
                    
                    half oneMinusReflectivity;
                    half3 specColor;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // pbs
                    col = BRDF_Unity_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);
                    col.a = alpha;

                    return col;
                }
            ENDCG
        }

        Pass 
        {
            Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadowTerrain
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }

	}
	FallBack "VertexLit"
    // CustomEditor "YoukiaPBRDetial"
}
