// 场景阴影,不渲染模型,通过通道显示阴影形状

Shader "YoukiaEngine/Shadow/ShadowCaster"
 {
	Properties 
	{
		_MainTex ("Main Tex", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1.1)) = 0.5
		[Header(Wind)]
        _WindAtten("全局风力衰减", Range(0, 1)) = 0

		// [Header(Wind)]
        // [Toggle(_WIND)]_Toggle_WIND_ON("风", Float) = 0
        // [MaterialToggle]_WindDetail("风(Detail)", Float) = 0
        // _WindStrength ("风强度", Range(0, 1)) = 1
        // _WindDirX ("X 方向风强度", Range(0, 5)) = 1
        // _WindDirY ("Y 方向风强度", Range(0, 5)) = 1
        // _WindDirZ ("Z 方向风强度", Range(0, 5)) = 1
	}
	
	SubShader 
	{
		Tags {"Queue"="Geometry" "RenderType"="Opaque" "ShadowType" = "ST_ShadowCaster" }

		Pass 
		{	
			ZWrite On
			ZTest LEqual
			Cull back
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// #pragma multi_compile_shadowcaster
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"
				#include "../Library/YoukiaEnvironment.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					half3 color : COLOR;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_OUTPUT_STEREO
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};
			
				v2f vert (appdata_t v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord);
					clip(col.r - saturate(_Cutoff));
					return col;
				}
				
			ENDCG
    	}

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_shadowcaster
				#pragma multi_compile_instancing
				#include "UnityCG.cginc"
				#include "../Library/YoukiaEnvironment.cginc"

				#pragma multi_compile __ _GWIND

				struct appdata_shadow 
				{
					half4 vertex : POSITION;
					half2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					half3 color : COLOR;
				};

				struct v2f 
				{
					V2F_SHADOW_CASTER;
					UNITY_VERTEX_OUTPUT_STEREO
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				v2f vert( appdata_shadow v )
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    UNITY_SETUP_INSTANCE_ID(v);

					TRANSFER_SHADOW_CASTER(o)

					o.texcoord = v.texcoord;
					return o;
				}

				float4 frag( v2f i ) : SV_Target
				{
					half4 color = tex2D(_MainTex, TRANSFORM_TEX(i.texcoord, _MainTex));
					clip(color.r - _Cutoff);

					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
	} 
}
