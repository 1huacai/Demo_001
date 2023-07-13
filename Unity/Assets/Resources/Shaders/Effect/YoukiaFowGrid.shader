//@@@DynamicShaderInfoStart
//视野迷雾格子
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Effect/YoukiaFowGrid"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _NumberTex ("Number Texture", 2D) = "white" {}
        _FowNumber ("Number", float) = 0
        _FowParam("", vector) = (0, 0, 0, 0)
        _FowAlpha("", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #define COUNT 7

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                half2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 param :TEXCOORD1;
                half alpha : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            half4 _Color;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _NumberTex;
            float4 _NumberTex_ST;
            half _Number;

            UNITY_INSTANCING_BUFFER_START(NumberProps)

            // UNITY_DEFINE_INSTANCED_PROP(half, _FowNumber)
            // UNITY_DEFINE_INSTANCED_PROP(half, _FowDisappear)
            // x: number, y: time stamp, z: disappear
            UNITY_DEFINE_INSTANCED_PROP(float4, _FowParam_Array)
            UNITY_DEFINE_INSTANCED_PROP(half, _FowAlpha_Array)

            UNITY_INSTANCING_BUFFER_END(NumberProps)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                // x: number, y: time stamp, z: disappear
                o.param = UNITY_ACCESS_INSTANCED_PROP(NumberProps, _FowParam_Array);
                o.alpha = UNITY_ACCESS_INSTANCED_PROP(NumberProps, _FowAlpha_Array);

                o.uv = v.uv;
                return o;
            }

            inline half4 SamplerNumber(half2 uv, half2 numberUV, in out half n, int index)
            {
                n = floor(n / 10);
                half offset = (n % 10) / 10;
                half flg = saturate(ceil(((index + 1) - floor(uv.x * COUNT)) * (ceil(uv.x * COUNT) - index))); 
                return tex2D(_NumberTex, numberUV.xy * _NumberTex_ST.xy + _NumberTex_ST.zw + half2(offset, 0)) * flg;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half4 color = _Color;

                // x: number, y: time stamp, z: disappear
                // float4 fowParam = UNITY_ACCESS_INSTANCED_PROP(NumberProps, _FowParam_Array);
                float4 fowParam = i.param;
                float fowNumber = fowParam.x;

                //#if UNITY_UV_STARTS_AT_TOP
                //    half2 numberUV = i.uv;
                //    numberUV = frac(i.uv * COUNT);

                //    half n = fowNumber * 10;

                //    half4 number = SamplerNumber(i.uv, numberUV, n, 6);
                //    number += SamplerNumber(i.uv, numberUV, n, 5);
                //    number += SamplerNumber(i.uv, numberUV, n, 4);
                //    number += SamplerNumber(i.uv, numberUV, n, 3);
                //    number += SamplerNumber(i.uv, numberUV, n, 2);
                //    number += SamplerNumber(i.uv, numberUV, n, 1);
                //    number += SamplerNumber(i.uv, numberUV, n, 0);

                //    number = lerp(number, _Color, 0.32f);

                //    half centerFlg = saturate(ceil(((3 + 1) - floor(i.uv.y * COUNT)) * (ceil(i.uv.y * COUNT) - 3))); 
                //    number = lerp(_Color, number, centerFlg);

                //    half4 tex = tex2D(_MainTex, i.uv);
                //    tex.rgb = lerp(number.rgb, tex.rgb, 0.5f);
                //    number.rgb = lerp(number.rgb, tex.rgb, tex.a);

                //    color = half4(number.rgb, 1);
                //#else

                //#endif

                color.g = 1 - i.alpha;

                // timestamp
                half disappear = saturate(fowParam.z);
                float time = fowParam.y;
                color.rgb = lerp(color.rgb, half3(0, 1, 0), lerp(0, saturate((_Time.y - time) / 2 /* * 2 */), disappear));


                return color;
            }
            ENDCG
        }
    }
}
