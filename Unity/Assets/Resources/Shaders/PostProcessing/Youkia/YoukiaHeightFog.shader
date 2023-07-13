// ref： https://iquilezles.org/www/articles/fog/fog.htm
// 高度雾

Shader "Hidden/PostProcessing/HeightFog"
{
    HLSLINCLUDE

        #include "HeightFog.hlsl"

        #pragma multi_compile __ _UNITY_RENDER
        #pragma multi_compile __ _HEIGHTFOG_NOISE
        // #pragma multi_compile __ _HEIGHTFOG_HEIGHTMAP
        #pragma multi_compile __ _EM

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        half4 _MainTex_TexelSize;

        TEXTURE2D_SAMPLER2D(_HeightFogTex, sampler_HeightFogTex);

        half4 Frag(VaryingsDefault i) : SV_Target
        {   
            half4 fog = HeightFog(i.texcoord);
            return fog;
        }

        half4 FragCombine(VaryingsDefault i) : SV_Target
        {
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            half4 fogColor = SAMPLE_TEXTURE2D(_HeightFogTex, sampler_HeightFogTex, i.texcoord);
            fogColor.rgb = lerp(color.rgb, fogColor.rgb, fogColor.a);
            
            return half4(fogColor.rgb, color.a);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        
        Pass
        {
            HLSLPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma vertex VertDefault
                #pragma fragment FragCombine

            ENDHLSL
        }
    }
}
