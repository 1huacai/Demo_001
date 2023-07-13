Shader "Hidden/Obsolete/BumpNoiseCloud"
{
	Properties
	{
		_Noise3D("3D Noise", 3D) = "white" {}
		_NoiseScale("3D Noise Scale", Range(0.0, 50.0)) = 1.0
		[HDR]_Color("Color", Color) = (1,1,1,1)
		_Speed("Speed 0", Vector) = (0, 0, 0, 0)
		_CutOff("Cut Off", Range(0.0, 1.0)) = 0.5
		_Expand("Expand", Range(0.0, 10.0)) = 1.0
		_SssStrength("SSS Strength", Range(0.0, 1.0)) = 1.0
		_DitherScale("Dither Scale", Range(0.0, 1.0)) = 1.0
	}
	SubShader
	{
		// Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			// ZWrite On
			// Cull Back
			// Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile __ _HEIGHTFOG
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "../Library/YoukiaEnvironment.cginc"
			#include "YoukiaObsolete.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half4 uv : TEXCOORD0;
				// float2 uv : TEXCOORD0;
				float4 pos_world : TEXCOORD1;
				float3 nor_world : TEXCOORD2;
				half4 heightfog : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler3D _Noise3D;
			half4 _NoiseScale;
			half4 _Speed;
			half _Step;
			half _Expand;
			half _CutOff;
			half _SssStrength;
			half _DitherScale;

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(half, _Steps)
				UNITY_DEFINE_INSTANCED_PROP(half, _AlphaClips)
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_SETUP_INSTANCE_ID(v);

				half step = UNITY_ACCESS_INSTANCED_PROP(Props, _Steps);

				o.nor_world = normalize(UnityObjectToWorldNormal(v.normal));
				float3 pos = v.vertex + v.normal * step * _Expand;
				o.vertex = UnityObjectToClipPos(pos);
				o.pos_world = mul(unity_ObjectToWorld, half4(pos, 1.0f));

				o.uv.xyz = v.vertex.xyz / _NoiseScale.rgb + _Time.x * _Speed.xyz;
				
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				// height fog
				#ifdef _HEIGHTFOG
					half fog = 0;
					o.heightfog.rgb = YoukiaHeightFog(o.pos_world, 0, fog);
					o.heightfog.a = fog;
				#endif

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return OBSOLETECOLOR;

				fixed noise = tex3D(_Noise3D, i.uv.xyz);
				fixed4 col = noise;
				
				UNITY_SETUP_INSTANCE_ID(i);
				half ac = UNITY_ACCESS_INSTANCED_PROP(Props, _AlphaClips);
				half alpha = 1 - (ac * _CutOff);
				half dither = frac((sin(i.pos_world.x + i.pos_world.y) * 99 + 11) * 99);
				ac += dither * _DitherScale;
				
				// alpha *= (noise * 0.5 + 0.5);

				clip(noise - (ac * _CutOff));

				// dir light
				half3 nor = normalize(i.nor_world);
				half3 dir = normalize(_WorldSpaceLightPos0);
				half NdotL = saturate(dot(nor, dir));
				half smoothNdotL = saturate(pow(NdotL, 2.0f - col.x));

				// sss
				half3 view_dir = normalize(_WorldSpaceCameraPos - i.pos_world);
				half3 back_dir = nor * (1.0f - _SssStrength) + dir;
				half sss = saturate(dot(view_dir, -back_dir));
				sss = saturate(pow(sss, 2 + col.x * 2) * 1.5f);

				// nv
				half NdotV = saturate(dot(nor, view_dir));
				half smoothNdotV = saturate(pow(NdotV, 2.0f - col.x));

				half final = saturate(smoothNdotV * 0.5f + saturate(smoothNdotL + sss) * (1.0f - NdotV * 0.5f));
				col.rgb = UNITY_LIGHTMODEL_AMBIENT.rgb * (1.0f - NdotV) * _Color.rgb;
				col.rgb += _LightColor0.xyz * _Color.rgb * final;
				col.a = pow(alpha, 1.5f);
				
				// height fog
				#ifdef _HEIGHTFOG
					col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
				#endif

				return col;
			}
			ENDCG
		}
	}
	// Fallback "Transparent/VertexLit"
	Fallback "Diffuse"
}
