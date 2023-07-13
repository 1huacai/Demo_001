Shader "YoukiaEngine/Effect/ReflectPlane"
{
	Properties
	{
		[Header(Ref)]
		_RefColor ("反射颜色", Color) = (1, 1, 1, 1)
		_RefFactor ("反射强度", Range(0, 2)) = 0.5

		[Header(Distortion)]
		_BumpNormal("凹凸法线", 2D) = "bump" {}
		_NormalStrength("法线强度", Range(0.01, 0.2)) = 0.05
		//_Distort("扭曲", Range(0, 0.5)) = 0.25

		[Header(Others)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10

	}
	SubShader
	{
		Tags { "Queue"="Transparent-1" "RenderType"="Transparent" }

		Pass
		{
            Tags { "LightMode"="ForwardBase" }


            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
				#pragma multi_compile_fwdbase nolightmap
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_instancing

				#pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            	#pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK

				#pragma vertex vert
				#pragma fragment frag
				

				#include "UnityCG.cginc"
				#include "../Library/YoukiaTools.cginc"
				#include "../Library/YoukiaLight.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 uv : TEXCOORD0;
					float4 tangent : TANGENT;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float4 worldPos : TEXCOORD0;
					float4 proj : TEXCOORD2;
					float2 uv : TEXCOORD3;
					half3 TtoW[3] : TEXCOORD4;
				};

				half4 _RefColor;
				half _RefFactor;
				//half _Distort;
				sampler2D _BumpNormal;
				sampler2D _gReflectTex;

				v2f vert (appdata v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f, o);

					o.pos = UnityObjectToClipPos(v.vertex, o.worldPos);
					o.uv = v.uv;

					half3 worldNormal = 0;
					T2W(v.normal, v.tangent, o.TtoW, worldNormal);

					o.proj = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.proj.z);

					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					float3 worldPos = i.worldPos;

					half4 bump = tex2D(_BumpNormal, i.uv);
					half3 normal = UnpackNormalYoukia(bump, _NormalStrength);
					normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));
					
					half3 mirror = 1;
					half3 refCol = _RefFactor * _RefColor.rgb;
					mirror = refCol; 
					half2 uvDistort = normal.xz;

					fixed2 uvMirror = fixed2((i.proj.xy + uvDistort) / i.proj.w);
					mirror = saturate(tex2D(_gReflectTex, uvMirror)).rgb;
					mirror.rgb *= refCol;

					return half4(mirror, _RefColor.a);
				}
			ENDCG
		}
	}
}
