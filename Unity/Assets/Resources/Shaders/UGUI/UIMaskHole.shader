// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/UIMaskHole" {
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Color(RGB)",Color) = (0,0,0,1)
		_MainTex2 ("Base2 (RGB) Trans (A)", 2D) = "white" {}

		_Alpha("Alpha", Range(0,1)) = 0.5 
		_OffsetX ("OffsetX", Range(-10,4)) = 0  
		_OffsetY("OffsetY",Range(-10,4)) =0

		_ScaleX("scaleX", Range(0,10)) = 0  
		_ScaleY("scaleY", Range(0,10)) = 0 



		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				half2 texcoord  : TEXCOORD0;
				//half2 texcoord1 : TEXCOORD1;
			};
			
			
			//sampler2D _MainTex;
			//float4 _MainTex_ST;
			sampler2D _MainTex2;
			float4 _MainTex2_ST;

			float4 _Color;

			float _Alpha;
			float _OffsetX;
			float _OffsetY;

			float _ScaleX;
			float _ScaleY;


			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				//OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex2);
				OUT.texcoord[0] = (OUT.texcoord[0] - 0.5-_OffsetX)/_ScaleX + 0.5;
				OUT.texcoord[1] = (OUT.texcoord[1]- 0.5-_OffsetY)/_ScaleY + 0.5;
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif
				return OUT;
			}


			fixed4 frag(v2f IN) : SV_Target
			{

				fixed4 col = _Color;//tex2D(_MainTex, IN.texcoord);
				fixed4 col2 = tex2D(_MainTex2, IN.texcoord);
				
				col.a = 1 -col2.a;
				col.a = col.a*_Alpha;
				return col;
			}
		ENDCG
		}
	}
}
