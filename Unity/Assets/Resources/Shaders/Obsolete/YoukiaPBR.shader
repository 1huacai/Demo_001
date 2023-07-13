Shader "YoukiaEngine/Obsolete/YoukiaPBR" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMap("法线纹理", 2D) = "white" {}
        [Header(Metallic Gloss)]
        [NoScaleOffset] _MetallicMap ("金属度光滑度贴图 (R: Metallic, G: Roughness, B: AO)", 2D) = "balck" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        [Header(Emission)]
        [Toggle(_EMISSION)]_Toggle_EMISSION_ON("自发光", Float) = 0
        [HDR]_ColorEmission("自发光颜色", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_EmissionTex("自发光贴图", 2D) = "black" {}
        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0
        
        // [Space(30)]
        // [Header(Bloom)]
        // _BloomThreshold ("Bloom 阈值(作用于alpha mask)", Range(0, 1)) = 1
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
		Tags { "RenderType"="Opaque" "ShadowType" = "ST_Opaque" }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 2.0

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "YoukiaObsolete.cginc"

            #pragma shader_feature _EMISSION
            // #pragma shader_feature _SHADOW_BLUR
            #pragma multi_compile __ _UNITY_RENDER
            
            // sss
            half4 _SSSColor;
            half _SSSDistortion, _SSSPower, _SSSScale, _SSSStrength;

            // emission
            sampler2D _EmissionTex;
            half4 _ColorEmission;

            // bloom
            // half _BloomThreshold;
            // half _Threshold_Scene;

            struct appdata_t 
            {
                half4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                half3 color : COLOR;
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
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase
                
                // unity light map
                // #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
                // youkia height fog
                #pragma multi_compile __ _HEIGHTFOG

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    float4 worldPos : TEXCOORD1;

                    #ifdef _UNITY_RENDER
                        UNITY_LIGHTING_COORDS(7, 8)
                    #else
                        fixed4 screenPos : TEXCOORD2;
                    #endif

                    // UNITY_FOG_COORDS(3)
                    half4 heightfog : TEXCOORD3;
                    half4 TtoW0 : TEXCOORD4;  
                    half4 TtoW1 : TEXCOORD5;  
                    half4 TtoW2 : TEXCOORD6;
                    half3 sh : TEXCOORD7;

                    fixed2 uv : TEXCOORD0;

                    half3 giColor : TEXCOORD9;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                    #ifdef _UNITY_RENDER
                        UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    #else
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif

                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    o.TtoW0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, v.color.r);
                    o.TtoW1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, v.color.g);
                    o.TtoW2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, v.color.b);
                    
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    // UNITY_TRANSFER_FOG(o,o.pos);
                    // height fog
                    #ifdef _HEIGHTFOG
                        half fog = 0;
                        o.heightfog.rgb = YoukiaHeightFog(o.worldPos, 0, fog);
                        o.heightfog.a = fog;
                    #endif
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    return OBSOLETECOLOR;

                    half3 vertColor = half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                    
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                    half3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                    
                    fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                    half alpha = col.a;
                    fixed3 abledo = col.rgb;
                    clip(col.a - _Cutoff);

                    // Metallic R: Metallic, G: Roughness
                    half4 meta = tex2D(_MetallicMap, i.uv);
                    half metallic = meta.r * _Metallic;
                    half smoothness = ((1 - meta.g) * _Roughness);

                    // normal
                    fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
                    normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));

                    // light
                    #ifdef _UNITY_RENDER
                        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                        fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                    #else
                        half atten = 0;
                        fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten);
                    #endif

                    // ao
                    half3 ao = lerp(1, lerp(_AOColor.rgb, 1, meta.b), _AO);
                    colShadow *= ao;
                    UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);

                    half oneMinusReflectivity;
                    half3 specColor;
                    col.rgb = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // pbs
                    col = BRDF_Unity_PBS(abledo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                    #if _EMISSION
                        col.rgb += tex2D(_EmissionTex, i.uv) * _ColorEmission;
                    #endif

                    col.a = alpha;
                    // bloom alpha mask
                    // col.a *= saturate(1 - min(_Threshold_Scene, _BloomThreshold));

                    // height fog
                    #ifdef _HEIGHTFOG
                        col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
                    #endif
                    // UNITY_APPLY_FOG(i.fogCoord, col);
                    return col;
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
                
                struct v2f 
                {
                    half4 vertex : SV_POSITION;
                    float4 worldPos : TEXCOORD1;

                    // UNITY_FOG_COORDS(3)
                    half4 TtoW0 : TEXCOORD4;  
                    half4 TtoW1 : TEXCOORD5;  
                    half4 TtoW2 : TEXCOORD6;
                    half3 sh : TEXCOORD7;

                    fixed2 uv : TEXCOORD0;

                    half3 giColor : TEXCOORD8;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    
                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    o.TtoW0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, v.color.r);
                    o.TtoW1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, v.color.g);
                    o.TtoW2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, v.color.b);
                    
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    // UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    half3 vertColor = half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                    
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                    half3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                    
                    fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                    half alpha = col.a;
                    fixed3 abledo = col.rgb;
                    clip(col.a - _Cutoff);

                    // Metallic R: Metallic, G: Roughness
                    half4 meta = tex2D(_MetallicMap, i.uv);
                    half metallic = meta.r * _Metallic;
                    half smoothness = ((1 - meta.g) * _Roughness);

                    // normal
                    fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.uv));
                    normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
                    
                    UnityGI gi = GetUnityGI(abledo, normal, worldPos, lightDir, viewDir, 1, metallic, smoothness, i.sh, i.giColor);

                    half oneMinusReflectivity;
                    half3 specColor;
                    col.rgb = DiffuseAndSpecularFromMetallic(abledo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    // pbs
                    col = BRDF_Unity_PBS(abledo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, 1, 1);

                    col.a = alpha;
                    // bloom alpha mask
                    // col.a *= saturate(1 - min(_Threshold_Scene, _BloomThreshold));

                    // height fog
                    // col.rgb = YoukiaHeightFog(i.worldPos, col.rgb);
                    // UNITY_APPLY_FOG(i.fogCoord, col);
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
            #pragma fragment fragShadow
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }

	}
	FallBack "VertexLit"
    // CustomEditor "YoukiaCharacterPBRInspector"
}
