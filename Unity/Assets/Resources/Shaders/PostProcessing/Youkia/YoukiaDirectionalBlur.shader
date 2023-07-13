Shader "Hidden/PostProcessing/YoukiaDirectionalBlur"
{
    HLSLINCLUDE

        #include "../StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;
        
        // x: intensity, y: radius, z: distance, w: iterator
        half4 _DirectionBlurParam;
        half2 _DirectionCenter;

        half4 Frag(VaryingsDefault i) : SV_Target
        {
            half2 blurVector = (_DirectionCenter - i.texcoord.xy) * _DirectionBlurParam.z;

            half4 color = 0;
            half4 colMain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            for (int j = 0; j < _DirectionBlurParam.w; j++)
            {
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                i.texcoord.xy += blurVector;
            }

            color /= _DirectionBlurParam.w;

            half2 uvAtten = (i.texcoord.xy * 2 - 1) * half2(_ScreenParams.x / _ScreenParams.y, 1);
            half atten = saturate(sqrt(uvAtten.x * uvAtten.x + uvAtten.y * uvAtten.y));
            atten = saturate(pow(atten, _DirectionBlurParam.y));
            atten *= _DirectionBlurParam.x;
            color = lerp(colMain, color, atten);
            return color;
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
    }
}
