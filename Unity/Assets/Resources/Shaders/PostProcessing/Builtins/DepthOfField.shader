Shader "Hidden/PostProcessing/DepthOfField"
{
    // Fallback SubShader with SM 3.5
    // DX11+, OpenGL 3.2+, OpenGL ES 3+, Metal, Vulkan, consoles
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass // 0
        {
            Name "CoC Calculation"

            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex VertDefault
                #pragma fragment FragCoC
                #pragma multi_compile __ _UNITY_RENDER

                #include "DepthOfField.hlsl"
            ENDHLSL
        }

        Pass // 1
        {
            Name "Downsample and Prefilter"

            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex VertDefault
                #pragma fragment FragPrefilter
                #pragma multi_compile __ _UNITY_RENDER

                #include "DepthOfField.hlsl"
            ENDHLSL
        }

        Pass // 2
        {
            Name "Downsample"

            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex VertDefault
                #pragma fragment FragDownSample
                #pragma multi_compile __ _UNITY_RENDER

                #include "DepthOfField.hlsl"
            ENDHLSL
        }

        Pass // 3
        {
            Name "Bokeh Filter (small)"

            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex VertDefault
                #pragma fragment FragBlur
                #define KERNEL_SMALL
                #pragma multi_compile __ _UNITY_RENDER

                #include "DepthOfField.hlsl"
            ENDHLSL
        }

        Pass // 4
        {
            Name "Simple blur"

            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex VertSimpleBlur
                #pragma fragment FragSimpleBlur
                #pragma multi_compile __ _UNITY_RENDER

                #include "DepthOfField.hlsl"
            ENDHLSL
        }


        Pass // 5
        {
            Name "Combine"

            HLSLPROGRAM
                #pragma target 3.5
                #pragma vertex VertDefault
                #pragma fragment FragCombine
                #pragma multi_compile __ _UNITY_RENDER
                
                #include "DepthOfField.hlsl"
            ENDHLSL
        }
    }
}
