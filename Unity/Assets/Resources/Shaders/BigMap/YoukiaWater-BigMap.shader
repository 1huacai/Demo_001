//@@@DynamicShaderInfoStart
//大地图水体Shader。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/BigMap/YoukiaWater-BigMap"
{
	Properties
	{
		_Color ("水颜色", Color) = (1, 1, 1, 1)
		_Wave1Tex ("法线贴图1", 2D) = "white" {}
		_Wave2Tex ("法线贴图2", 2D) = "white" {}
		_NormalStrength ("法线强度", Range(0, 2)) = 0.5
		_TextureScale ("贴图缩放", Float) = 1
		_MinNdL ("diffuse调整参数", Range(0, 1)) = 0.25
		[Header(Distortion)]
		_Distort("扭曲", Range(0, 10)) = 0.25

		[Header(Shinniess)]
		[HDR]_SpecularColor ("高光颜色", Color) = (1, 1, 1, 1)
		_Shinniess ("Shinniess", Range(0, 256)) = 0.9
		[MaterialToggle] _Specular2 ("高光2", Float) = 0
		[HDR]_SpecularColor2 ("高光2颜色", Color) = (1, 1, 1, 1)
		_SpecularScale ("高光2聚集度", Range(0, 2)) = 0.5
		_Shinniess2 ("Shinniess2", Range(0, 256)) = 1
		[MaterialToggle] _CustomLight ("自定义高光2", Float) = 0
		[MaterialToggle] _CameraLight ("相机高光方向", Float) = 0
		_CustomLightDir ("自定义高光2方向", Vector) = (1, 0, 0, 0)

		[Header(Depth)]
		_ColorDepth ("深度颜色（Alpha：透明度）", Color) = (1, 1, 1, 1)
		_FadeDepth ("深度偏移", Range(-10, 10)) = 0
		_FadeDepthPower ("深度过渡", Range(0.001, 20)) = 1

		[Header(Ref)]
		_RefColor ("反射颜色", Color) = (1, 1, 1, 1)
		_RefFactor ("反射强度", Range(0, 2)) = 0.5
		_Fresnel ("Fresnel", Range(0, 10)) = 4

		[Header(Foam)]
		[HDR]_FoamColor ("边缘颜色", Color) = (1, 1, 1, 1)
		_FoamFade ("边缘过渡", Range(0, 1)) = 1

		[Header(Caustics)]
		[HDR]_CausticsColor ("焦散颜色", Color) = (0, 0, 0, 1)
		[NoScaleOffset]_CausticsTex ("焦散纹理", 2D) = "black" {}
		_CausticsScale ("焦散纹理缩放", Range(0, 1)) = 0.5
		_CausticsSpeed ("焦散速度", Range(0, 10)) = 0.5
		_CausticsDistort ("焦散扭曲", Range(0, 1)) = 0.5

		[Header(Wave)]
        _Dir ("波纹方向", Vector) = (1.0, 1.0, -1.0, -1.0)

		[Header(Environment)]
		_Envir ("环境系数#注意：一定要将水的层级设置为Water。#Layer->Water。", Range(0, 1)) = 0.5
	}
	SubShader
	{
		Tags { "Queue"="Transparent-1" "RenderType"="Transparent" }

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
				
				// #pragma multi_compile __ _HEIGHTFOG
				#pragma multi_compile __ _UNITY_RENDER
				// #pragma multi_compile __ _PP_HEIGHTFOG
				#pragma multi_compile __ _PP_FOGOFWAR
				#pragma multi_compile __ _EM
				// #pragma multi_compile __ _HEIGHTFOG_NOISE
				// #pragma multi_compile __ _CAUSTICS
				
				#include "../Library/YoukiaLight.cginc"
				#include "../Library/YoukiaEnvironment.cginc"
				#include "../Library/Atmosphere.cginc"
				#include "../Library/YoukiaMrt.cginc"
				#include "../Environment/YoukiaWater.cginc"

				// fog of war
				sampler2D _gFowTex;
				half4 _FowColor;
				half _FowWaterScale;

				struct v2fbase
				{
					float4 pos : SV_POSITION;
					half4 uvWave : TEXCOORD0;
					float4 worldPos : TEXCOORD1;
					float4 proj : TEXCOORD2;
					half4 TtoW[3] : TEXCOORD3;

					half4 sh : TEXCOORD6;
					half4 vertColor : TEXCOORD7;

					// YOUKIA_HEIGHTFOG_DECLARE(10)

					half3 giColor : TEXCOORD11;
					half4 lightDir : TEXCOORD12;
					half4 viewDir : TEXCOORD13;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				v2fbase vert (appdata v)
				{
					v2fbase o;
					UNITY_INITIALIZE_OUTPUT(v2fbase, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
					WorldViewLightDir(o.worldPos, o.viewDir.xyz, o.lightDir.xyz);
					half3 refCol = _RefFactor * _RefColor;

					float2 uvSpeed1 = _Time.xx * _Dir.xy;
					float2 uvSpeed2 = _Time.xx * _Dir.zw;
					o.uvWave.xy = v.texcoord.xy * _Wave1Tex_ST.xy * -_TextureScale + _Wave1Tex_ST.zw + uvSpeed1;
					o.uvWave.zw = v.texcoord.xy * _Wave2Tex_ST.xy * -_TextureScale + _Wave2Tex_ST.zw + uvSpeed2;

					half3 worldNormal = 0;
					T2W(v.normal, v.tangent, o.TtoW, worldNormal, refCol);

					o.proj = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.proj.z);

					o.vertColor = v.color;
					EMData emData;
    				YoukiaEMLod(o.worldPos, emData);

					#if defined (UNITY_UV_STARTS_AT_TOP)
						o.sh.xyz = ShadeSHPerVertex(worldNormal, o.sh.xyz);
					#endif
					o.sh.xyz += YoukiaGI_IndirectDiffuse(worldNormal, o.worldPos, o.giColor, emData);
					o.sh = max(0, o.sh * _Envir);

					// fresnel
					o.viewDir.w = saturate(pow(1 - saturate(dot(o.viewDir.xyz, worldNormal)), _Fresnel));
					// distort
					o.lightDir.w = _Distort * saturate(o.vertColor.a);

					// height fog
					// YOUKIA_TRANSFER_HEIGHTFOG_EM(o, 0, emData)

					return o;
				}

				fixed4 frag (v2fbase i) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(i);

					float3 worldPos = i.worldPos;
					half3 viewDir = i.viewDir.xyz;
					half3 lightDir = i.lightDir;
					// 定点数太少，会有高光偏移的问题
					viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

					// multi
					EMData emData;
					YoukiaEM(worldPos, emData);
					half4 color = GetWaterColor(emData);
					half4 colorDepth = GetWaterDepthColor(emData);
					half4 colorFoam = GetWaterFoamColor(emData);

					// normal
					half3 normal1 = UnpackNormal(tex2D(_Wave1Tex, i.uvWave.xy)).rgb;
					half3 normal2 = UnpackNormal(tex2D(_Wave2Tex, i.uvWave.zw)).rgb;

					half3 normal = (normal1 + normal2);
					normal.xy *= _NormalStrength;
					normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
					
					// depth
					half distort = i.lightDir.w;
					
					half2 uv = 0;
					half2 uvScreen = i.proj.xy / i.proj.w;
					float depth = 0;
					half linearDepth = 0;
					half edgeFade = 0;
					half depthDifference = 0;
					depth = CalcDepthEdge(uvScreen, normal, distort, i.proj, i.vertColor.r, linearDepth, uv, edgeFade, depthDifference, emData);
					
					// half4 colGrab = tex2D(_gGrabTex, uv);
					half3 caustics = Caustics(uv, normal, depth);
					// half3 albedo = lerp(color * colGrab.rgb + color * caustics, lerp(colGrab.rgb * colorDepth, colorDepth, colorDepth.a), linearDepth);
					half3 albedo = lerp(color + color * caustics, colorDepth, linearDepth);
					half alpha = lerp(color.a, colorDepth.a, linearDepth);
					normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

					// reflect
					half3 mirror = half3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w);

					// light
					half ndotl = max(_MinNdL, dot(normal, lightDir));

					// final color
					half4 colorFinal = 1;
					colorFinal.rgb = albedo * ndotl * _LightColor0.rgb;
					colorFinal.rgb = lerp(colorFoam.rgb, colorFinal.rgb, edgeFade);
					
					// fresnel
					fixed fresnel = i.viewDir.w;
					colorFinal.rgb = lerp(colorFinal, mirror, fresnel);

					half3 h = normalize(lightDir + viewDir);
					half nh = max(0, dot(normal, h));
					half spec = max(0, pow(nh, _Shinniess * 128));
					half3 waterSpecular = _SpecularColor.rgb * spec;

					// 补高光
					#if _UNITY_RENDER
						half3 camFwd = normalize(mul((float3x3)unity_CameraToWorld, float3(0, 0, 1)));
						half3 camLightDir = normalize(-reflect(-camFwd, half3(0, 1, 0)));
						SecondSpecularBigmap(waterSpecular, normal, viewDir, camLightDir);
					#else
						SecondSpecularBigmap(waterSpecular, normal, viewDir, _gMainCameraReflectLightDir);
					#endif

					#if UNITY_UV_STARTS_AT_TOP
					#else
						waterSpecular = clamp(waterSpecular, 0, SPECMAX);
					#endif
					
					// shadow
					half atten = 1;
					#ifndef _UNITY_RENDER
						YoukiaScreenShadow(uvScreen, atten);
					#endif

					colorFinal.rgb = colorFinal.rgb + waterSpecular * fresnel * atten + i.sh;

					// height fog
    				// YOUKIA_HEIGHTFOG(colorFinal, i)

					// post fog
					// colorFinal = PostHeightFog(uvScreen, colorFinal);
					// #if _PP_HEIGHTFOG
					// 	half4 heightFog = PostHeightFog(worldPos, emData);
					// 	colorFinal.rgb = lerp(colorFinal.rgb, heightFog.rgb, heightFog.a);
					// #endif

					colorFinal.rgb = BigMapFog(colorFinal.rgb, worldPos, emData);
					
					// lum
                    colorFinal.rgb = SceneLumChange(colorFinal.rgb);

					// fog of war
					#if _PP_FOGOFWAR
						half4 fow = tex2D(_gFowTex, uvScreen);
						half fogOfWar = lerp(0, fow.x * _FowColor.a, fow.y);
						fogOfWar = lerp(0, fogOfWar, _FowWaterScale);
						colorFinal.rgb = lerp(colorFinal.rgb, _FowColor.rgb, fogOfWar);
					#endif

					colorFinal.a *= edgeFade * alpha;

					return colorFinal;
				}
			ENDCG
		}
	}
	// FallBack "VertexLit"
	CustomEditor "YoukiaWaterBigMapInspector"
}
