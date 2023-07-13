Shader "YoukiaEngine/Unlit/YoukiaUnlitTexture"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff ("Cut off", Range(0, 1)) = 0

        [Space(20)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
        _Ref ("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp ("Stencil Compare", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil Pass", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque"}
        LOD 100

        Stencil
        {
            Ref [_Ref]
            Comp [_Comp]
            Pass [_Pass]
        }

        Pass
        {
            ZWrite[_zWrite]
			ZTest[_zTest]

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _Cutoff;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                clip(col.a - _Cutoff);

                return col;
            }
            ENDCG
        }
    }
}
