Shader "Hidden/PostProcessing/YoukiaMaskBlur"
{
    HLSLINCLUDE

        #include "../StdLib.hlsl"
        #include "../Mask.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;
        
        struct VaryingsBlur
        {
            float4 vertex : SV_POSITION;
            float2 texcoord : TEXCOORD0;
            float2 texcoordStereo : TEXCOORD1;
            float4 uv1 : TEXCOORD2;
            float4 uv2 : TEXCOORD3;	
            #if STEREO_INSTANCING_ENABLED
            uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
            #endif
        };

        // blur
        VaryingsBlur VertBlur(AttributesDefault v)
        {
            VaryingsBlur o;
            o.vertex = float4(v.vertex.xy, 0.0, 1.0);
            o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

            #if UNITY_UV_STARTS_AT_TOP
                o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
            #endif

            o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

            half radius = _MaskParam.x;
            o.uv1.xy = o.texcoordStereo.xy + radius * _MainTex_TexelSize * half2(1, 1);
            o.uv1.zw = o.texcoordStereo.xy + radius * _MainTex_TexelSize * half2(-1, 1);
            o.uv2.xy = o.texcoordStereo.xy + radius * _MainTex_TexelSize * half2(-1, -1);
            o.uv2.zw = o.texcoordStereo.xy + radius * _MainTex_TexelSize * half2(1, -1);

            return o;
        }

        half4 Frag(VaryingsDefault i) : SV_Target
        {
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            
            return Mask(color, i.texcoord);
        }

        half4 FragMask(VaryingsDefault i) : SV_Target
        {
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

            return MaskTexture(color, i.texcoord);
        }

        half4 FragBlur(VaryingsBlur i) : SV_Target
        {
            half4 col = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord));
            half a = col.a;
            half4 sum = col;

            half4 tmp = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv1.xy));
            sum += tmp;
            tmp = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv1.zw));
            sum += tmp;
            tmp = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv2.xy));
            sum += tmp;
            tmp = saturate(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv2.zw));
            sum += tmp;

            sum *= 0.2f;

            col = sum;
            return col;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragMask

            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertBlur
                #pragma fragment FragBlur

            ENDHLSL
        }
    }
}
