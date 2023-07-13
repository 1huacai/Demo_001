Shader "Hidden/PostProcessing/YoukiaSkinSSS"
{
    HLSLINCLUDE

        #include "../StdLib.hlsl"
        #include "../MRT.hlsl"
        #pragma multi_compile __ _UNITY_RENDER

        #define DistanceToProjectionWindow 5.671281819617709             //1.0 / tan(0.5 * radians(20));
        #define DPTimes300 1701.384545885313                             //DistanceToProjectionWindow * 300
        #define MAXSamplerSteps 6
        #define RANDOM(seed) (sin(cos(seed * 1354.135748 + 13.546184) * 1354.135716 + 32.6842317))

        half _SamplerSteps;
        half4 _Kernel[MAXSamplerSteps];
        half _RandomNumber;
        half _SSSScale;

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;
        
        half4 SkinSSS(half4 color, float2 uv, half2 intensity)
        {
            float depth = YoukiaDepth(uv);
            float linearDepth = Linear01Depth(depth);
            linearDepth = saturate(linearDepth / 0.0005f);
            depth = LinearEyeDepth(depth);

            half blurLength = DistanceToProjectionWindow / depth;
            // return blurLength;
            half2 uvOffset = intensity * blurLength * linearDepth;
            half4 blurColor = color;
            blurColor.rgb *= _Kernel[0].rgb;

            half samplerSteps = min(_SamplerSteps, MAXSamplerSteps);
            [unroll(samplerSteps)]
            for(int i = 1; i < samplerSteps; i++)
            {
                half2 curUV = RANDOM((_ScreenParams.y * uv.y * 5 + uv.x) * _ScreenParams.x * 5 + _RandomNumber) * uvOffset;
                half4 k = _Kernel[i];
                half2 sssUV = uv + k.a * curUV;
                half4 sssColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sssUV);
                
                UNITY_BRANCH
                if (sssColor.a <= 1)
                {
                    blurColor.rgb += k.rgb * color.rgb;
                }
                else
                {
                    float sssDepth = 0;
                    sssDepth = YoukiaDepth(sssUV);

                    sssDepth = LinearEyeDepth(sssDepth);
                    half sssScale = saturate(DPTimes300 * intensity * abs(depth - sssDepth));
                    sssColor.rgb = lerp(sssColor.rgb, color.rgb, sssScale);
                    blurColor.rgb += k.rgb * sssColor.rgb;
                }
            }

            return blurColor;
        }

        half4 Frag(VaryingsDefault i, half2 intensity)
        {
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            half3 sssColor = color.rgb;
            if (color.a > 1)
                sssColor = SkinSSS(color, i.texcoord, intensity).rgb;

            return half4(sssColor.rgb, color.a);
        }

        half4 FragX(VaryingsDefault i) : SV_Target
        {
            half4 color = 0;
            half SSSIntencity = 0;
            SSSIntencity = (_SSSScale * YoukiaDepthTextureTexelSize().x);

            color = Frag(i, half2(SSSIntencity, 0));

            return color;
        }

        half4 FragY(VaryingsDefault i) : SV_Target
        {
            half4 color = 0;
            half SSSIntencity = 0;
            SSSIntencity = (_SSSScale * YoukiaDepthTextureTexelSize().y);
            
            color = Frag(i, half2(0, SSSIntencity));

            // color.a = ceil(saturate(color.a - 1));

            return color;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "x-blur"
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragX

            ENDHLSL
        }

        Pass
        {
            Name "y-blur"
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragY

            ENDHLSL
        }
    }
}
