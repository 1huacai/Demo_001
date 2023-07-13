//@@@DynamicShaderInfoStart
//此扭曲Shader虽然效果相对较好，不过开销较大，不建议在游戏中使用。在游戏中建议使用YoukiaEngine/Particle/YoukiaDistort。虽然有表现上的瑕疵，不过性能可控。
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Particle/YoukiaDistort-Grabpass"
{
    Properties
    {   
        [HDR]_Color ("颜色", Color) = (1, 1, 1, 1)
        _DistortTex ("扭曲纹理", 2D) = "white" {}
        _DistortMask ("扭曲mask(控制扭曲范围: alpha 通道)", 2D) = "white" {}
        [Header(Speed)]
        _DistortTex_Speed_U ("U 方向速度", float) = 0
		_DistortTex_Speed_V ("V 方向速度", float) = 0
        _DistortTexStrengthAir ("空气扭曲强度", Range(0, 1)) = 0

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        LOD 100

        GrabPass{ }

        Pass
        {
            // ColorMask RGB
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
			Lighting Off
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vertDistort
            #pragma fragment fragDistort
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            #include "../Library/ParticleLibrary.cginc"
            
            struct appdataDistort
            {
                float4 vertex : POSITION;
                fixed2 uv : TEXCOORD0;
                fixed4 color : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2fDistort
            {
                float4 vertex : SV_POSITION;
                half4 uv : TEXCOORD0;
                half4 projPos : TEXCOORD2;
                fixed4 color : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            sampler2D _DistortMask;
            half4 _DistortMask_ST;
            sampler2D _GrabTexture;

            v2fDistort vertDistort (appdataDistort v)
            {
                v2fDistort o;
                UNITY_INITIALIZE_OUTPUT(v2fDistort, o);
                UNITY_SETUP_INSTANCE_ID(v);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _DistortMask);
                half2 uvDistortSpeed = UVSpeed(_DistortTex_Speed_U, _DistortTex_Speed_V);
                o.uv.zw = TRANSFORM_TEX(v.uv, _DistortTex) + uvDistortSpeed;

                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);

                o.color = v.color;

                return o;
            }

            fixed4 fragDistort (v2fDistort i) : SV_Target
            {
                // sample the texture
                half mask = tex2D(_DistortMask, i.uv.xy).a;
                half4 distortTex = tex2D(_DistortTex, i.uv.zw);
                half distort = distortTex.a * distortTex.r;

                fixed2 uvGrab = fixed2(i.projPos.xy / i.projPos.w) + distort * _DistortTexStrengthAir * _Color.a * i.color.a * mask;
                fixed3 colGrab = tex2D(_GrabTexture, uvGrab).rgb;
                
                half4 color = half4(colGrab.rgb, _Color.a * i.color.a * mask);
                
                return color;
            }
            ENDCG
        }
    }
}
