// ref: https://zhuanlan.zhihu.com/p/510620589

Shader "Hidden/PostProcessing/YoukiaSSAO"
{
    HLSLINCLUDE
        #pragma multi_compile __ _UNITY_RENDER
        #include "../StdLib.hlsl"

        VaryingsDefault VertSSAO(AttributesDefault v)
        {
            VaryingsDefault o;
            o.vertex = float4(v.vertex.xy, 0.0, 1.0);
            o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

            #if UNITY_UV_STARTS_AT_TOP
                o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
            #endif

            o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);
            o.texcoordStereo.xy += 1.0e-6;

            return o;
        }


    ENDHLSL 

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        
        Pass
        {
            Name "SSAO_Occlusion"

            HLSLPROGRAM
                #include "SSAO.hlsl"
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex VertSSAO
                #pragma fragment FragSSAO

            ENDHLSL
        }

        Pass
        {
            Name "SSAO_HoriBlur"

            HLSLPROGRAM
                #include "SSAO.hlsl"
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex VertSSAO
                #pragma fragment FragHorizontalBlur
                

            ENDHLSL
        }

        Pass
        {
            Name "SSAO_VertBlur"

            HLSLPROGRAM
                #include "SSAO.hlsl"
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex VertSSAO
                #pragma fragment FragVerticalBlur

            ENDHLSL
        }

        Pass
        {
            Name "SSAO_FinalBlur"

            HLSLPROGRAM
                #include "SSAO.hlsl"
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex VertSSAO
                #pragma fragment FinalBlur

            ENDHLSL
        }

        Pass
        {
            Name "SSAO_HoriGaussainBlur"

            HLSLPROGRAM
                #include "SSAO.hlsl"
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex VertBlurH
                #pragma fragment FragGaussainBlurH
                

            ENDHLSL
        }

        Pass
        {
            Name "SSAO_VertGaussianBlur"

            HLSLPROGRAM
                #include "SSAO.hlsl"
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex VertBlurV
                #pragma fragment FragGaussainBlurV

            ENDHLSL
        }
    }
}
