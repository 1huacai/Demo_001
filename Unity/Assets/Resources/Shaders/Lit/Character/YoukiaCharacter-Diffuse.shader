//@@@DynamicShaderInfoStart
//非PBR光照
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/YoukiaCharacter-Diffuse" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_MainTex("纹理", 2D) = "white" {}
        [Header(Normal)]
        [Toggle(_NORMAL)]_Toggle_NORMAL_ON("计算法线纹理", Float) = 0
        [NoScaleOffset]_BumpMap("法线纹理", 2D) = "white" {}

        [Header(Specular)]
		_Specular ("高光范围", Range(0, 1)) = 1
		_Gloss ("高光强度", Range(0, 10)) = 1
        _Fresnel ("Fresnel", Range(0.001, 5)) = 4

        [Header(CutOut)]
        _Cutoff ("Alpha 裁剪(硬切)", Range(0, 1)) = 0

        // 换色
        // [Header(ChangeColor)]
        // // [Toggle(_CHANGECOLOR)]_Toggle_CHANGECOLOR_ON("换色", Float) = 0
        // [NoScaleOffset]_ChangeColorTex ("纹理", 2D) = "black" {}
        // [Header(HSV 1)]
        // _HSV_H_1 ("色相", Range(0, 1)) = 0
        // _HSV_S_1 ("饱和度", Range(0, 2)) = 1
        // _HSV_V_1 ("明度", Range(0, 2)) = 1
        // [MaterialToggle]_ColorAdd_1("颜色叠加", float) = 0
        // _HSV_Color_1 ("叠加颜色", Color) = (0.5, 0.5, 0.5, 1)
        // [Header(HSV 2)]
        // _HSV_H_2 ("色相", Range(0, 1)) = 0
        // _HSV_S_2 ("饱和度", Range(0, 2)) = 1
        // _HSV_V_2 ("明度", Range(0, 2)) = 1
        // [MaterialToggle]_ColorAdd_2("颜色叠加", float) = 0
        // _HSV_Color_2 ("叠加颜色", Color) = (0.5, 0.5, 0.5, 1)
        // [Header(HSV 3)]
        // _HSV_H_3 ("色相", Range(0, 1)) = 0
        // _HSV_S_3 ("饱和度", Range(0, 2)) = 1
        // _HSV_V_3 ("明度", Range(0, 2)) = 1
        // [MaterialToggle]_ColorAdd_3("颜色叠加", float) = 0
        // _HSV_Color_3 ("叠加颜色", Color) = (0.5, 0.5, 0.5, 1)
        // [Header(HSV 4)]
        // _HSV_H_4 ("色相", Range(0, 1)) = 0
        // _HSV_S_4 ("饱和度", Range(0, 2)) = 1
        // _HSV_V_4 ("明度", Range(0, 2)) = 1
        // [MaterialToggle]_ColorAdd_4("颜色叠加", float) = 0
        // _HSV_Color_4 ("叠加颜色", Color) = (0.5, 0.5, 0.5, 1)

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
		_Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0           
	}
	SubShader 
    {
		Tags { "RenderType"="Opaque" "ShadowType" = "ST_Opaque" }
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            Stencil
            {
                Ref [_Ref]
                Comp [_Comp]
                Pass [_pass]
            }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
            // ColorMask RGB

			CGPROGRAM
                #pragma multi_compile_instancing
                #pragma multi_compile_fwdbase
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma vertex vert
                #pragma fragment frag

                #pragma target 2.0

                #pragma shader_feature _NORMAL
                // #pragma shader_feature _SPECULAR
                // youkia height fog
                #pragma multi_compile __ _HEIGHTFOG

                // #pragma shader_feature _SHADOW_BLUR
                #pragma multi_compile __ _UNITY_RENDER

                #include "../../Library/YoukiaLight.cginc"
                #include "../../Library/YoukiaEnvironment.cginc"
                #include "../../Library/YoukiaTools.cginc"

                struct appdata_t 
                {
                    half4 vertex : POSITION;
                    fixed2 texcoord : TEXCOORD0;
                    half3 normal : NORMAL;
                    half4 tangent : TANGENT;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    fixed2 texcoord : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;

                    #if _NORMAL
                        half3 TtoW0 : TEXCOORD2;  
                        half3 TtoW1 : TEXCOORD3;  
                        half3 TtoW2 : TEXCOORD4; 
                    #else
                        half3 normal : TEXCOORD2;
                    #endif

                    half3 sh : TEXCOORD5;
                    fixed3 viewDir : TEXCOORD6;
                    fixed3 lightDir : TEXCOORD7;

                    #ifdef _SKYENABLE
                        half3 inscatter : TEXCOORD8;
                        half3 extinction : TEXCOORD9;
                    #endif

                    #ifdef _UNITY_RENDER
                        UNITY_LIGHTING_COORDS(10, 11)
                    #else
                        fixed4 screenPos : TEXCOORD10;
                    #endif
                
                    #ifdef _HEIGHTFOG
                        half4 heightfog : TEXCOORD12;
                    #endif

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                half _Specular, _Gloss, _Fresnel;

                // change color
                // sampler2D _ChangeColorTex;
                // fixed _ColorAdd_1, _ColorAdd_2, _ColorAdd_3, _ColorAdd_4;
                // half4 _HSV_Color_1, _HSV_Color_2, _HSV_Color_3, _HSV_Color_4;
                // half _HSV_H_1, _HSV_S_1, _HSV_V_1;
                // half _HSV_H_2, _HSV_S_2, _HSV_V_2;
                // half _HSV_H_3, _HSV_S_3, _HSV_V_3;
                // half _HSV_H_4, _HSV_S_4, _HSV_V_4;

                // // moving tex
                // sampler2D _MovingTex;
                // half4 _MovingTex_ST;
                // half4 _MovingColor;
                // half _MovingTex_Speed_U, _MovingTex_Speed_V;

                // half3 modifyhsv(half3 hsv, half3 m, half addflg, half3 blend)
                // {
                //     half3 h = hsv;
                //     h.x += m.x;
                //     h.y *= m.y;
                //     h.z *= m.z;
                    
                //     half3 rgb = hsv2rgb(h);
                    
                //     if (addflg != 0)
                //     {
                //         rgb = saturate(blendColor(rgb, blend));
                //     }

                //     return rgb;
                // }

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));

                    #ifdef _UNITY_RENDER
                        UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    #else
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif

                    #if _NORMAL
                        fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                        fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                        fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                        o.TtoW0 = half3(worldTangent.x, worldBinormal.x, worldNormal.x);
                        o.TtoW1 = half3(worldTangent.y, worldBinormal.y, worldNormal.y);
                        o.TtoW2 = half3(worldTangent.z, worldBinormal.z, worldNormal.z);
                    #else
                        o.normal = UnityObjectToWorldNormal(v.normal);
                        fixed3 worldNormal = o.normal;
                    #endif

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    #endif
                    o.sh += YoukiaGI_IndirectDiffuse(worldNormal);

                    // height fog
                    #ifdef _HEIGHTFOG
                        half fog = 0;
                        o.heightfog.rgb = YoukiaHeightFog(o.worldPos, 0, fog);
                        o.heightfog.a = fog;
                    #endif

                    // atmosphere
                    #ifdef _SKYENABLE
                        half3 extinction = 0;
                        // float3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
                        o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, extinction);
                        o.extinction = extinction;
                    #endif

                    return o;
                }
                
                fixed4 frag(v2f i) : SV_Target 
                {
                    UNITY_SETUP_INSTANCE_ID(i);

				    float3 worldPos = i.worldPos;
				    fixed3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;

                    fixed4 c = tex2D(_MainTex, i.texcoord.xy) * _Color;
                    fixed3 albedo = c.rgb;
                    half alpha = c.a;

                    // dissolve
                    clip(alpha - _Cutoff);

                    // change color
                    // #if _CHANGECOLOR
                    //     half4 changeColor = tex2D(_ChangeColorTex, i.texcoord.xy);
                    //     half3 hsv = rgb2hsv(col.rgb);

                    //     // 1 - 最上层
                    //     if (changeColor.r > 0)
                    //         col.rgb = lerp(col.rgb, modifyhsv(hsv, half3(_HSV_H_1, _HSV_S_1, _HSV_V_1), _ColorAdd_1, _HSV_Color_1), changeColor.r);
                    //     // 2
                    //     if (changeColor.g > 0)
                    //         col.rgb = lerp(col.rgb, modifyhsv(hsv, half3(_HSV_H_2, _HSV_S_2, _HSV_V_2), _ColorAdd_2, _HSV_Color_2), saturate(changeColor.g - changeColor.r));
                    //     // 3
                    //     if (changeColor.b > 0)
                    //         col.rgb = lerp(col.rgb, modifyhsv(hsv, half3(_HSV_H_3, _HSV_S_3, _HSV_V_3), _ColorAdd_3, _HSV_Color_3), saturate(changeColor.b - changeColor.g - changeColor.r));
                    //     // 4
                    //     if (changeColor.a > 0)
                    //         col.rgb = lerp(col.rgb, modifyhsv(hsv, half3(_HSV_H_4, _HSV_S_4, _HSV_V_4), _ColorAdd_4, _HSV_Color_4), saturate(changeColor.a - changeColor.b - changeColor.g - changeColor.r));
                    //     colTex = col;
                    // #endif

                    // normal
                    #if _NORMAL
                        fixed3 normal = UnpackNormal(tex2D(_BumpMap, i.texcoord.xy));
                        normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
                        normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
                    #else
                        fixed3 normal = i.normal;
                    #endif

                    // shadow
                    #ifdef _UNITY_RENDER
                        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                        fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                    #else
                        half atten = 0;
                        fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten);
                    #endif
                    
                    // youkia light
                    half4 col;
                    UnityGI gi = GetUnityGI_simplify(normal, i.worldPos, lightDir, viewDir, i.sh);
                     // gi 暗部增强
                    gi.indirect.diffuse = CharacterGIScale(normal, lightDir, colShadow.r, gi.indirect.diffuse, _gCharacterGIScale);
                    // character light color
                    gi.light.color = CharacterColorLerp();
                    gi.indirect.diffuse = CharacterGILerp(gi.indirect.diffuse);

                    YoukiaLightingCharacterData data = YoukiaDataCharacter(albedo, normal, viewDir, colShadow, _Gloss, _Specular, _Fresnel, gi.indirect, gi.light);
                    col.rgb = YoukiaLightCharacter(data);
                    col.a = alpha;

                    return col;
                }
			
			ENDCG
		}

	}
	FallBack "YoukiaEngine/Lighting/YoukiaDiffuse"
    // CustomEditor "YoukiaCharacterInspector"
}
