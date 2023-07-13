// ref: https://zhuanlan.zhihu.com/p/57897827
// ref: https://mp.weixin.qq.com/s/aIWMEO5Qa2gNn2yCmnHbOg
//@@@DynamicShaderInfoStart
//Lod1 绒毛
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-CharacterPBR-Fur" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_MainTex("纹理", 2D) = "white" {}
        [Gamma]_Metallic ("Metallic", Range(0, 1)) = 0.0
		[Gamma]_Roughness ("Roughness", Range(0, 1)) = 1
        
        [Header(AO)]
        _AO ("AO 强度", Range(0, 1)) = 0.5
        _AOCorrect ("AO 矫正", Range(0, 4)) = 1
        _AOColor ("AO 颜色", Color) = (0, 0, 0, 1)

        [Header(Fabric)]
        [HDR]_FabricScatterColor ("Fur 边缘颜色", Color) = (0.5, 0.5, 0.5, 1)
        _FabricScatterScale ("Fur 边缘范围", Range(1, 6)) = 4
        [Space(5)]
        _LightTransScale ("Fur 透光范围", Range(0, 1)) = 0.32

        [Header(Aniso)]
        _Shift ("各向异性高光偏移", Range(-2, 2)) = 1
        _Exp ("各项异性高光范围", Range(0, 360)) = 200
        _Strength ("各项异性高光强度", Range(0, 100)) = 0

        [Header(Fur)]
        [NoScaleOffset]_LayerTex("Fur 纹理", 2D) = "white" {}
        _LayerTexScale("Fur 纹理缩放", Range(0, 100)) = 2
        [Space(5)]
        _FurLength("Fur 长度", Range(.0002, 0.1)) = .01
		_CutoffEnd("Fur 末端Alpha值", Range(0, 1)) = 0.8
		[HideInInspector]_EdgeFade("Edge fade", Range(0, 1)) = 0.4
		
        [Header(Flow)]
        [NoScaleOffset]_FlowMap("Flow Map", 2D) = "gray" {}
        _FlowMapStrength("Flow Map 强度", Range(0, 2)) = 0
        _UVoffsetU ("UV偏移 U", Range(-0.1, 0.1)) = 0
        _UVoffsetV ("UV偏移 V", Range(-0.1, 0.1)) = 0
        _GravityStrength("重力影响强度", Range(0, 4)) = 0.05
        _ColorFlowMapStrength ("颜色纹理偏移系数", Range(0, 1)) = 0
       
        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0

        [Header(Others)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		// [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		// [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        // [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		// [Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
	}
	SubShader 
    {
		Tags { "Queue"="AlphaTest+10" "RenderType"="Opaque" "ShadowType" = "ST_CharacterPBR_Lod1" "Reflection" = "RenderReflectionOpaque" }

        CGINCLUDE
            #pragma multi_compile_instancing
            #pragma target 3.0

            #include "../../Library/YoukiaLightPBR.cginc"
            #include "../../Library/YoukiaEnvironment.cginc"
            #include "../../Library/Atmosphere.cginc"
            #include "../../Library/YoukiaMrt.cginc"
            #include "../../Library/YoukiaEffect.cginc"
            #include "YoukiaCharacterLod1.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

            #pragma multi_compile __ _UNITY_RENDER

            const static half3 GRAVITY = half3(0, -9.8, 0);

            half _UVoffsetU, _UVoffsetV;

            sampler2D _LayerTex;
            half4 _LayerTex_ST;
            half _LayerTexScale;

            half _LightTransScale;

            // half4 _SpecColor;
            half _SpecStrength;
            half _Shift;
            half _Exp;
            half _Strength;

            half _FurLength;
            half _CutoffEnd;
            half _EdgeFade;
            half _GravityStrength;
            half _ColorFlowMapStrength;

            #define FURSTEP0 0.0
            #define FURSTEP1 0.2
            #define FURSTEP2 0.4
            #define FURSTEP3 0.6
            #define FURSTEP4 0.65
            #define FURSTEP5 0.7
            #define FURSTEP6 0.75
            #define FURSTEP7 0.8
            #define FURSTEP8 0.85
            #define FURSTEP9 0.9
            #define FURSTEP10 0.95
            #define FURSTEP11 1.0

            struct v2fbase
            {
                half4 pos : SV_POSITION;
                half4 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                half4 worldNormal : TEXCOORD2;
                half4 vertLit : TEXCOORD3;
                half3 sh : TEXCOORD5;
                fixed4 viewDir : TEXCOORD6;
                fixed3 lightDir : TEXCOORD7;
                half3 giColor : TEXCOORD8;

                // YOUKIA_ATMOSPERE_DECLARE(9, 10)
                YOUKIA_LIGHTING_COORDS(11, 12)
                // YOUKIA_HEIGHTFOG_DECLARE(13)

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2fadd
            {
                half4 pos : SV_POSITION;
                half4 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                half3 worldNormal : TEXCOORD2;
                half2 vertLit : TEXCOORD3;

                half3 sh : TEXCOORD5;

                UNITY_LIGHTING_COORDS(6, 7)

                fixed3 viewDir : TEXCOORD8;
                fixed3 lightDir : TEXCOORD9;

                half3 giColor : TEXCOORD10;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            half3 GravityDir(half3 vertex, half3 normal, half furOffset, half2 uv)
            {
                furOffset *= furOffset;
                half3 gravity = mul(unity_WorldToObject, GRAVITY);
                half3 direction = lerp(normal, gravity * _GravityStrength + normal, furOffset);
	            vertex += direction * _FurLength * furOffset;

                return vertex;
            }

            half3 FabricScatter(half nl, half nv)
            {
                return _FabricScatterColor * (nl * 0.5 + 0.5) * FabricScatterFresnelLerp(nv, _FabricScatterScale);
            }

            half3 FurScatter(half nl, half nv, half furOffset)
            {
                half fresnel = 1 - saturate(nv);
                fresnel = saturate(pow(fresnel, _FabricScatterScale));

                half rim = fresnel;
                return rim * _FabricScatterColor;
            }

            half FurAlpha(half furmask, half furOffset, half nv)
            {
                // half alpha = furmask;
                // alpha = step(lerp(0, _CutoffEnd, furOffset * furOffset), alpha);

                // half o = 1 - furOffset * furOffset;
                // o += nv - _EdgeFade;
                // o = saturate(o);
                // o *= alpha;

                half alphaEnd = saturate(1 - _CutoffEnd);
                half o = saturate(furmask * 2 - (furOffset * furOffset + (furOffset * 5)) * alphaEnd);
                o = furOffset > 0 ? o : 1;

                return o;
            }
            
            v2fbase vertBase(appdata_t v, half furOffset = 0) 
            {
                v2fbase o;
                UNITY_INITIALIZE_OUTPUT(v2fbase,o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                v.vertex.xyz = GravityDir(v.vertex.xyz, v.normal, furOffset, v.texcoord);

                o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                half2 uvOffset = half2(_UVoffsetU, _UVoffsetV) * furOffset;
                o.uv.xy = v.texcoord;
                o.uv.zw = uvOffset;

                WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                half3 wTangent;
                half3 wBinormal;
                T2W(v.normal, v.tangent, o.worldNormal.xyz, wTangent, wBinormal);
                // o.worldNormal.xyz = UnityObjectToWorldNormal(v.normal);

                // nl + 逆光
                o.worldNormal.w = saturate(dot(o.worldNormal, o.lightDir) + _LightTransScale * furOffset);
                // sub light
                o.vertLit.w = saturate(dot(o.worldNormal, normalize(_gVSLFwdVec_C)));
                // fabric
                half nv = saturate(dot(o.worldNormal, o.viewDir));
                // o.vertLit.xyz = FabricScatter(o.worldNormal.w, nv);
                o.vertLit.xyz = FurScatter(o.worldNormal.w, nv, furOffset);
                // aniso
                half3 t = TShift(wBinormal, o.worldNormal.xyz, _Shift);
                o.vertLit.xyz += StrandSpecular(t, o.viewDir.xyz, o.lightDir, _Exp) * _Strength;

                o.viewDir.w = nv;
                
                YoukiaVertSH(o.worldNormal, o.worldPos, o.sh, o.giColor);

                // shadow
                YOUKIA_TRANSFER_LIGHTING(o, v.texcoord);
                // height fog
                // YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
                // atmosphere
                // YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)


                return o;
            }

            half4 fragBase(v2fbase i, half furOffset = 0)
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float3 worldPos = i.worldPos;
                fixed3 viewDir = i.viewDir.xyz;
                fixed3 lightDir = i.lightDir;
                fixed3 worldNormal = i.worldNormal;
                half nl = i.worldNormal.w;
                half nlSub = i.vertLit.w;
                half3 fabricScatter = i.vertLit.xyz;
                half nv = i.viewDir.w;

                half2 flowMap = tex2D(_FlowMap, i.uv.xy).rg * 2 - 1;
                flowMap *= furOffset * _FlowMapStrength;
                
                half2 uv = TRANSFORM_TEX(i.uv, _MainTex) + (i.uv.zw + flowMap) * _ColorFlowMapStrength;
                half2 uvFur = (TRANSFORM_TEX(i.uv, _LayerTex) + i.uv.zw) * _LayerTexScale + flowMap;

                // tex
                // rgb：color, a: alpha
                fixed4 c = tex2D(_MainTex, uv);
                // fur
                half3 furtex = tex2D(_LayerTex, uvFur).rgb;
                // half furmask = lerp(furtex.r, furtex.g, saturate(0.5 - furOffset * furOffset) * 2);
                // furmask = lerp(furmask, furtex.b, saturate(furOffset * furOffset - 0.5) * 2);
                
                // furmask = lerp(furtex.r, saturate(furtex.r + furtex.g * furtex.b) * 0.5, furOffset * furOffset * furOffset * furOffset);
                half furmask = max(furtex.r, max(furtex.g, saturate(furtex.r * furtex.b + furtex.g)));
                // half furmask = furtex.r;
                
                fixed4 col = c * _Color;
                half alpha = col.a;
                fixed3 albedo = col.rgb;

                alpha = FurAlpha(furmask, furOffset, nv) * alpha;
                clip(alpha - _Cutoff - 0.001f);

                half metallic = MetallicCalc(1);
                half smoothness = SmoothnessCalc(0);
                // ao
                half occlusion = saturate(pow(furmask, _AOCorrect));
                half3 ao = lerp(1, lerp(_AOColor.rgb, 1, occlusion), _AO);

                // normal
                half3 normal = worldNormal;

                #if defined (UNITY_UV_STARTS_AT_TOP)
                    i.sh += YoukiaGI_IndirectDiffuse(normal, worldPos, i.giColor);
                #endif

                // shadow
                YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);

                UnityGI gi = GetUnityGI(albedo, normal, worldPos, lightDir, viewDir, colShadow, metallic, smoothness, i.sh, i.giColor);
                CharacterGIScale(gi, normal, lightDir, colShadow, ao);
                fabricScatter *= ao;
                gi.indirect.diffuse *= ao;
                gi.indirect.specular *= ao;
                albedo *= ao;

                // pbs
                half oneMinusReflectivity;
                half3 specColor = 0;
                col.rgb = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);
                
                // brdf pre
                BRDFPreData brdfPreData = (BRDFPreData)0;
                PrepareBRDF(smoothness, normal, viewDir, oneMinusReflectivity, brdfPreData);

                col = BRDF_Fur_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, nl, fabricScatter, gi.light, gi.indirect, colShadow, atten);

                // sub light
                #ifdef _SUB_LIGHT_C
                    UnityGI giSub = GetUnityGI_sub(albedo, normalize(_gVSLFwdVec_C), _gVSLColor_C.rgb);
                    col += BRDF_Fur_PBS(col.rgb, specColor, normal, viewDir, brdfPreData, nlSub, fabricScatter, giSub.light, giSub.indirect, 1, 1);
                #endif

                col.a = alpha;

                // height fog
                // YOUKIA_HEIGHTFOG(col, i)
                // // atmosphere
                // YOUKIA_ATMOSPHERE(col, i)
                return col;
            }

            v2fbase vertBaseFurLayer0(appdata_t v) { return vertBase(v, FURSTEP0); }
            v2fbase vertBaseFurLayer1(appdata_t v) { return vertBase(v, FURSTEP1); }
            v2fbase vertBaseFurLayer2(appdata_t v) { return vertBase(v, FURSTEP2); }
            v2fbase vertBaseFurLayer3(appdata_t v) { return vertBase(v, FURSTEP3); }
            v2fbase vertBaseFurLayer4(appdata_t v) { return vertBase(v, FURSTEP4); }
            v2fbase vertBaseFurLayer5(appdata_t v) { return vertBase(v, FURSTEP5); }
            v2fbase vertBaseFurLayer6(appdata_t v) { return vertBase(v, FURSTEP6); }
            v2fbase vertBaseFurLayer7(appdata_t v) { return vertBase(v, FURSTEP7); }
            v2fbase vertBaseFurLayer8(appdata_t v) { return vertBase(v, FURSTEP8); }
            v2fbase vertBaseFurLayer9(appdata_t v) { return vertBase(v, FURSTEP9); }
            v2fbase vertBaseFurLayer10(appdata_t v) { return vertBase(v, FURSTEP10); }
            v2fbase vertBaseFurLayer11(appdata_t v) { return vertBase(v, FURSTEP11); }

            FragmentOutput fragBaseFurLayer0 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP0)); }
            FragmentOutput fragBaseFurLayer1 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP1)); }
            FragmentOutput fragBaseFurLayer2 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP2)); }
            FragmentOutput fragBaseFurLayer3 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP3)); }
            FragmentOutput fragBaseFurLayer4 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP4)); }
            FragmentOutput fragBaseFurLayer5 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP5)); }
            FragmentOutput fragBaseFurLayer6 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP6)); }
            FragmentOutput fragBaseFurLayer7 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP7)); }
            FragmentOutput fragBaseFurLayer8 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP8)); }
            FragmentOutput fragBaseFurLayer9 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP9)); }
            FragmentOutput fragBaseFurLayer10 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP10)); }
            FragmentOutput fragBaseFurLayer11 (v2fbase i) : SV_Target { return OutPutCharacterLod1(fragBase(i, FURSTEP11)); }

            v2fadd vertadd(appdata_t v, half furOffset = 0) 
            {
                v2fadd o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_OUTPUT(v2fadd,o);

                v.vertex.xyz = GravityDir(v.vertex.xyz, v.normal, furOffset, v.texcoord);

                o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.texcoord, _LayerTex) * _LayerTexScale;
                WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir);
                
                o.worldNormal = UnityObjectToWorldNormal(v.normal);  

                // nl
                o.vertLit.x = saturate(dot(o.worldNormal, o.lightDir));
                // nv
                o.vertLit.y = saturate(dot(o.worldNormal, o.viewDir));
                
                o.sh = ShadeSHPerVertex(o.worldNormal, o.sh);
                o.sh += YoukiaGI_IndirectDiffuse(o.worldNormal, o.worldPos, o.giColor);

                UNITY_TRANSFER_LIGHTING(o, v.texcoord);

                return o;
            }

            fixed4 fragadd(v2fadd i, half furOffset = 0)
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 worldPos = i.worldPos;
                fixed3 viewDir = i.viewDir;
                half3 lightDir = i.lightDir;
                half3 fabricScatter = 0;
                half2 uv = i.uv.xy;

                fixed3 worldNormal = i.worldNormal;
                half nl = i.vertLit.x;
                half nv = i.vertLit.y;

                // tex
                // rgb：color, a: alpha
                fixed4 c = tex2D(_MainTex, uv);
                // fur
                half3 furtex = tex2D(_LayerTex, i.uv.zw).rgb;
                half furmask = max(furtex.r, max(furtex.g, saturate(furtex.r * furtex.b + furtex.g)));
                
                fixed4 col = c * _Color;
                half alpha = col.a;
                fixed3 albedo = col.rgb;
                
                // light
                UNITY_LIGHT_ATTENUATION(atten, i, worldPos);

                col = BRDF_Fur_PBS_Add(albedo, nv, nl, _LightColor0.rgb, i.sh, atten);

                // col.a = FurAlpha(furmask, furOffset, nv) * alpha;
                col.a = 1;
                return col;
            }

            v2fadd vertAddFurLayer0(appdata_t v) { return vertadd(v, FURSTEP0); }
            v2fadd vertAddFurLayer1(appdata_t v) { return vertadd(v, FURSTEP1); }
            v2fadd vertAddFurLayer2(appdata_t v) { return vertadd(v, FURSTEP2); }
            v2fadd vertAddFurLayer3(appdata_t v) { return vertadd(v, FURSTEP3); }
            v2fadd vertAddFurLayer4(appdata_t v) { return vertadd(v, FURSTEP4); }
            v2fadd vertAddFurLayer5(appdata_t v) { return vertadd(v, FURSTEP5); }
            v2fadd vertAddFurLayer6(appdata_t v) { return vertadd(v, FURSTEP6); }
            v2fadd vertAddFurLayer7(appdata_t v) { return vertadd(v, FURSTEP7); }
            v2fadd vertAddFurLayer8(appdata_t v) { return vertadd(v, FURSTEP8); }
            v2fadd vertAddFurLayer9(appdata_t v) { return vertadd(v, FURSTEP9); }
            v2fadd vertAddFurLayer10(appdata_t v) { return vertadd(v, FURSTEP10); }
            v2fadd vertAddFurLayer11(appdata_t v) { return vertadd(v, FURSTEP11); }

            half4 fragAddFurLayer0 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP0); }
            half4 fragAddFurLayer1 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP1); }
            half4 fragAddFurLayer2 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP2); }
            half4 fragAddFurLayer3 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP3); }
            half4 fragAddFurLayer4 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP4); }
            half4 fragAddFurLayer5 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP5); }
            half4 fragAddFurLayer6 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP6); }
            half4 fragAddFurLayer7 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP7); }
            half4 fragAddFurLayer8 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP8); }
            half4 fragAddFurLayer9 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP9); }
            half4 fragAddFurLayer10 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP10); }
            half4 fragAddFurLayer11 (v2fadd i) : SV_Target { return fragadd(i, FURSTEP11); }

        ENDCG
		
        // 0
        Pass 
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer0
                #pragma fragment fragBaseFurLayer0
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer0
                #pragma fragment fragAddFurLayer0
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 1
		Pass 
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer1
                #pragma fragment fragBaseFurLayer1
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer1
                #pragma fragment fragAddFurLayer1
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 2
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer2
                #pragma fragment fragBaseFurLayer2
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer2
                #pragma fragment fragAddFurLayer2
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 3
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer3
                #pragma fragment fragBaseFurLayer3
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer3
                #pragma fragment fragAddFurLayer3
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 4
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer4
                #pragma fragment fragBaseFurLayer4
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer4
                #pragma fragment fragAddFurLayer4
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 5
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer5
                #pragma fragment fragBaseFurLayer5
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer5
                #pragma fragment fragAddFurLayer5
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 6
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer6
                #pragma fragment fragBaseFurLayer6
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer6
                #pragma fragment fragAddFurLayer6
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 7
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer7
                #pragma fragment fragBaseFurLayer7
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer7
                #pragma fragment fragAddFurLayer7
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 8
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer8
                #pragma fragment fragBaseFurLayer8
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer8
                #pragma fragment fragAddFurLayer8
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 9
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer9
                #pragma fragment fragBaseFurLayer9
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer9
                #pragma fragment fragAddFurLayer9
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 10
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer10
                #pragma fragment fragBaseFurLayer10
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer10
                #pragma fragment fragAddFurLayer10
                #pragma multi_compile_fwdadd

            ENDCG
        }
        // 11
        Pass
        {
			Tags { "LightMode"="ForwardBase" "ShadowType" = "ST_CharacterPBR_Lod1" }

            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
                #pragma vertex vertBaseFurLayer11
                #pragma fragment fragBaseFurLayer11
                #pragma multi_compile_fwdbase

                // youkia height fog
                // #pragma multi_compile __ _HEIGHTFOG
                // #pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_C
			
			ENDCG
		}
        Pass
        {
            Tags { "LightMode"="ForwardAdd" }

            Blend SrcAlpha One
			ZWrite Off
			ZTest LEqual
			Cull[_cull]

            CGPROGRAM
                #pragma vertex vertAddFurLayer11
                #pragma fragment fragAddFurLayer11
                #pragma multi_compile_fwdadd

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
                #pragma multi_compile_instancing
                #pragma multi_compile_shadowcaster
                #include "UnityCG.cginc"
                #include "Lighting.cginc"

                #pragma fragmentoption ARB_precision_hint_fastest

            ENDCG
        }

	}
	FallBack "VertexLit"
    // CustomEditor "YoukiaCharacterPBRInspector"
}
