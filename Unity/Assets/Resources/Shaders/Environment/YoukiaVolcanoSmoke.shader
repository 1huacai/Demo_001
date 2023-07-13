//@@@DynamicShaderInfoStart
//火山Shader。uv1->mask，uv2->流动方向。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Environment/YoukiaVolcanoSmoke"
{
    Properties 
    {
        [HDR]_Color("颜色1", Color) = (1, 1, 1, 1)
        _ColorNoise("Color Noise", Range(1, 20)) = 1
		_MainTex("纹理1 (uv2)", 2D) = "white" {}
        _MainTex2("纹理2 (uv2)", 2D) = "white" {}
        [Header(Speed)]
        _SmokeSpeed1("Speed 1", Range(0, 10)) = 0
        _SmokeSpeed2("Speed 2", Range(0, 10)) = 0

        [Header(Normal Metallic)]
        [NoScaleOffset] _BumpMetaMap("法线纹理 (rg: normal) (uv2)", 2D) = "white" {}
        [HideInInspector]_NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 0.5
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse threshold", Range(0, 0.5)) = 0

        [Header(Smoke)]
        _SmokeColor ("烟雾颜色", Color) = (0.5, 0.5, 0.5, 1)
        _SmokeTex("烟雾纹理 (uv2)", 2D) = "white" {}
        _Smoke ("烟雾浓度", Range(0, 1)) = 0.5
        _SmokeStrength ("烟雾范围", Range(0, 5)) = 0
        _SmokeScale ("烟雾边缘", Range(1, 20)) = 1
        _SmokeSpeedU ("烟雾速度 U", Range(-5, 5)) = 0
        _SmokeSpeedV ("烟雾速度 V", Range(0, 5)) = 0

        [Header(VertexAnim)]
        _VertexNoiseStrength ("顶点噪声强度", Range(0, 10)) = 0

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
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_PBRDetail" "Reflection" = "RenderReflectionOpaque" }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 2.0

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"
            #include "../Library/YoukiaEaryZ.cginc"
            #include "../Library/YoukiaEffect.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            half _ColorNoise;

            sampler2D _MainTex2;
            half4 _MainTex2_ST;

            // normal
            // half _NormalStrength;

            sampler2D _SmokeTex;
            half4 _SmokeTex_ST;
            half4 _SmokeColor;
            half _Smoke;
            half _SmokeStrength, _SmokeScale;
            half _SmokeSpeedU, _SmokeSpeedV;

            half _SmokeSpeed1, _SmokeSpeed2;
            
            // vertex noise
            half _VertexNoiseStrength;

            struct appdata_t 
            {
                float4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                fixed2 texcoord2 : TEXCOORD1;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                // half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 顶点noise
            float3 VertNoise(half2 uv, half3 normal)
            {
                float2 vertUV = uv - _Time.x * half2(0, _SmokeSpeed1);
                half vertNoise = tex2Dlod(_MainTex2, half4(vertUV, 0, 0));
                vertNoise *= _VertexNoiseStrength;
                float3 noise = normal * vertNoise;
                return noise;
            }

            void Smoke(half2 uv1, half2 uv2, half2 uvSmoke1, half2 uvSmoke2, 
                in out half3 albedo, in out half3 normal, in out half metallic, in out half smoothness, in out half alpha, in out half3 ao)
            {
                half2 smokeUV1 = uvSmoke1 - half2(0, _SmokeSpeed1) * _Time.x;
                half2 smokeUV2 = uvSmoke2 - half2(0, _SmokeSpeed2) * _Time.x;
                // tex
                // rgb: color
                fixed4 c = tex2D(_MainTex, smokeUV1);
                half4 smoke = tex2D(_MainTex2, smokeUV2);
                // rg: normal
                fixed4 n = tex2D(_BumpMetaMap, smokeUV1);

                // smoke
                // 1 -> color, 0 -> smoke
                half smokeRange = saturate((1 - abs(uv2.x - 0.5) * 2) * (1 - uv2.y));
                smokeRange = saturate(pow(saturate(smokeRange * _SmokeStrength), _SmokeScale));

                fixed4 col;
                half noise = (c.r + smoke.r) * 0.5f;
                noise = pow(1 - noise, lerp(_ColorNoise, 1, smokeRange));
                noise = saturate(noise);

                smokeUV2 = uv1 * _SmokeTex_ST.xy - half2(_SmokeSpeedU, _SmokeSpeedV) * _Time.x;
                smoke = tex2D(_SmokeTex, smokeUV2);
                smoke = lerp(smoke, 1, saturate(smokeRange * _Smoke));

                half3 volcanoColor = lerp(_SmokeColor.rgb, _Color.rgb, smokeRange * smoke * noise);
                
                alpha = _Color.a;
                albedo = volcanoColor;
                metallic = _Metallic;
                smoothness = _Roughness;

                // normal
                _NormalStrength = 1;
                normal = UnpackNormalYoukia(n, _NormalStrength);

                // ao
                ao = 1;
            }

        ENDCG
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
            // ZWrite Off
            // ZTest Equal
			Cull[_cull]

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
                    float4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 TtoW[3] : TEXCOORD2;
                    half3 sh : TEXCOORD5;
                    fixed3 viewDir : TEXCOORD6;
                    fixed3 lightDir : TEXCOORD7;
                    half3 giColor : TEXCOORD8;

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

                    half4 uvSmoke : TEXCOORD14;
                    
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.uv.xy = v.texcoord;
                    o.uv.zw = v.texcoord2;
                    o.uvSmoke.xy = TRANSFORM_TEX(v.texcoord2, _MainTex);
                    o.uvSmoke.zw = TRANSFORM_TEX(v.texcoord2, _MainTex2);

                    // vertex noise
                    v.vertex.xyz += VertNoise(o.uvSmoke.xy, v.normal);
                    o.pos = UnityObjectToClipPos(v.vertex);

                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));

                    #ifdef _UNITY_RENDER
                        UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    #else
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif

                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    o.TtoW[0] = half3(worldTangent.x, worldBinormal.x, worldNormal.x);
                    o.TtoW[1] = half3(worldTangent.y, worldBinormal.y, worldNormal.y);
                    o.TtoW[2] = half3(worldTangent.z, worldBinormal.z, worldNormal.z);

                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);
                    
                    // height fog
                    #ifdef _HEIGHTFOG
                        o.heightfog.rgb = YoukiaHeightFog(o.worldPos, 0, o.heightfog.a);
                    #endif

                    // atmosphere
                    #ifdef _SKYENABLE
                        o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, o.extinction);
                    #endif

                    return o;
                }

                FragmentOutput frag(v2f i)
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;

                    half4 col = 1;
                    half3 albedo = 0;
                    half3 normal = 0;
                    half alpha = 1;
                    half metallic = 0;
                    half smoothness = 0;
                    half3 ao = 1;

                    Smoke(i.uv.xy, i.uv.zw, i.uvSmoke.xy, i.uvSmoke.zw, albedo, normal, metallic, smoothness, alpha, ao);
                    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
                    // return half4(albedo, 1);
                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh = YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // light
                    #ifdef _UNITY_RENDER
                        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                        fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                    #else
                        half atten = 0;
                        fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten);
                    #endif

                    // colShadow *= ao;
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
                    col.a = alpha;

                    // height fog
                    #ifdef _HEIGHTFOG
                        col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
                    #endif

                    #ifdef _SKYENABLE
                        col.rgb = col.rgb * i.extinction + i.inscatter;
                    #endif

                    // lum
                    col.rgb = SceneLumChange(col.rgb);
                    
                    return OutPutDefault(col);
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
                
                struct v2f 
                {
                    float4 pos : SV_POSITION;
                    fixed4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 TtoW[3] : TEXCOORD2;
                    half3 sh : TEXCOORD5;
                    UNITY_LIGHTING_COORDS(6, 7)
                    fixed3 viewDir : TEXCOORD8;
                    fixed3 lightDir : TEXCOORD9;

                    half3 giColor : TEXCOORD10;

                    half4 uvSmoke : TEXCOORD11;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    // vertex noise
                    v.vertex.xyz += VertNoise(v.texcoord2, v.normal);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = v.texcoord;
                    o.uv.zw = v.texcoord2;
                    o.uvSmoke.xy = TRANSFORM_TEX(v.texcoord2, _MainTex);
                    o.uvSmoke.zw = TRANSFORM_TEX(v.texcoord2, _MainTex2);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));
                    
                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                    o.TtoW[0] = half3(worldTangent.x, worldBinormal.x, worldNormal.x);
                    o.TtoW[1] = half3(worldTangent.y, worldBinormal.y, worldNormal.y);
                    o.TtoW[2] = half3(worldTangent.z, worldBinormal.z, worldNormal.z);
                    
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;

                    half4 col = 1;
                    half3 albedo = 0;
                    half3 normal = 0;
                    half alpha = 1;
                    half metallic = 0;
                    half smoothness = 0;
                    half3 ao = 1;

                    Smoke(i.uv.xy, i.uv.zw, i.uvSmoke.xy, i.uvSmoke.zw, albedo, normal, metallic, smoothness, alpha, ao);
                    normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;
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
            #pragma vertex vertShadowSmoke
            #pragma fragment fragShadowDetail
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            void vertShadowSmoke (appdata_shadow v, 
                out float4 opos : SV_POSITION,
                out v2fShadow o)
            {
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SETUP_INSTANCE_ID(v);
                // vertex noise
                v.vertex.xyz += VertNoise(v.uv1, v.normal);
                TRANSFER_SHADOW_CASTER_NOPOS(o,opos)

                o.tex.xy = v.uv0;

                #if _USE_DISSOLVE
                    o.tex.zw = TRANSFORM_TEX(v.uv0, _DissolveTex);
                #endif
            }

            ENDCG
        }

	}
	FallBack "VertexLit"
    // CustomEditor "YoukiaPBRDetial"
}
