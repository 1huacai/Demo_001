Shader "YoukiaEngine/Effect/YoukiaMarchingLine"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}

        [Header(UV speed)]
		_UVSpeedU ("纹理U方向速度", float) = 0
		_UVSpeedV ("纹理V方向速度", float) = 0

        [Header(UV Fade)]
        _UVFadePow ("UV fade power", Range(0, 1)) = 0.12

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
        LOD 100

        Pass
        {
            Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
			Lighting Off

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_instancing

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    half4 color : TEXCOORD1;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                UNITY_INSTANCING_BUFFER_START(Props)
                // color
                UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
                // st
                UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)

                UNITY_INSTANCING_BUFFER_END(Props)

                sampler2D _MainTex;

                half _UVSpeedU, _UVSpeedV;
                half _UVFade;
                half _UVFadePow;

                // half4 _Color;

                v2f vert (appdata v)
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    // o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                    float4 st = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_ST);
                    o.uv.xy = (v.uv.xy * st.xy + st.zw) + _Time.y * float2(_UVSpeedU, _UVSpeedV);
                    o.uv.zw = v.uv;

                    o.color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    
                    // sample the texture
                    float2 uv = i.uv.xy;
                    fixed4 col = tex2Dlod(_MainTex, float4(uv, 0, 0)) * i.color;

                    half uvFade = saturate(i.uv.w - _UVFade);
                    uvFade = pow(uvFade, _UVFadePow);
                    
                    col.a *= uvFade;

                    return col;
                }
            ENDCG
        }
    }
}
