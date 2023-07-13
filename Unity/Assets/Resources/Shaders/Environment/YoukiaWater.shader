//@@@DynamicShaderInfoStart
//基础水体Shader。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Environment/YoukiaWater"
{
	Properties
	{
		_ShadowIntensity("水面阴影接受强度", Range(0,1)) = 1
		_Color ("水颜色", Color) = (1, 1, 1, 1)
		_Wave1Tex ("法线贴图1", 2D) = "white" {}
		_Wave2Tex ("法线贴图2", 2D) = "white" {}
		_NormalStrength ("法线强度", Range(0, 2)) = 0.5
		[MaterialToggle]_UVWorldSpace ("世界坐标uv", Float) = 0
		_TextureScale ("贴图缩放", Float) = 1
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
		//[MaterialToggle] _CameraLight ("相机高光方向", Float) = 0
		[Enum(Custom, 0, Camera, 1)] _SecLightDir("高光2方向", Float) = 0
		//_CustomLightDir ("自定义高光2方向", Vector) = (1, 0, 0, 0)

		[Header(Depth)]
		_ColorDepth ("深度颜色（Alpha：透明度）", Color) = (1, 1, 1, 1)
		_FadeDepth ("深度偏移", Range(-10, 10)) = 0
		_FadeDepthPower ("深度过渡", Range(0.001, 20)) = 1
		// _FadeDepthMax ("最大深度", Range(0, 1)) = 1

		[Header(Ref)]
		_RefColor ("反射颜色", Color) = (1, 1, 1, 1)
		_RefFactor ("反射强度", Range(0, 2)) = 0.5
		_Fresnel ("Fresnel", Range(0, 10)) = 4
		[Header(Ref Screen)]
		[Toggle(_REFLECT_SS)]_Toggle_REFLECT_SS_ON("平面空间反射", float) = 0

		[Header(Foam)]
		[HDR]_FoamColor ("边缘颜色（Alpha：透明度）", Color) = (1, 1, 1, 1)
		_FoamFade ("边缘过渡", Range(0, 50)) = 1
		_FoamWidth ("边缘泡沫距离", Range(0, 10)) = 1.5
		_FoamNoise ("边缘泡沫噪声强度", Range(0, 0.1)) = 0.05
		_FoamCut ("边缘泡沫宽度", Range(0, 0.26)) = 0.25

		[Header(Caustics)]
		[HDR]_CausticsColor ("焦散颜色", Color) = (0, 0, 0, 1)
		[NoScaleOffset]_CausticsTex ("焦散纹理", 2D) = "black" {}
		_CausticsScale ("焦散纹理缩放", Range(0, 2)) = 0.5
		_CausticsSpeed ("焦散速度", Range(0, 100)) = 0.5
		_CausticsDistort ("焦散扭曲", Range(0, 1)) = 0.5

		[Header(Wave)]
        _Dir ("波纹方向", Vector) = (1.0, 1.0, -1.0, -1.0)

		[Header(Environment)]
		_Envir ("环境系数#注意：一定要将水的层级设置为Water。#Layer->Water。", Range(0, 1)) = 0.5

		[Header(Stencil Test)]
		_Ref("Stencil Reference", Range(0, 255)) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
		[Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 1
	}
	SubShader
	{
		Tags { "Queue"="Transparent-1" "RenderType"="Transparent" }

		Pass
		{
            Tags { "LightMode"="ForwardBase" }

			Stencil
			{
				Ref [_Ref]
				Comp [_Comp]
				Pass [_Pass]
			}

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
				#define GI_TMPFIX 1
				// #pragma target 3.0
				
				#pragma multi_compile __ _REFLECT _REFLECT_SS
				#pragma multi_compile __ _HEIGHTFOG
				#pragma multi_compile __ _UNITY_RENDER
				#pragma multi_compile __ _PP_HEIGHTFOG
				#pragma multi_compile __ _SKYENABLE
				#pragma multi_compile __ _TAIL
				#pragma shader_feature __ _CAUSTICS
				
				#include "../Library/YoukiaLight.cginc"
				#include "../Library/YoukiaEnvironment.cginc"
				#include "../Library/Atmosphere.cginc"
				#include "../Library/YoukiaMrt.cginc"
				#include "YoukiaWater.cginc"

				float4x4 _gReflectVP;
				// x: strength, y: distort
				half4 _gTailParam;

				half _CustomLight;
				half _SecLightDir;

				half _ShadowIntensity;

				v2f vert (appdata v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
					WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir.xyz);
					half3 refCol = _RefFactor * _RefColor;

					float2 uvSpeed1 = _Time.xx * _Dir.xy;
					float2 uvSpeed2 = _Time.xx * _Dir.zw;
					UNITY_BRANCH
					if (_UVWorldSpace)
					{
						o.uvWave.xy = o.worldPos.xz * _TextureScale * 0.0005 + uvSpeed1;
						o.uvWave.zw = o.worldPos.xz * _TextureScale * 0.0005 + uvSpeed2;
					}
					else
					{
						o.uvWave.xy = v.texcoord.xy * _Wave1Tex_ST.xy * -_TextureScale + _Wave1Tex_ST.zw + uvSpeed1;
						o.uvWave.zw = v.texcoord.xy * _Wave2Tex_ST.xy * -_TextureScale + _Wave2Tex_ST.zw + uvSpeed2;
					}

					half3 worldNormal = 0;
					T2W(v.normal, v.tangent, o.TtoW, worldNormal, refCol);

					#if defined (UNITY_UV_STARTS_AT_TOP)
						o.sh.xyz = ShadeSHPerVertex(worldNormal, o.sh.xyz);
					#endif
					o.sh.xyz += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor);
					o.sh = max(0, o.sh);

					o.proj = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.proj.z);

					o.vertColor = v.color;
					// fresnel
					o.viewDir.w = saturate(pow(1 - saturate(dot(o.viewDir.xyz, worldNormal)), _Fresnel));
					// distort
					o.lightDir.w = _Distort * saturate(o.vertColor.a);

					// height fog
					YOUKIA_TRANSFER_HEIGHTFOG(o, 0)
					// atmosphere
    				YOUKIA_TRANSFER_ATMOSPHERE(o, _WorldSpaceCameraPos)

					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);

					float3 worldPos = i.worldPos;
					half3 viewDir = i.viewDir.xyz;
					half3 lightDir = i.lightDir;

					// tail
					half tail = 0;
					half tailDepth = 0;
					#ifdef _TAIL
						half2 tailValue = (Tail(worldPos).rg * 2 - 1) * _gTailParam.x;
						tail = tailValue.x;
						tailDepth = tail * 0.25f;
					#endif

					// normal
					half3 normal1 = UnpackNormal(tex2D(_Wave1Tex, i.uvWave.xy)).rgb;
					half3 normal2 = UnpackNormal(tex2D(_Wave2Tex, i.uvWave.zw)).rgb;
					half3 normal = normal1 + normal2;
					
					#ifdef _TAIL
						normal.xy = lerp(normal.xy, half2(-1, -1), saturate(tail));
						normal.xy *= (_NormalStrength * (1 + tail));
					#else
						normal.xy *= _NormalStrength;
					#endif
					
					normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
					normal = normalize(normal);

					// depth
					half distort = i.lightDir.w;
					half2 uv = 0;
					half2 uvScreen = i.proj.xy / i.proj.w;
					float depth = 0;
					half linearDepth = 0;
					half edgeFade = 0;
					half depthDifference = 0;
					depth = CalcDepthEdge(uvScreen, normal, distort, i.proj, i.vertColor.r, linearDepth, uv, edgeFade, depthDifference);
					linearDepth = saturate(linearDepth + tailDepth);

					half4 colGrab = tex2D(_gGrabTex, uv);
					half3 caustics = Caustics(uv, normal, depth) * edgeFade;
					colGrab.rgb = colGrab * (1 + caustics);
					half3 albedo = lerp(_Color * colGrab.rgb, lerp(colGrab.rgb * _ColorDepth, _ColorDepth, _ColorDepth.a), linearDepth);
					normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
					half3 normalNoise = normal;

					half3 mirror = 1;
					half3 refCol = half3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w);
					mirror = refCol * i.sh;
					half2 uvDistort = normal.xz * distort;

					// 反射相机反射
					#ifdef _REFLECT
						fixed2 uvMirror = fixed2((i.proj.xy + uvDistort) / i.proj.w);
						mirror = saturate(tex2D(_gReflectTex, uvMirror)).rgb;
						mirror.rgb *= refCol;
					#elif _REFLECT_SS
						half2 uvMirror = ProjReflectCoord(_gReflectVP, worldPos) + uvDistort;
						mirror = tex2D(_gGrabTex, uvMirror);
						mirror.rgb *= refCol;

						half mirrorfade = Pow4(uvMirror.y);
						mirrorfade = Pow4(mirrorfade);

						// 消除非平面的反射
						half up = saturate(dot(normal, half3(0, 1, 0)));
						up = Pow5(up);
						up = Pow5(up);
						up = Pow5(up);

						mirrorfade = lerp(1, mirrorfade, _gReflectStrength);
						mirror = lerp(mirror, refCol, saturate(mirrorfade));
						mirror = lerp(0, mirror, up);
					#endif

					// light
					half ndotl = max(_MinNdL, dot(normal, lightDir));

					// final color
					half4 colorFinal = 1;
					colorFinal.rgb = albedo * ndotl * _LightColor0.rgb;
					
					// foam
					half foam = 0;
					half4 colFoam = Foam(foam, depthDifference, (normalNoise.z + normalNoise.x) * 0.5f);

					// fresnel
					fixed fresnel = i.viewDir.w;
					colorFinal.rgb = max(lerp(colorFinal, mirror, fresnel), 0) + colFoam * _FoamColor.a;

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
						SecondSpecular(waterSpecular, normal1, normal2, lightDir, viewDir, i.TtoW);
					#else
						half3 secSpecLitDir = lerp(_CustomLightDir, _gMainCameraReflectLightDir, _SecLightDir);
						lightDir = lerp(lightDir, secSpecLitDir, _CustomLight);
						SecondSpecular(waterSpecular, normal1, normal2, lightDir, viewDir, i.TtoW);
					#endif

					#if UNITY_UV_STARTS_AT_TOP
					#else
						waterSpecular = clamp(waterSpecular, 0, SPECMAX);
					#endif
					
					// shadow
					half atten = 1;
					#ifndef _UNITY_RENDER
						YoukiaScreenShadow(uvScreen, atten);
						atten = lerp(1, atten, _ShadowIntensity);
					#endif

					colorFinal.rgb = colorFinal.rgb + waterSpecular * fresnel * atten + i.sh * _Envir;

					// height fog
    				YOUKIA_HEIGHTFOG(colorFinal, i)
					// atmosphere
    				YOUKIA_ATMOSPHERE(colorFinal, i)

					// post fog
					colorFinal = PostHeightFog(worldPos, colorFinal);
					
					colorFinal.a = lerp(edgeFade, 1, saturate(foam * _FoamColor.a));

					// lum
                    colorFinal.rgb = SceneLumChange(colorFinal.rgb);
					
					return colorFinal;
				}
			ENDCG
		}
	}
	// FallBack "VertexLit"
	CustomEditor "YoukiaWaterInspector"
}
