Shader "YoukiaEngine/Environment/YoukiaWaterfall"
{
    Properties
	{
		_Color ("水颜色", Color) = (1, 1, 1, 1)
		_BumpMap ("法线贴图", 2D) = "white" {}
		_NormalStrength ("法线强度", Range(0, 2)) = 0.5
		_MinNdL ("diffuse调整参数", Range(0, 1)) = 0.25
		[Header(Distortion)]
		_Distort("扭曲", Range(0, 0.5)) = 0.25

		[Header(Shinniess)]
		[HDR]_SpecularColor ("高光颜色", Color) = (1, 1, 1, 1)
		_Shinniess ("Shinniess", Range(0, 256)) = 0.9
		[MaterialToggle] _Specular2 ("高光2", Float) = 0
		[HDR]_SpecularColor2 ("高光2颜色", Color) = (1, 1, 1, 1)
		_SpecularScale ("高光2聚集度", Range(0, 2)) = 0.5
		_Shinniess2 ("Shinniess2", Range(0, 256)) = 1
		[MaterialToggle] _CustomLight ("自定义高光2", Float) = 0
		[Enum(Custom, 0, Camera, 1)] _SecLightDir("高光2方向", Float) = 0

		[Header(Depth)]
		_ColorDepth ("深度颜色（Alpha：透明度）", Color) = (1, 1, 1, 1)
		_FadeDepth ("深度偏移", Range(-10, 10)) = 0
		_FadeDepthPower ("深度过渡", Range(0.001, 20)) = 1

		[Header(Ref)]
		_RefColor ("反射颜色", Color) = (1, 1, 1, 1)
		_RefFactor ("反射强度", Range(0, 2)) = 0.5
		_Fresnel ("Fresnel", Range(0, 10)) = 4

		[Header(WaterFallTexture)]
        _WaterfallColor ("瀑布泡沫颜色", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_WaterfallTex ("瀑布泡沫纹理", 2D) = "white" {}
		_WaterfallTexScale ("瀑布泡沫纹理 Scale", Vector) = (1, 1, 1, 1)
        _WaterfallCutoff ("瀑布泡沫末端 Alpha 裁剪", Range(0, 2)) = 0
		_WaterfallSpeedUp ("瀑布加速", Range(1, 2)) = 1.2

		[Header(Foam)]
		_FoamFade ("边缘过渡", Range(0, 2)) = 1
		_FoamWidth ("边缘泡沫范围 (值越大，范围越集中)", Range(1, 10)) = 1
		_FoamTex ("边缘泡沫贴图", 2D) = "white" {}
		[HDR]_FoamColor ("边缘颜色（Alpha：透明度）", Color) = (1, 1, 1, 1)
		_FoamSpeed ("边缘泡沫速度", Range(1, 10)) = 1
		_FoamDepth ("边缘泡沫偏移系数", Range(0, 1)) = .5

		[Header(Wave)]
        _Dir ("波纹方向", Vector) = (1.0, 1.0, -1.0, -1.0)

		[Header(Environment)]
		_Envir ("环境系数#注意：一定要将水的层级设置为Water。#Layer->Water。", Range(0, 1)) = 0.5
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }

		Pass
		{
            Tags { "LightMode"="ForwardBase" }

			ZWrite On
			ZTest LEqual
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha 
			ColorMask RGB

			CGPROGRAM
				#pragma multi_compile_fwdbase nolightmap
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_instancing

				#pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            	#pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

				#pragma vertex vert
				#pragma fragment frag
				// #pragma target 3.0
				
				#pragma multi_compile __ _HEIGHTFOG
				#pragma multi_compile __ _UNITY_RENDER
				// #pragma multi_compile __ _PP_HEIGHTFOG
				#pragma multi_compile __ _SKYENABLE
				
				#include "../Library/YoukiaLight.cginc"
				#include "../Library/YoukiaEnvironment.cginc"
				#include "../Library/Atmosphere.cginc"
				#include "../Library/YoukiaMrt.cginc"
				#include "YoukiaWater.cginc"

                // water fall
                half4 _WaterfallColor;
                sampler2D _WaterfallTex;
                half4 _WaterfallTex_ST;
                half _WaterfallStrength;
                half _WaterfallCutoff;
				half4 _WaterfallTexScale;
				half _WaterfallSpeedUp;

				half _CustomLight;
				half _SecLightDir;

				sampler2D _FoamTex;
				half4 _FoamTex_ST;
				half _FoamSpeed;
				half _FoamDepth;

				half4 _BumpMap_ST;
				
				struct v2fWaterfall
				{
					float4 pos : SV_POSITION;
					float4 uvWave : TEXCOORD0;
					float4 worldPos : TEXCOORD1;
					float4 proj : TEXCOORD2;
					half4 TtoW[3] : TEXCOORD3;

					half4 sh : TEXCOORD6;

					YOUKIA_ATMOSPERE_DECLARE(7, 8)
					YOUKIA_HEIGHTFOG_DECLARE(9)

					half3 giColor : TEXCOORD10;
					half4 lightDir : TEXCOORD11;
					half4 viewDir : TEXCOORD12;
					#if _UNITY_RENDER
						half3 camLightDir : TEXCOORD13;
					#endif
					float4 uvWaterfall : TEXCOORD14;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				v2fWaterfall vert (appdata v)
				{
					v2fWaterfall o;
					UNITY_INITIALIZE_OUTPUT(v2fWaterfall, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
					WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir.xyz);

                    o.uvWave.xy = TRANSFORM_TEX(v.texcoord.xy, _BumpMap) - _Time.x * _Dir.xy;
					o.uvWave.zw = v.texcoord.xy;
					o.uvWaterfall.xy = _Time.x * _Dir.xy;
					o.uvWaterfall.zw = _Time.x * _Dir.zw;

					half3 worldNormal = 0;
					T2W(v.normal, v.tangent, o.TtoW, worldNormal, v.color.aaa);

					#if defined (UNITY_UV_STARTS_AT_TOP)
						o.sh.xyz = ShadeSHPerVertex(worldNormal, o.sh.xyz);
					#endif
					o.sh.xyz += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);

					o.proj = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.proj.z);
					#if _UNITY_RENDER
						half3 camFwd = normalize(mul((float3x3)unity_CameraToWorld, float3(0, 0, 1)));
						o.camLightDir = normalize(-reflect(-camFwd, half3(0, 1, 0)));
					#endif

					// fresnel
					o.viewDir.w = saturate(pow(1 - saturate(dot(o.viewDir.xyz, worldNormal)), _Fresnel));
					// distort
					o.lightDir.w = _Distort * saturate(v.color.a);

					// height fog
					YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
					// atmosphere
    				YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

					return o;
				}

				fixed4 frag (v2fWaterfall i) : SV_Target
				{
                    UNITY_SETUP_INSTANCE_ID(i);

					float3 worldPos = i.worldPos;
					half3 viewDir = i.viewDir.xyz;
					half3 lightDir = i.lightDir;
					half2 vertCol = half2(i.TtoW[0].w, i.TtoW[1].w);
					half2 texcoord = i.uvWave.zw;

					// normal
					half3 worldNormal = normalize(half3(i.TtoW[0].z, i.TtoW[1].z, i.TtoW[2].z));
					half3 normal1 = UnpackNormal(tex2D(_BumpMap, i.uvWave.xy)).rgb;
					half3 normal = normal1;

					normal.xy *= _NormalStrength;
					normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
					
					// depth
					half distort = i.lightDir.w;
					
					half2 uv = 0;
					half2 uvScreen = i.proj.xy / i.proj.w;
					half depth = 0;
					half edgeFade = 0;
					half depthDifference = 0;
					CalcDepthEdge(uvScreen, normal, distort, i.proj, vertCol.x, depth, uv, edgeFade, depthDifference);

					half4 colGrab = tex2D(_gGrabTex, uv);
					half3 albedo = lerp(_Color * colGrab.rgb, lerp(colGrab.rgb * _ColorDepth, _ColorDepth, _ColorDepth.a), depth);
					normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                    // water fall
					float4 waterfallUV;
					float2 waterfallCoord = texcoord;
					waterfallCoord.x = saturate(pow(texcoord.x, _WaterfallSpeedUp));
					// waterfallCoord.x = pow((texcoord.x), 2) * _WaterfallSpeedUp;
					waterfallUV.xy = waterfallCoord.xy * _WaterfallTexScale.xy - i.uvWaterfall.xy;
                    waterfallUV.zw = waterfallCoord.xy * _WaterfallTexScale.zw - i.uvWaterfall.zw;
                    half3 waterfall = tex2D(_WaterfallTex, waterfallUV.xy);
                    waterfall += tex2D(_WaterfallTex, waterfallUV.zw);
                    waterfall *= _WaterfallColor * 0.5;

					half waterfallFade = 1 - texcoord.x;
                    waterfall *= waterfallFade;
                    albedo = waterfall + albedo;

                    // water fall cut off
                    half waterfallAlpha = lerp(1, waterfall, waterfallFade);
                    half waterfallCutoff = lerp(0, _WaterfallCutoff * waterfallFade, waterfallFade);
                    clip(waterfallAlpha - waterfallCutoff);

					// ref
					half3 mirror = _RefFactor * _RefColor;

					// light
					half ndotl = max(_MinNdL, dot(normal, lightDir));
					
					// final color
					half4 colorFinal = 1;
					
					// foam
					half foam = 0;
					float edgeDepth = saturate(edgeFade * _FoamWidth);
					float2 foamUV = TRANSFORM_TEX(texcoord, _FoamTex);
					foamUV.y += 1 - saturate(exp2(-_FoamDepth * depthDifference));
					half4 foamTex = tex2D(_FoamTex, foamUV - _Time.x * _Dir.xy * _FoamSpeed);
					foamTex *= (1 - edgeDepth) * _FoamColor;

					colorFinal.rgb = (albedo + foamTex.rgb) * ndotl * _LightColor0.rgb;
					
					// fresnel
					fixed fresnel = i.viewDir.w;
					colorFinal.rgb = saturate(lerp(colorFinal, mirror, fresnel));// + colFoam * _FoamColor.a;
					
					half3 h = normalize(lightDir + viewDir);
					half nh = max(0, dot(normalize(normal), h));
					half spec = max(0, pow(nh, _Shinniess * 128));
					half3 waterSpecular = _SpecularColor.rgb * spec;

					// 补高光
					#if _UNITY_RENDER
						half3 camFwd = normalize(mul((float3x3)unity_CameraToWorld, float3(0, 0, 1)));
						half3 camLightDir = normalize(-reflect(-camFwd, half3(0, 1, 0)));

						half3 secSpecLitDir = lerp(_CustomLightDir, camLightDir, _SecLightDir);
						lightDir = lerp(lightDir, secSpecLitDir, _CustomLight);
						SecondSpecular(waterSpecular, normal1, lightDir, viewDir, i.TtoW);
					#else
						half3 secSpecLitDir = lerp(_CustomLightDir, _gMainCameraReflectLightDir, _SecLightDir);
						lightDir = lerp(lightDir, secSpecLitDir, _CustomLight);
						SecondSpecular(waterSpecular, normal1, lightDir, viewDir, i.TtoW);
					#endif

					// shadow
					half atten = 1;
					#ifndef _UNITY_RENDER
						YoukiaScreenShadow(uvScreen, atten);
					#endif

					colorFinal.rgb += waterSpecular * fresnel * atten + i.sh * _Envir;

					// height fog
    				YOUKIA_HEIGHTFOG(colorFinal, i)
					// atmosphere
    				YOUKIA_ATMOSPHERE(colorFinal, i)

					// post fog
					colorFinal = PostHeightFog(worldPos, colorFinal);

					// colorFinal.a = lerp(edgeFade, 1, saturate(foam * _FoamColor.a));
					colorFinal.a = edgeFade;
					// return colorFinal.aaaa;

					// lum
                    colorFinal.rgb = SceneLumChange(colorFinal.rgb);

					return colorFinal;
				}
			ENDCG
		}
	}
    // FallBack "Transparent/VertexLit"
    CustomEditor "YoukiaWaterfallInspector"
}
