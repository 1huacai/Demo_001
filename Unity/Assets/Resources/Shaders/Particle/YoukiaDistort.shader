Shader "YoukiaEngine/Particle/YoukiaDistort"
{
    Properties
    {
        [HDR]_Color ("颜色", Color) = (1, 1, 1, 1)
        _DistortTex ("扭曲纹理", 2D) = "white" {}
        _DistortTex_Speed_U ("U 方向速度", float) = 0
		_DistortTex_Speed_V ("V 方向速度", float) = 0
        _DistortTexStrengthAir ("空气扭曲强度", Range(0, 1)) = 0

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		// [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		// [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        // [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		// [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent-10" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        LOD 100

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha 
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
			#pragma fragmentoption ARB_precision_hint_fastest

            #include "../Library/ParticleLibrary.cginc"
            
            struct appdataDistort
            {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                fixed4 color : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2fDistort
            {
                float4 vertex : SV_POSITION;
                // xy: uv, z: distort strngth, w: alpha
                half4 uv : TEXCOORD0;
                half4 projPos : TEXCOORD2;
                fixed4 color : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2fDistort vertDistort (appdataDistort v)
            {
                v2fDistort o;
                UNITY_INITIALIZE_OUTPUT(v2fDistort, o);
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);

                half2 uvDistortSpeed = UVSpeed(_DistortTex_Speed_U, _DistortTex_Speed_V);
                o.uv.xy = TRANSFORM_TEX(v.uv, _DistortTex) + uvDistortSpeed;
                o.uv.z = _DistortTexStrengthAir * _Color.a * o.color.a;
                o.uv.w = _Color.a * o.color.a;

                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);

                o.color = v.color;

                return o;
            }

            fixed4 fragDistort (v2fDistort i) : SV_Target
            {
                // sample the texture
                half4 distortTex = tex2D(_DistortTex, i.uv);
                half distort = distortTex.a * distortTex.r;

                fixed2 uvGrab = fixed2(i.projPos.xy / i.projPos.w) + distort * i.uv.z;
                fixed3 colGrab = tex2D(_gGrabTex, uvGrab).rgb;
                
                half4 color = half4(colGrab.rgb, i.uv.w * distortTex.a);
                
                return color;
            }
            ENDCG
        }
    }
}
