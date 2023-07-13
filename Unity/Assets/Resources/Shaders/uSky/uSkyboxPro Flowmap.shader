Shader "YoukiaEngine/uSkyPro/uSkyboxPro Flowmap" 
{
	Properties 
	{
		_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
		_Rotation ("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _MainTex ("Spherical  (HDR)", 2D) = "grey" {}
		_Height ("height", Range(-10, 10)) = 0

		[Space(5)]
		[Header(FlowMap)]
		_MaskTex("MaskTex", 2D) = "white" {}
		[KeywordEnum(None, F 1, F 4)] _Flowmap("flowmap", Float) = 0
		_Speed("流动速度", Range(-1, 1)) = 0.1
		[Space(5)]
		[Header(Bloom)]
		[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
		_BloomThreshold("BloomThreshold", Range(0 , 10)) = 5
		_BloomRange("BloomRange", Range(0 , 20)) = 0

		[Space(5)]
		[MaterialToggle]_EnableAtomsphere("计算大气", float) = 1
		_Brightness("Brightness", Range(0, 1)) = 1
		[MaterialToggle]_EnableAtomsphereFade("大气高度衰减", float) = 0
		_AltitudeScale("AltitudeScale(海拔缩放)", Range(0.000001, 0.001)) = 1
		_GroundOffset("GroundOffset(海拔偏移)", Range(0, 100000)) = 0

		[Space(5)]
		[Header(Ocean)]
		[HideInInspector][Enum(OFF,0,ON,1)] _uSkySkyboxOcean ("Skybox Ocean", int) = 0

		[Space(20)]
		[Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		_Ref("Stencil Reference", Range(0,255)) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_Comp("Stencil Compare", Float) = 3
		[Enum(UnityEngine.Rendering.StencilOp)]_Pass("Stencil Pass", Float) = 0
	}
	
	SubShader 
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
		Cull back 
		ZWrite[_zWrite]
		ZTest[_zTest]
		// ColorMask RGB
	
		Stencil
		{
			Ref [_Ref]
			Comp [_Comp]
			Pass [_Pass]
		}

    	Pass 
    	{	
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "../Library/Atmosphere.cginc"
			#include "../Library/YoukiaTools.cginc"
			#include "../Library/YoukiaEffect.cginc"
			#include "../Library/YoukiaEnvironment.cginc"

			#pragma multi_compile _FLOWMAP_NONE _FLOWMAP_F_1 _FLOWMAP_F_4
			// youkia height fog
            #pragma multi_compile __ _HEIGHTFOG
			#pragma multi_compile __ _SKYENABLE
			// #pragma shader_feature _FLOWMAP

			struct v2f 
			{
    			float4 pos : SV_POSITION;
				half3 texcoord : TEXCOORD0;
    			float4 worldPosAndCamY : TEXCOORD1;
    			// float3 MiePhase_g : TEXCOORD2;
				#if defined(_HEIGHTFOG) || defined(_SKYENABLE)
					float4 worldPos : TEXCOORD3;
				#endif

				float2 image180ScaleAndCutoff : TEXCOORD4;
            	float4 layout3DScaleAndOffset : TEXCOORD5;
			};

			half4 _MainTex_HDR;
			half4 _Tint;
			half _Exposure;
			half _Rotation;

			half _SkyboxLum;
			half _Height;
			half _BloomThreshold, _BloomRange;

			half _EnableAtomsphere;
			half _Brightness;
			half _EnableAtomsphereFade;
			float _AltitudeScale;
			float _GroundOffset;

			v2f vert(appdata_base v)
			{
    			v2f o;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				float3 rotated = RotateAroundYInDegreesCube(v.vertex, _Rotation);
    			o.pos = UnityObjectToClipPos(rotated);
				o.texcoord = v.vertex.xyz;
				#if defined(_HEIGHTFOG) || defined(_SKYENABLE)
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				#endif

				o.image180ScaleAndCutoff = half2(1.0, 1.0);
				o.layout3DScaleAndOffset = half4(0, 0, 1, 1);

    			o.worldPosAndCamY.xyz = mul((float3x3)unity_ObjectToWorld, rotated.xyz);
    			o.worldPosAndCamY.w = max(_WorldSpaceCameraPos.y * _uSkyAltitudeScale + _uSkyGroundOffset, 0.0);
    			
				// if (_SkyEnabled > 0 && _EnableAtomsphere > 0)
				// 	o.MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);

    			return o;
			}
			
			FragmentOutput frag(v2f i)
			{
				i.texcoord.y += _Height;
				float2 tc = ToRadialCoords(i.texcoord);
				UNITY_BRANCH
				if (tc.x > i.image180ScaleAndCutoff[1])
					return OutPutDefault(half4(0, 0, 0, 1));
				tc.x = fmod(tc.x*i.image180ScaleAndCutoff[0], 1);
				tc = (tc + i.layout3DScaleAndOffset.xy) * i.layout3DScaleAndOffset.zw;
				
				if (tc.y > 0.5)
					tc.y = tc.y * 2 - 1;
				else
					tc.y = -(tc.y * 2 - 1);

				half3 c = Flowmap(_MainTex, _MainTex_HDR, tc);

				c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;

				half lum = Luminance(c.rgb);
				half BloomRange = lerp(lum, _BloomRange, saturate(lum));
				c.rgb = _Exposure * c.rgb + BloomRange * _BloomThreshold * c.rgb;
				c.rgb = max(0, c.rgb);

				half3 col = c.rgb;
				#ifdef _SKYENABLE
					UNITY_BRANCH
					if (_EnableAtomsphere > 0)
					{
						half3 dir = normalize(i.worldPosAndCamY.xyz);
				
				// sun direction
						float nu = dot(dir, _SunDirSize.xyz); 
										
						half3 extinction = 0;
						// inscatter
						half3 inscatter = SkyRadiance(float3(0.0, i.worldPosAndCamY.w, 0.0), dir, extinction);
						inscatter = min(inscatter, MAX_INSCATTER);
						// return half4(inscatter, 1);
						// col *= SkyRadiance(float3(0.0, i.worldPosAndCamY.w, 0.0), dir, extinction);
						// half3 col = i.inscatter;
						// add sun disc
						float sun = pow(saturate(nu), 5000 / _SunDirSize.w);
						
						half3 atmosphere = inscatter * _Brightness + sun * SUN_BRIGHTNESS * extinction * _LightColor0.rgb;
						
						float height = max(0, i.worldPos.y - _GroundOffset);
						half fade = exp(-height * _AltitudeScale);
						
						col = lerp(col + atmosphere, lerp(col, atmosphere, fade), _EnableAtomsphereFade);
					}
				#endif

				#ifdef _HEIGHTFOG
					// height fog-
                    half fog = 0;
                    half3 heightfog = YoukiaHeightFog(i.worldPos, 1, fog);
					col.rgb = lerp(col.rgb, heightfog.rgb, fog);
				#endif

				// 环境亮度
				col = EnvirLumChange(col);

				return OutPutDefault(half4(col, 1 - step(0.1 ,pow(lum, 2))));
			}			
			ENDCG
    	}

	}
	Fallback "YoukiaEngine/uSkyPro/uSkyboxPro"
}