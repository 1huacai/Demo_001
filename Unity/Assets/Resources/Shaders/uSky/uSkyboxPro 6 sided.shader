Shader "YoukiaEngine/uSkyPro/uSkyboxPro 6 sided" 
{
	Properties 
	{
		_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
		_Rotation ("Rotation", Range(0, 360)) = 0
		[NoScaleOffset] _FrontTex ("Front [+Z]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _BackTex ("Back [-Z]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _LeftTex ("Left [+X]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _RightTex ("Right [-X]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _UpTex ("Up [+Y]   (HDR)", 2D) = "grey" {}
		[NoScaleOffset] _DownTex ("Down [-Y]   (HDR)", 2D) = "grey" {}
		[Space(5)]
		[Header(Bloom)]
		[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
		_BloomThreshold("BloomThreshold", Range(0 , 10)) = 5
		_BloomRange("BloomRange", Range(0 , 20)) = 0

		[Space(5)]
		[MaterialToggle]_EnableAtomsphere("计算大气", float) = 1
		_Brightness("Brightness", Range(0, 1)) = 1

		[Enum(OFF,0,ON,1)] _uSkySkyboxOcean ("Skybox Ocean", int) = 0

		[Space(10)]
		[MaterialToggle]_CollectVariants("_CollectVariants(强行收集材质球)", float) = 0
	}
	
	SubShader 
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
		Cull Off ZWrite Off 
		// ColorMask RGB

		CGINCLUDE
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "../Library/Atmosphere.cginc"
			#include "../Library/YoukiaEnvironment.cginc"
			#include "../Library/YoukiaTools.cginc"
			#include "../Library/YoukiaMrt.cginc"

			// youkia height fog
            #pragma multi_compile __ _HEIGHTFOG
			#pragma multi_compile __ _SKYENABLE

			half4 _Tint;
			half _Exposure;
			half _Rotation;
			half _SkyboxLum;
			half _EnableAtomsphere;
			half _Brightness;

			half _BloomThreshold, _BloomRange;

			half _CollectVariants;

			struct appdata_t 
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float4 worldPosAndCamY : TEXCOORD1;
    			// float3 MiePhase_g : TEXCOORD2;
				#ifdef _HEIGHTFOG
					float4 worldPos : TEXCOORD3;
				#endif
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				float3 rotated = RotateAroundYInDegreesCube(v.vertex, _Rotation);
				o.vertex = UnityObjectToClipPos(rotated);
				o.texcoord = v.texcoord;
				#ifdef _HEIGHTFOG
					o.worldPos = mul(unity_ObjectToWorld, rotated);
				#endif

				o.worldPosAndCamY.xyz = mul((float3x3)unity_ObjectToWorld, rotated.xyz);
    			o.worldPosAndCamY.w = max(_WorldSpaceCameraPos.y * _uSkyAltitudeScale + _uSkyGroundOffset, 0.0);
    			
				// if (_SkyEnabled > 0 && _EnableAtomsphere > 0)
				// 	o.MiePhase_g =  PhaseFunctionG(_uSkyMieG, _uSkyMieScale);

				return o;
			}

			half4 skybox_frag (v2f i, sampler2D smp, half4 smpDecode)
			{
				half4 tex = tex2D(smp, i.texcoord);
				half3 c = DecodeHDR (tex, smpDecode);
				c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
				// c *= _Exposure;

				half lum = Luminance(c.rgb);
				half BloomRange = lerp(lum, _BloomRange, saturate(lum));
				c.rgb = _Exposure * c.rgb + BloomRange * _BloomThreshold * c.rgb;
				c.rgb = max(0, c.rgb);

				half3 col = c;
				#ifdef _SKYENABLE
					UNITY_BRANCH
					if (_EnableAtomsphere > 0)
					{
						half3 dir = normalize(i.worldPosAndCamY.xyz);
						// sun direction
						half nu = dot(dir, _SunDirSize.xyz); 
										
						half3 extinction = 0;
						// inscatter
						half3 inscatter = SkyRadiance(float3(0.0, i.worldPosAndCamY.w, 0.0), dir, extinction); 
						inscatter = min(inscatter, MAX_INSCATTER);
						// add sun disc
						half sun = pow(saturate(nu), 5000 / _SunDirSize.w);
					
						col = col + inscatter * _Brightness + sun * SUN_BRIGHTNESS * extinction * _LightColor0.rgb;
					}
				#endif

				#ifdef _HEIGHTFOG
					// height fog
                    half fog = 0;
                    half3 heightfog = YoukiaHeightFog(i.worldPos, 1, fog);
					col.rgb = lerp(col.rgb, heightfog.rgb, fog);
				#endif

				// 环境亮度
				col = EnvirLumChange(col);
				
				return half4(col, tex.a);
			}
		ENDCG
	
    	Pass 
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				sampler2D _FrontTex;
				half4 _FrontTex_HDR;
				FragmentOutput frag(v2f i)
				{ 
					return OutPutDefault(skybox_frag(i,_FrontTex, _FrontTex_HDR)); 
				}
			ENDCG
    	}
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				sampler2D _BackTex;
				half4 _BackTex_HDR;
				FragmentOutput frag(v2f i)
				{ 
					return OutPutDefault(skybox_frag(i,_BackTex, _BackTex_HDR)); 
				}
			ENDCG
		}
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				sampler2D _LeftTex;
				half4 _LeftTex_HDR;
				FragmentOutput frag(v2f i)
				{ 
					return OutPutDefault(skybox_frag(i,_LeftTex, _LeftTex_HDR));
				}
			ENDCG
		}
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				sampler2D _RightTex;
				half4 _RightTex_HDR;
				FragmentOutput frag(v2f i)
				{ 
					return OutPutDefault(skybox_frag(i,_RightTex, _RightTex_HDR));
				}
			ENDCG
		}
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				sampler2D _UpTex;
				half4 _UpTex_HDR;
				FragmentOutput frag(v2f i)
				{ 
					return OutPutDefault(skybox_frag(i,_UpTex, _UpTex_HDR)); 
				}
			ENDCG
		}
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				sampler2D _DownTex;
				half4 _DownTex_HDR;
				FragmentOutput frag(v2f i)
				{ 
					return OutPutDefault(skybox_frag(i,_DownTex, _DownTex_HDR)); 
				}
			ENDCG
		}
	}
	Fallback "YoukiaEngine/uSkyPro/uSkyboxPro"
}