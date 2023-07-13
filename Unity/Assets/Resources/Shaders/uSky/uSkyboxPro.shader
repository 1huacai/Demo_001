Shader "YoukiaEngine/uSkyPro/uSkyboxPro" 
{
	Properties 
	{
		[Enum(OFF,0,ON,1)] _uSkySkyboxOcean ("Skybox Ocean", int) = 0
		_Brightness("Brightness", Range(0, 1)) = 1
	}
	
	SubShader 
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
		Cull Off ZWrite Off 
		// ColorMask RGB
	
    	Pass 
    	{	
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "../Library/Atmosphere.cginc"
			#include "../Library/YoukiaEnvironment.cginc"
			#include "../Library/YoukiaMrt.cginc"

			// youkia height fog
            #pragma multi_compile __ _HEIGHTFOG
			#pragma multi_compile __ _SKYENABLE

			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float4 worldPosAndCamY : TEXCOORD0;
    			// float3 MiePhase_g : TEXCOORD1;
				#ifdef _HEIGHTFOG
					float4 worldPos : TEXCOORD3;
				#endif
				// float3 extinction : TEXCOORD5;
				// float3 inscatter : TEXCOORD6;
			};

			half _SkyboxLum;
			half _Brightness;

			v2f vert(appdata_base v)
			{
    			v2f o;
    			o.pos = UnityObjectToClipPos(v.vertex);
				#ifdef _HEIGHTFOG
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				#endif
    			o.worldPosAndCamY.xyz = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
    			o.worldPosAndCamY.w = max(_WorldSpaceCameraPos.y * _uSkyAltitudeScale + _uSkyGroundOffset, 0.0);
    			
				// o.MiePhase_g =  PhaseFunctionG(_uSkyMieG, _uSkyMieScale);

    			return o;
			}
			
			FragmentOutput frag(v2f i)
			{
			    float3 dir = normalize(i.worldPosAndCamY.xyz);
				
				// sun direction
			    float nu = dot(dir, _SunDirSize.xyz); 
				// add sun disc
				float sun = pow(saturate(nu), 5000 / _SunDirSize.w);
			    				
				// inscatter
				#if _SKYENABLE
					half3 extinction = 0;
					half3 col = SkyRadiance(float3(0.0, i.worldPosAndCamY.w, 0.0), dir, extinction) * _Brightness; 
					col = min(col, MAX_INSCATTER);
					col += sun * SUN_BRIGHTNESS * extinction * _LightColor0.rgb;
				#else
					half3 col = sun * SUN_BRIGHTNESS;
				#endif

				#ifdef _HEIGHTFOG
					// height fog
                    half fog = 0;
                    half3 heightfog = YoukiaHeightFog(i.worldPos, 1, fog);
					col.rgb = lerp(col.rgb, heightfog.rgb, fog);
				#endif
				
				// 环境亮度
				col = EnvirLumChange(col);
				
				return OutPutDefault(half4(col , 1));
			}			
			ENDCG
    	}

	}
	// Fallback "uSkyPro/Skybox Mobile SM2 (Altitude Sample X1 only)"
	FallBack "Skybox/Cubemap"
}