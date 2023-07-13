Shader "TSW/ParallaxCloud" 
{
	Properties {

		_LinearStep("LoopAmount", range(1, 16)) = 16 
		_CloudColor("Color",Color) = (1,1,1,1)
		_CloudMap("MainTex",2D)="white"{}
		_Alpha("Alpha", Range(0,1)) = 0.5
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
	}
	SubShader 
	{	
		Tags 
		{
			"IgnoreProjector"="True"
			"Queue"="Transparent"
			"RenderType"="Transparent"
			//"RenderType"="Opaque"
		}

		Pass
		{
			Name "FORWARD"
			Tags 
			{
				"LightMode"="ForwardBase"
			}
			Blend SrcAlpha OneMinusSrcAlpha
			//ZWrite Off
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_fwdbase

			#pragma shader_feature _USE_SOFTPARTICLE

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

			half _LinearStep;
			half _NoiseScale;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half3 normalDir : TEXCOORD1;
				half3 viewDir : TEXCOORD2;
				float3 posWorld : TEXCOORD3;
				half2 uv2 : TEXCOORD4;
				half2 uv3 : TEXCOORD5;
				#if (defined(_USE_SOFTPARTICLE))
					half4 projPos : TEXCOORD6;
				#endif
			};

			v2f vert (appdata_full v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord,_CloudMap) + frac(_Time.y*_HeightTileSpeed.zw);
				o.uv2 = v.texcoord * _HeightTileSpeed.xy;
				o.uv3 = v.texcoord * _NoiseScale - frac(_Time.y*_HeightTileSpeed.zw * half2(1,-2));
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				TANGENT_SPACE_ROTATION;
				o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex));

				#if _USE_SOFTPARTICLE
					o.projPos = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.projPos.z);
				#endif

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float3 viewRay=normalize(i.viewDir * -1);
				viewRay.z=abs(viewRay.z)+0.2;
				viewRay.xy *= _Height;

				float3 shadeP = half3(i.uv,0);

				half noise = tex2D(_CloudMap, i.uv3).r;
				half4 T = tex2D(_CloudMap, i.uv2);
				half h2 = T.a * _HeightAmount * noise;

				half3 lioffset = viewRay / (viewRay.z * _LinearStep);
				half d = 1.0 - tex2Dlod(_CloudMap, half4(shadeP.xy,0,0)).a * h2;
				half3 prev_d = d;
				half3 prev_shadeP = shadeP;
				while(d > shadeP.z)
				{
					prev_shadeP = shadeP;
					shadeP += lioffset;
					prev_d = d;
					d = 1.0 - tex2Dlod(_CloudMap, half4(shadeP.xy,0,0)).a * h2;
				}
				half d1 = d - shadeP.z;
				half d2 = prev_d - prev_shadeP.z;
				half w = d1 / (d1 - d2);
				shadeP = lerp(shadeP, prev_shadeP, w);

				half4 c = tex2D(_CloudMap,shadeP.xy) * T * _CloudColor;
				// half Alpha = lerp(c.a, 1.0, _Alpha);

				half3 normal = normalize(i.normalDir);
				half3 lightDir1 = normalize(_FixedLightDir.xyz);
				half3 lightDir2 = UnityWorldSpaceLightDir(i.posWorld);
				half3 lightDir = lerp(lightDir2, lightDir1, _UseFixedLight);
				half NdotL = max(0,dot(normal,lightDir));
				half3 lightColor = _LightColor0.rgb;
				half3 finalColor = c.rgb*(NdotL*lightColor + 1.0);

				half2 uv = UVOrthographic(i.posWorld, _gFowCamPos.xyz, _gFowCamPos.w);
				_Alpha *= tex2D(_gFowMask, uv).r;

				#if _USE_SOFTPARTICLE
					_Alpha *= SoftParticle(i.projPos);
				#endif


				return half4(finalColor.rgb,_Alpha);
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}
