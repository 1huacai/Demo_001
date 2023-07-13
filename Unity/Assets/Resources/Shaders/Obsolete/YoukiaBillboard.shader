Shader "YoukiaEngine/Obsolete/YoukiaBillboard"
{
    Properties
    {
        [HDR]_Color ("颜色", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        
        [Enum(None, 0, default, 1, X, 2, Y, 3, Z, 4)] _BillboardType ("Billboard Type", Int) = 0

        [Header(UV speed)]
		_UV_Speed_U_Main ("纹理U方向速度", float) = 0
		_UV_Speed_V_Main ("纹理V方向速度", float) = 0

        [Header(Distort)]
		// [Toggle(_USE_DISTORT)]_Toggle_USE_DISTORT_ON("扭曲", float) = 0
		_DistortTex ("扭曲纹理", 2D) = "whiter" {}
		_DistortStrength ("扭曲强度", Range(0, 5)) = 0
		_DistortTex_Speed_U ("U 方向扭曲速度", float) = 0
		_DistortTex_Speed_V ("V 方向扭曲速度", float) = 0

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

        CGINCLUDE
            #include "UnityCG.cginc"
            #include "../Library/YoukiaTools.cginc"
            #include "YoukiaObsolete.cginc"

            half4 _Color;
            half _BillboardType;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // uv speed
            half _UV_Speed_U_Main, _UV_Speed_V_Main;

            // distort
            sampler2D _DistortTex;
            float4 _DistortTex_ST;
            half _DistortStrength;
            half _DistortTex_Speed_U, _DistortTex_Speed_V;

            // bloom
            // half _Threshold_Particle;

            struct appdata
            {
                fixed4 color : COLOR;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                fixed4 color : COLOR;
                float4 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                
                o.vertex = BillboardVectex(v.vertex, _BillboardType);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _DistortTex);
                o.color = v.color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return OBSOLETECOLOR;

                half4 color = _Color * i.color;

                #if _USE_DISTORT
                    half2 uvspeed = half2(_Time.y * half2(_UV_Speed_U_Main, _UV_Speed_V_Main));
                    half2 uvDistort = half2(_Time.y * half2(_DistortTex_Speed_U, _DistortTex_Speed_V));
                    half4 distortTex = tex2D(_DistortTex, i.uv.zw + uvDistort);
                    uvspeed += distortTex.a * distortTex.r * _DistortStrength; 
                #else
                    half2 uvspeed = half2(_Time.y * half2(_UV_Speed_U_Main, _UV_Speed_V_Main));
                #endif

                fixed4 col = tex2D(_MainTex, i.uv + uvspeed) * color;

                return col;
            }

            fixed4 fragAlpha (v2f i) : SV_Target
            {
                return OBSOLETECOLOR;
                
                half4 color = _Color * i.color;

                #if _USE_DISTORT
                    half2 uvspeed = half2(_Time.y * half2(_UV_Speed_U_Main, _UV_Speed_V_Main));
                    half2 uvDistort = half2(_Time.y * half2(_DistortTex_Speed_U, _DistortTex_Speed_V));
                    half4 distortTex = tex2D(_DistortTex, i.uv.zw + uvDistort);
                    uvspeed += distortTex.a * distortTex.r * _DistortStrength; 
                #else
                    half2 uvspeed = half2(_Time.y * half2(_UV_Speed_U_Main, _UV_Speed_V_Main));
                #endif

                fixed4 col = tex2D(_MainTex, i.uv + uvspeed) * color;
                // col.a *= saturate(1 - _Threshold_Particle);

                return col;
            }

        ENDCG

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
			
			// ColorMask RGB
			Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
			Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles

            #pragma shader_feature _USE_DISTORT

            ENDCG
        }

        // Pass
        // {
        //     Tags { "LightMode"="ForwardBase" }
			
		// 	ColorMask A
		// 	Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
		// 	ZWrite[_zWrite]
		// 	ZTest[_zTest]
		// 	Cull[_cull]
		// 	Lighting Off

        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment fragAlpha
        //     #pragma multi_compile_fog
        //     #pragma multi_compile_particles

        //     #pragma shader_feature _USE_DISTORT

        //     ENDCG
        // }
    }
    CustomEditor "YoukiaBillboardInspector"
    FallBack "Transparent/VertexLit"
}
