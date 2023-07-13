// with scene effect Somker

Shader "YoukiaEngine/Effect/YoukiaSmokeFlow"
{
	Properties
	{
		_MainColor("MainColor", Color) = (1,1,1,1)	
		_MainTexture("MainTexture", 2D) = "white" {}
		_FlowTexture("FlowTexture", 2D) = "white" {}
		_Speed("Speed", Range(-5 , 5)) = 0.0

		[Header(Soft Particle)]
		[Toggle(_USE_SOFTPARTICLE)]_Toggle_USE_SOFTPARTICLE_ON("软粒子", float) = 0
		_SoftFade ("柔和度", Range(0.01, 3)) = 1.0

        [Space(30)]
		[Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
		_Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0       
		
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

		Stencil
		{
			Ref [_Ref]
			Comp [_Comp]
			Pass [_Pass]
		}

        Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
		ZWrite[_zWrite]
		ZTest[_zTest]
		Cull[_cull]	

		Pass
         {	
			CGPROGRAM				
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile __ _SKYENABLE
			#pragma multi_compile __ _UNITY_RENDER
			#pragma shader_feature _USE_SOFTPARTICLE
			#include "UnityCG.cginc"
			#include "../Library/Atmosphere.cginc"
			#include "../Library/YoukiaEffect.cginc"
			#include "../Library/YoukiaEnvironment.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID					
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half4 uv: TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID

				#ifdef _SKYENABLE
					float3 inscatter : TEXCOORD1;
					float3 extinction : TEXCOORD2;
				#endif

				#if (defined(_USE_SOFTPARTICLE))
					half4 projPos : TEXCOORD3;
				#endif
			};
		
			float4 _MainColor;
			sampler2D _FlowTexture;
			float4 _FlowTexture_ST;
		    sampler2D _MainTexture;
			float4 _MainTexture_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				
				UNITY_SETUP_INSTANCE_ID(v);				
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTexture);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, _FlowTexture) + _Time.x * half2(_Speed, 0);

				// atmosphere
				#ifdef _SKYENABLE
					float3 worldPos = mul(unity_ObjectToWorld, v.vertex);	
					float3 extinction = 0;
					o.inscatter = InScattering(_WorldSpaceCameraPos, worldPos, extinction);
					o.extinction = extinction;
				#endif

				#if _USE_SOFTPARTICLE
					o.projPos = ComputeScreenPos(o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
					
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( i );
               
				float MainTex = tex2D(_MainTexture, i.uv.xy).a;
				float FlowTex = tex2D(_FlowTexture, i.uv.zw).a;
				float BlendCor= lerp(0.0, FlowTex, MainTex);
				fixed4 col = (_MainColor * BlendCor);

				#if _USE_SOFTPARTICLE
					col.a *= SoftParticle(i.projPos);
				#endif
					
				#ifdef _SKYENABLE
					col.rgb = col.rgb * i.extinction + i.inscatter;
					col.rgb = saturate(col.rgb);
				#endif

				// 环境亮度
				col.rgb = EnvirLumChange(col.rgb);
				
				return col;
			}

			ENDCG 
		}
	}	
	
	FallBack "Transparent/VertexLit"	
	
}