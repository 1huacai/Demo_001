Shader "Hidden/ShaderReplace/YoukiaReflection"
{
	Properties 
	{
		_Color("Tint", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AlbedoTex("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
	}

	CGINCLUDE		

	// make fog work
	#include "UnityCG.cginc"
	
	struct v2f 
	{
		half4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
	};

	struct v2f_full
	{
		half4 pos : SV_POSITION;
		half4 uv : TEXCOORD0;
		half3 tsBase0 : TEXCOORD2;
		half3 tsBase1 : TEXCOORD3;
		half3 tsBase2 : TEXCOORD4;	
		half3 viewDirNotNormalized : TEXCOORD5;
	};

	struct v2f_worldPos
	{
		half4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		half3 worldPos : TEXCOORD1;
	};
		
	sampler2D _MainTex;
	sampler2D _Normal;
	sampler2D _AlbedoTex;
	half4 _Color;
							
	ENDCG 

	SubShader
	{
		Tags {"RenderType" = "Qpaque" "Reflection" = "RenderReflectionHair"}
		LOD 200
		Fog {Mode Off}
		Pass
		{
			CGPROGRAM

			half4 _MainTex_ST;		
					
			v2f vert (appdata_full v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o; 
			}		
			
			fixed4 frag (v2f i) : COLOR0 
			{
				fixed4 tex = tex2D (_AlbedoTex, i.uv) * _Color;
				tex.rgb = tex.rgb;
				return tex;		
			}	
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest	

			ENDCG
		}
	}


	SubShader 
	{
		Tags { "RenderType" = "Opaque" "Reflection" = "RenderReflectionOpaque" }
		LOD 200 
	    Fog { Mode Off }
		Pass 
		{
			CGPROGRAM

			half4 _MainTex_ST;		
					
			v2f vert (appdata_full v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o; 
			}		
			
			fixed4 frag (v2f i) : COLOR0 
			{
				fixed4 tex = tex2D (_MainTex, i.uv) * _Color;
				tex.rgb = tex.rgb;
				return tex;		
			}	
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
	
			ENDCG
		}
	} 

	SubShader {
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" "Reflection" = "RenderReflectionCutout" }
		
		LOD 200 
	    Fog { Mode Off }
		Pass 
		{
	        Cull Off
			
			CGPROGRAM

			half4 _MainTex_ST;	
			half _Cutoff;	
					
			v2f vert (appdata_full v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o; 
			}		
			
			fixed4 frag (v2f i) : COLOR0 
			{
				fixed4 tex = tex2D (_MainTex, i.uv);
				clip(tex.a - _Cutoff);
				return tex;		
			}	
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
		
			ENDCG
		}
	} 

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "Reflection" = "RenderReflectionTransparent"}
		
		LOD 200 
	    Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		
		Pass 
		{
	        Cull Off
			
			CGPROGRAM

			half4 _MainTex_ST;		
					
			v2f vert (appdata_full v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o; 
			}		
			
			fixed4 frag (v2f i) : COLOR0 
			{
				fixed4 tex = tex2D (_MainTex, i.uv);
				return tex;		
			}	
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
		
			ENDCG
		}
	} 

	FallBack Off
}
