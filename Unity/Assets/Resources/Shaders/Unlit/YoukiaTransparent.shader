Shader "YoukiaEngine/Unlit/Transparent" 
{
    Properties 
    {
        [HDR]_Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

        [Space(5)]
		[MaterialToggle]_EnableAtomsphere("计算大气", float) = 0

        [Space(5)]
        [MaterialToggle]_ReceiveColorGrading("接受后处理校色", float) = 0

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
        _Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 1
    }

    SubShader 
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

        Stencil
        {
            Ref [_Ref]
            Comp [_Comp]
            Pass [_Pass]
        }


        Blend[_SrcBlend][_DstBlend]
        ZWrite[_zWrite]
        ZTest[_zTest]
        Cull[_cull]
        // ColorMask RGB

        Pass 
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #pragma multi_compile_instancing
                #pragma multi_compile __ _SKYENABLE

                #include "UnityCG.cginc"
                #include "../Library/YoukiaCommon.cginc"
                #include "../Library/YoukiaMrt.cginc"
                #include "../Library/Atmosphere.cginc"
                #include "../Library/YoukiaEnvironment.cginc"

                struct appdata_t 
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f 
                {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO

                    float3 inscatter : TEXCOORD11;
                    float3 extinction : TEXCOORD12;
                };
                
                half _EnableAtomsphere;
                half _ReceiveColorGrading;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                    // atmosphere
                    #ifdef _SKYENABLE
                        if (_EnableAtomsphere > 0)
                        {
                            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);	
                            float3 extinction = 0;
                            // float3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
                            o.inscatter = InScattering(_WorldSpaceCameraPos, worldPos, extinction);
                            o.extinction = extinction;
                        }
                    #endif

                    return o;
                }

                FragmentOutput frag (v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;

                    #ifdef _SKYENABLE
                        if (_EnableAtomsphere > 0)
                        {
                            col.rgb = col.rgb * i.extinction + i.inscatter;
                        }
                    #endif

                    // 环境亮度
				    col.rgb = EnvirLumChange(col.rgb);

                    return OutPutCustom(col, (1 - _ReceiveColorGrading) * col.a);
                }
            ENDCG
        }
    }
}
