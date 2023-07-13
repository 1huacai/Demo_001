Shader "YoukiaEngine/Environment/MenPai/ParallaxCloud" 
{
	Properties {
		_CloudColor("Color",Color) = (1,1,1,1)
		_CloudMap("MainTex",2D)="white"{}
		_Alpha("Alpha", Range(0,2)) = 1
		_Height("Displacement Amount",range(0,1)) = 0.15
		_HeightAmount("Turbulence Amount",range(0,2)) = 1
		_HeightTileSpeed("Turbulence Tile&Speed",Vector) = (1.0,1.0,0.05,0.0)
		_LightIntensity ("Ambient Intensity", Range(0,3)) = 1.0
		[Toggle] _UseFixedLight("Use Fixed Light", Int) = 1
		_FixedLightDir("Fixed Light Direction", Vector) = (0.981, 0.122, -0.148, 0.0)
		_NoiseScale("NoiseScale", Range(1, 30)) = 10
		[Space(20)]
		[Toggle(_USE_SOFTPARTICLE)]_Toggle_USE_SOFTPARTICLE_ON("软粒子", float) = 0
		_SoftFade ("柔和度", Range(0.01, 3)) = 1.0
		[Space(20)]
		[Toggle] _IsMenPai("是否是门派迷雾", Float) = 0
		[Toggle] _IsQfz("是否是清风镇迷雾", Float) = 0
		_ColorMultiply ("颜色增幅", Range(1,2)) = 1
	}
	SubShader 
	{	
		Tags 
		{
			"IgnoreProjector"="True"
			"Queue"="Transparent"
			"RenderType"="Transparent"
		}

		Pass
		{
			Name "FORWARD"
			Tags 
			{
				"LightMode"="ForwardBase"
			}
			Blend SrcAlpha OneMinusSrcAlpha

			ZWrite Off
			// Cull Off
			// ZTest Always
			

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_fwdbase
			

			#pragma multi_compile __ _USE_SOFTPARTICLE

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "Assets/Resources/Shaders/Library/YoukiaEffect.cginc"
			#include "Assets/Resources/Shaders/Library/YoukiaEnvironment.cginc"
			
			#pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
			#pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

			//_Color
			sampler2D _CloudMap;
			half4 _CloudMap_ST;
			half _Height;
			half4 _HeightTileSpeed;
			half _HeightAmount;
			half4 _CloudColor;
			half _Alpha;
			half _LightIntensity;

			half4 _LightingColor;
			half4 _FixedLightDir;	
			half _UseFixedLight;

			half _NoiseScale;

			half4 _TerrianFogParam;

			half4 _TerrianSampleOffset;
			float4 _gFowMask_TexelSize;

			half _IsMenPai;
			half _IsQfz;
			half _ColorMultiply;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				half3 normalDir : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float3 posWorld : TEXCOORD3;
				float2 uv2 : TEXCOORD4;
				float2 uv3 : TEXCOORD5;
				half4 projPos : TEXCOORD6;
				half3 vertColor : TEXCOORD7;
			};


			half2 UVOrthographicWithOffset(float3 wsPos, float3 camPos, float size, half2 texelSize, half xOffset, half yOffset)
			{
				half2 uv = wsPos.xz - camPos.xz;
				uv = uv / (size * 2);
				uv += 0.5f;

			    uv.x -= xOffset * texelSize.x;
			    uv.y -= yOffset * texelSize.y;

			    return uv;
			}


			v2f vert (appdata_full v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord,_CloudMap) + frac(_Time.y*_HeightTileSpeed.zw);
				o.uv2 = TRANSFORM_TEX(v.texcoord,_CloudMap);
				// o.uv = v.texcoord.xy * _ParallaxTiling.xx + frac(_Time.y*_HeightTileSpeed.zw);	//无极缩放匹配...
				// o.uv2 = v.texcoord.xy * _ParallaxTiling.xx;
				o.uv3 = v.texcoord * _NoiseScale - frac(_Time.y*_HeightTileSpeed.zw * half2(1,-2));
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				TANGENT_SPACE_ROTATION;
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));
				o.vertColor = v.color;

				o.projPos = ComputeScreenPos(o.pos);
				COMPUTE_EYEDEPTH(o.projPos.z);

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{

				half2 uv_offset = saturate(UVOrthographicWithOffset(i.posWorld, _gFowCamPos.xyz, _gFowCamPos.w, _gFowMask_TexelSize.xy, _TerrianSampleOffset.x, _TerrianSampleOffset.y));
				half2 uv = UVOrthographic(i.posWorld, _gFowCamPos.xyz, _gFowCamPos.w);
				half3 maskAlpha = tex2D(_gFowMask, uv_offset).rbg;


				half _LinearStep = lerp(1, _TerrianFogParam.z, maskAlpha.x * (1 - _TerrianFogParam.x));
				_LinearStep = lerp(_LinearStep, 1, _IsMenPai);
				_LinearStep = lerp(_LinearStep, 6, _IsQfz);

				float3 viewRay=normalize(-i.viewDir);
				viewRay.z=abs(viewRay.z)+0.2;
				viewRay.xy *= _Height;

				float linearStep = _LinearStep;
				float3 shadeP = half3(i.uv,0); 

				half noise = tex2D(_CloudMap, i.uv3).r;
				half4 T = tex2D(_CloudMap, i.uv2);
				half h2 = T.a * _HeightAmount * noise;


				float3 lioffset = viewRay / (viewRay.z * linearStep);
				half d = 1.0 - tex2Dlod(_CloudMap, half4(shadeP.xy,0,0)).a * h2;
				float3 prev_d = d;
				float3 prev_shadeP = shadeP;
				[unroll(5)]
				while(d > shadeP.z)
				{
					prev_shadeP = shadeP;
					shadeP += lioffset;
					prev_d = d;
					d = 1.0 - tex2Dlod(_CloudMap, half4(shadeP.xy,0,0)).a * h2;
				}
				float d1 = d - shadeP.z;
				float d2 = prev_d - prev_shadeP.z;
				float w = d1 / (d1 - d2);
				shadeP = lerp(shadeP, prev_shadeP, w);

				half4 c = tex2D(_CloudMap,shadeP.xy) * T * _CloudColor;

				half3 normal = normalize(i.normalDir);
				half3 lightDir1 = normalize(_FixedLightDir.xyz);
				half3 lightDir2 = UnityWorldSpaceLightDir(i.posWorld);
				half3 lightDir = lerp(lightDir2, lightDir1, _UseFixedLight);
				half NdotL = max(0,dot(normal,lightDir));
				half3 lightColor = _LightColor0.rgb;
				half3 finalColor = c.rgb * (NdotL*lightColor + 1.0) * _ColorMultiply;


				maskAlpha.y = lerp(maskAlpha.y, 0, _IsMenPai);
				_Alpha = lerp(_Alpha, 0.5, maskAlpha.y);
				half mask = lerp(maskAlpha.x, 1, maskAlpha.y);
				//mask = lerp(pow(mask, 2), mask, maskAlpha.z);

				//return half4(mask.xxx, 1);


				half TerrianAlpha = c.a * mask * lerp(Pow5(c.a), 1, mask) * _Alpha;
				half ZhenFaAlpha = _Alpha * c.a * i.vertColor.r;
				half alpha = lerp(TerrianAlpha, ZhenFaAlpha, _IsMenPai);
				// half2 uvScreen = half2(i.projPos.xy / i.projPos.w);
				// half alpha = tex2D(_gFowMask, uvScreen).r * _Alpha;

				#if _USE_SOFTPARTICLE
					alpha *= SoftParticle(i.projPos);
				#endif

				alpha = saturate(alpha);


				//无极缩放适配-UI层不显示
				alpha *= (1 - _TerrianFogParam.x);  

				return half4(finalColor.rgb,saturate(alpha));
			}
			ENDCG
		}
	}  

	FallBack "Diffuse"
}
