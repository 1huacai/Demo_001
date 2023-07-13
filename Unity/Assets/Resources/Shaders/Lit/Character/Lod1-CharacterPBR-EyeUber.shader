//@@@DynamicShaderInfoStart
//Lod1 眼睛扩展, 可以调节瞳孔、眼白等
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-CharacterPBR-EyeUber"
{
    Properties
    {
        [HDR]_Color("颜色", Color) = (1, 1, 1, 1)
		[NoScaleOffset]_MainTex("瞳孔纹理 RGB: 颜色, A: 瞳孔遮罩", 2D) = "white" {}
        [NoScaleOffset]_MainTexW("眼白纹理 ", 2D) = "white" {}
        [Header(Normal)]
        [NoScaleOffset] _BumpMetaMap("法线纹理, rg: normal, b: ao, a: 自发光", 2D) = "white" {}
        _NormalStrength ("法线强度矫正", Range(0, 2)) = 1
        [Header(Diffuse)]
        [HideInInspector]_DiffThreshold ("Diffuse threshold", Range(0, 0.5)) = 0
        [Header(Eye)]
        _EyeSize ("Eye size", Range(2, 3.5)) = 2
        _PupilSize ("Pupil size", Range(-0.55, 1)) = 0
        _PupilSoft ("Pupil soft", Range(0, 0.3)) = 0.1
        _PupilScale ("Pupil scale", Range(0, 0.4)) = 0
        [Header(Metallic Gloss)]
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		_Roughness ("Roughness", Range(0, 1)) = 0.5
        _Spec ("高光强度", Range(0, 1)) = 1
        [Header(AO)]
        _AO ("AO强度", Range(0, 1)) = 0.5
        _AOColor ("AO颜色", Color) = (0, 0, 0, 1)
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        [Header(Shadow)]
        [HideInInspector]_ShadowStrength ("阴影强度", Range(0, 1)) = 1
        [Header(MatCap)]
        _MatCapMap ("MatCapMap", 2D) = "white" {}
        _MatCapLighting ("MatCapLighting", Range(0, 10)) = 1
        [Header(ToneMapping)]
        [MaterialToggle] _ToneMapping ("Tone mapping", float) = 0
        _AdaptedLum ("white level", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_CharacterPBR_Lod1" }
        LOD 100
        Cull Back

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma target 2.0

            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "YoukiaCharacterLod1.cginc"

            // youkia height fog
            // #pragma multi_compile __ _HEIGHTFOG
            // #pragma multi_compile __ _SKYENABLE
            #pragma multi_compile __ _UNITY_RENDER
            #pragma multi_compile __ _SUB_LIGHT_C

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            sampler2D _MainTexW;
            // eye
            half _EyeSize, _PupilSize, _PupilSoft, _PupilScale;
            // matcap
            sampler2D _MatCapMap;
            half _MatCapLighting;

            // tone mapping
            half _AdaptedLum;
            half _ToneMapping;

            struct appdata 
            {
                half4 vertex : POSITION;
                fixed2 texcoord : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        ENDCG

        Pass
        {
            Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    half4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    half3 TtoW[3] : TEXCOORD2;
                    half3 sh : TEXCOORD5;
                    fixed3 viewDir : TEXCOORD6;
                    fixed3 lightDir : TEXCOORD7;
                    half3 giColor : TEXCOORD8;

                    // YOUKIA_ATMOSPERE_DECLARE(9, 10)
                    YOUKIA_LIGHTING_COORDS(11, 12)
                    // YOUKIA_HEIGHTFOG_DECLARE(13)

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert (appdata v)
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv.xy = _EyeSize * (o.uv.zw - 0.5) + 0.5;
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);

                    half3 worldNormal = 0;
                    T2W(v.normal, v.tangent, o.TtoW, worldNormal);
                    // sh
                    YoukiaVertSH(worldNormal, o.worldPos, o.sh, o.giColor);

                    // shadow
                    YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
                    // height fog
                    // YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
                    // // atmosphere
                    // YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

                    return o;
                }

                FragmentOutput frag(v2f i)
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    _DiffThreshold = 0;

                    float3 worldPos = i.worldPos;
                    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    half3 wsNormal = fixed3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z);
                    half3 wsTangent = fixed3(i.TtoW[0].x, i.TtoW[1].x, i.TtoW[2].x);
                    half3 wsBitangent = fixed3(i.TtoW[0].y, i.TtoW[1].y, i.TtoW[2].y);
                    float3x3 w2t = float3x3(wsTangent, wsBitangent, wsNormal);
                    half3 tViewDir = mul(w2t, viewDir);

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, i.uv);
                    fixed4 cw = tex2D(_MainTexW, i.uv.zw);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, i.uv.xy);
                    
                    // normal
                    half3 normal = WorldNormal(n, i.TtoW);
                    
                    // 过渡
                    half eyeLerp = saturate(c.a / _PupilSoft);
                    normal = lerp(wsNormal, normal, eyeLerp);

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        i.sh += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                    #endif

                    // eye 
                    // 参考美术示例效果
                    // 存在一些trick的值
                    half2 offset = (0.27 - 1) * tViewDir.xy * (_PupilScale * c.a) + i.uv.xy;
                    half2 offsetn = normalize((offset - 0.5) * 0.5);
                    fixed2 eyeUV = lerp(offset, 0.5 + offsetn, (0.8 / _EyeSize * _PupilSize) * (1.0 - 2.0 * _EyeSize * length(i.uv.zw - 0.5)));
                    half4 eyeColor = lerp(cw, tex2D(_MainTex, eyeUV) * _Color, eyeLerp);
                    half4 matcap = tex2D(_MatCapMap, (mul(UNITY_MATRIX_V, half4(normal, 0.0)).xyz * 0.5 + 0.5).xy) * _MatCapLighting * eyeLerp;
                    eyeColor += matcap;

                    fixed4 col = 1;
                    col.rgb = saturate(eyeColor.rgb);
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    half metallic = _Metallic;
                    half smoothness = _Roughness * saturate(eyeLerp + 1.0);

                    // ao
                    half3 ao = pow(tex2D(_BumpMetaMap, i.uv.zw).b, _AOCorrect);
                    ao = lerp(1, lerp(_AOColor.rgb, 1, ao), _AO);

                    // shadow
                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);

                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    CharacterGIScale(gi, normal, lightDir, colShadow, ao);
                    
                    half oneMinusReflectivity;
                    half3 specColor;
                    albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_Eye_PBS(albedo, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow);

                    // sub light
                    #ifdef _SUB_LIGHT_C
                        UnityGI giSub = GetUnityGI_sub(albedo, normalize(_gVSLFwdVec_C), _gVSLColor_C.rgb);
                        col += BRDF_Eye_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, giSub.light, giSub.indirect, 1);
                    #endif
                    
                    // tone mapping
                    col.rgb = lerp(col.rgb, ACESToneMapping(col.rgb * _AdaptedLum), saturate(ceil(_ToneMapping)));

                    col.a = alpha;

                    // // height fog
                    // YOUKIA_HEIGHTFOG(col, i)
                    // // atmosphere
                    // YOUKIA_ATMOSPHERE(col, i)

                    return OutPutCharacterLod1(col);
                }
            ENDCG
        }

        Pass
        {
            Tags { "LightMode"="ForwardAdd" }
            Blend One One
			ZWrite Off

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd
                
                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    half4 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;

                    #ifndef _UNITY_RENDER
                        fixed4 screenPos : TEXCOORD2;
                    #endif

                    half3 TtoW[3] : TEXCOORD3;  
                    half3 sh : TEXCOORD6;

                    UNITY_LIGHTING_COORDS(7, 8)

                    fixed3 viewDir : TEXCOORD9;
                    fixed3 lightDir : TEXCOORD10;

                    half3 giColor : TEXCOORD11;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                    o.uv.zw = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.uv.xy = _EyeSize * (o.uv.zw - 0.5) + 0.5;
                    WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                    
                    half3 worldNormal = 0;
                    T2W(v.normal, v.tangent, o.TtoW, worldNormal);
                    
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target 
                {
                    float3 worldPos = i.worldPos;
                    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;
                    half3 wsNormal = fixed3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z);
                    half3 wsTangent = fixed3(i.TtoW[0].x, i.TtoW[1].x, i.TtoW[2].x);
                    half3 wsBitangent = fixed3(i.TtoW[0].y, i.TtoW[1].y, i.TtoW[2].y);
                    float3x3 w2t = float3x3(wsTangent, wsBitangent, wsNormal);
                    half3 tViewDir = mul(w2t, viewDir);

                    // tex
                    // rgb：color, a: alpha
                    fixed4 c = tex2D(_MainTex, i.uv);
                    fixed4 cw = tex2D(_MainTexW, i.uv.zw);
                    // rg: normal, b: ao, a: 自发光
                    fixed4 n = tex2D(_BumpMetaMap, i.uv.zw);
        
                    // normal
                    half3 normal = WorldNormal(n, i.TtoW);

                    // eye 
                    // 参考美术示例效果
                    // 存在一些trick的值
                    half eyeLerp = saturate(c.a / _PupilSoft);
                    half2 offset = (0.27 - 1) * tViewDir.xy * (_PupilScale * c.a) + i.uv.xy;
                    half2 offsetn = normalize((offset - 0.5) * 0.5);
                    fixed2 eyeUV = lerp(offset, 0.5 + offsetn, (0.8 / _EyeSize * _PupilSize) * (1.0 - 2.0 * _EyeSize * length(i.uv.zw - 0.5)));
                    half4 eyeColor = lerp(cw, tex2D(_MainTex, eyeUV) * _Color, eyeLerp);
                    half4 matcap = tex2D(_MatCapMap, (mul(UNITY_MATRIX_V, half4(normal, 0.0)).xyz * 0.5 + 0.5).xy) * _MatCapLighting * eyeLerp;
                    eyeColor += matcap;

                    fixed4 col = 1;
                    col.rgb = saturate(eyeColor.rgb);
                    half alpha = col.a;
                    fixed3 albedo = col.rgb;

                    half metallic = _Metallic;
                    half smoothness = _Roughness * saturate(eyeLerp + 1.0);
                    
                    // light
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = atten;
                    UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                    gi.indirect.specular *= colShadow;

                    half oneMinusReflectivity;
                    half3 specColor;
                    col.rgb = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                    // brdf pre
                    BRDFPreData brdfPreData = (BRDFPreData)0;
                    PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                    col = BRDF_Eye_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, gi.light, gi.indirect, colShadow);

                    // tone mapping
                    col.rgb = lerp(col.rgb, ACESToneMapping(col.rgb * _AdaptedLum), saturate(ceil(_ToneMapping)));
                        
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
}
