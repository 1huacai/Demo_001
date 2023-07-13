Shader "Hidden/ShaderReplace/YoukiaTerrainSurface"
{
	Properties 
	{

	}

	CGINCLUDE		

		#include "UnityCG.cginc"
		
		struct v2f 
		{
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			fixed4 uv_0 : TEXCOORD6;
            fixed4 uv_1 : TEXCOORD7;
		};
		
		sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
		half4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

		sampler2D _Control;
        half4 _Control_ST;
								
	ENDCG 

	SubShader
	{
		Tags {"RenderType" = "Qpaque" "ShadowType" = "ST_Terrain"}
		// LOD 200
		// Fog {Mode Off}
		Pass
		{
			CGPROGRAM

			half4 _MainTex_ST;		
					
			v2f vert (appdata_full v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				o.uv = TRANSFORM_TEX(v.texcoord, _Control);
				o.uv_0.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
				o.uv_0.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
				o.uv_1.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
				o.uv_1.zw = TRANSFORM_TEX(v.texcoord, _Splat3);

				return o; 
			}		
			
			fixed4 frag (v2f i) : SV_Target 
			{
				fixed4 blendmask = tex2D (_Control, i.uv);

				half4 lay0 = tex2D(_Splat0, i.uv_0.xy) * blendmask.r;
				half4 lay1 = tex2D(_Splat1, i.uv_0.zw) * blendmask.g;
				half4 lay2 = tex2D(_Splat2, i.uv_1.xy) * blendmask.b;
				half4 lay3 = tex2D(_Splat3, i.uv_1.zw) * blendmask.a;

				return lay0 + lay1 + lay2 + lay3;		
			}	
			
			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest	

			ENDCG
		}
	}

	FallBack Off
}
