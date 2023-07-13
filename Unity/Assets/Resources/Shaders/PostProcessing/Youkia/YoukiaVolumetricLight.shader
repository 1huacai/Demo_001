Shader "Hidden/PostProcessing/YoukiaVolumetricLight"
{
    HLSLINCLUDE

        #include "../Youkia.hlsl"
        #include "../Blur.hlsl"
        #include "../MRT.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        half4 _MainTex_TexelSize;

        sampler2D _gBlueNoise;
        half4 _vlSunUV;
         // x: intensity, y: light atten, z: light distance, w: sample count
        half4 _vlParams;
        // x: dither scale, y: dither strength, z: blur radius
        half4 _vlDitherParams;

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            half occlusion = 1;
            half4 color = 0;
            half2 uvDir = _vlSunUV.xy - i.texcoord;
            half2 uvDither = i.texcoord;
            uvDither.y = uvDither.y * (_MainTex_TexelSize.w / _MainTex_TexelSize.z);
            half dither = (tex2D(_gBlueNoise, uvDither * _vlDitherParams.x).r) * 2 - 1;
            dither *= _vlDitherParams.y;
            
            half sqrlen = length(uvDir);
            half shadowDistance = _vlParams.z;
            half sampleCount = _vlParams.w;
            half2 uv = normalize(uvDir);

            uvDir = lerp(uvDir, uv * shadowDistance, saturate(ceil(sqrlen - shadowDistance)));
            // if (sqrlen > shadowDistance)
            // {
            //     uvDir = uv * shadowDistance;
            // }

            half dis = max(length(_vlSunUV.xy - i.texcoord), 0.3) + dither;
            half2 deltaUV = (uvDir / sampleCount);

            half maxLum = 2;

            [unroll(4)]
            for (int a = 1; a < sampleCount; a++)
            {   
                half2 texcoord = deltaUV * a * dis * 2 + i.texcoord;
                half4 currentCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texcoord);
                // half4 mask = GetMask(texcoord);
                half4 mask = SAMPLE_TEXTURE2D(_gBufferOri_0, sampler_gBufferOri_0, texcoord);
                currentCol.a = GetMaskOpaque(mask);
                currentCol.a = step(0.01f, currentCol.a);

                currentCol = min(currentCol, maxLum);
                occlusion = exp(-_vlParams.y * currentCol.a);
                occlusion *= occlusion;
                occlusion *= occlusion;
                occlusion *= occlusion;
                occlusion *= occlusion;

                color.rgb += occlusion * currentCol.rgb * _vlSunUV.z * _vlSunUV.z;
            }

            color.rgb = color.rgb / sampleCount;

            color.rgb *= _vlParams.x;
            
            return color;
        }

        // Gaussian blur
        VaryingsGaussianBlur VertBlurH(AttributesDefault v)
        {
            return VertH(v, _vlDitherParams.z, _MainTex_TexelSize);
        }

        VaryingsGaussianBlur VertBlurV(AttributesDefault v)
        {
            return VertV(v, _vlDitherParams.z, _MainTex_TexelSize);
        }

        half4 FragGaussainBlur(VaryingsGaussianBlur i) : SV_Target
        {
            return FragGaussian(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i);
        }


    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "VolumetricLight"
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }

        Pass
        {
            Name "VolumetricLight_HoriGaussainBlur"

            HLSLPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex VertBlurH
                #pragma fragment FragGaussainBlur
                

            ENDHLSL
        }

        Pass
        {
            Name "VolumetricLight_VertGaussianBlur"

            HLSLPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma vertex VertBlurV
                #pragma fragment FragGaussainBlur

            ENDHLSL
        }
    }
}
