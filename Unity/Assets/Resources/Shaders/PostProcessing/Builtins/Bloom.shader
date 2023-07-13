// 2020-10-9
// 屏蔽了高品质采样

Shader "Hidden/PostProcessing/Bloom"
{
    HLSLINCLUDE

        #pragma multi_compile __ _UNITY_RENDER

        #include "../StdLib.hlsl"
        #include "../Colors.hlsl"
        #include "../Sampling.hlsl"
        #include "../MRT.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_BloomTex, sampler_BloomTex);
        
        half4 _MainTex_TexelSize;
        half  _SampleScale;
        half4 _ColorIntensity;
        half4 _Threshold; // x: threshold value (linear), y: threshold - knee, z: knee * 2, w: 0.25 / knee
        half4 _Params; // x: clamp, y: threshold eff, zw: unused
        // float _Threshold_Alpha;
        half _IterIntensity;
        half _IterIntensityEff;

        half _AlphaMask;

        // ----------------------------------------------------------------------------------------
        // eff scene lerp
        half EffLerp(half scene, half eff, half effmask)
        {
            return lerp(scene, eff, saturate(effmask * ALPHAFADE) * _AlphaMask);
        }

        // ----------------------------------------------------------------------------------------
        // Prefilter
        half4 Prefilter(half4 color, half2 uv, half particleMask)
        {
            //half autoExposure = SAMPLE_TEXTURE2D(_AutoExposureTex, sampler_AutoExposureTex, uv).r;
            //color *= autoExposure;

            color = min(_Params.x, color); // clamp to max
            half threshold = _Threshold.x;

            threshold = EffLerp(threshold, _Params.y, particleMask);
            color.rgb = QuadraticThreshold(color, threshold, _Threshold.yzw).rgb;
            return color;
        }

        // half4 FragPrefilter13(VaryingsDefault i) : SV_Target
        // {
        //     half4 color = DownsampleBox13Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
        //     return Prefilter(SafeHDR(color), i.texcoord);
        // }

        half4 FragPrefilter4(VaryingsDefault i) : SV_Target
        {
            half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
            half intensity = _IterIntensity;

            // mrt mask
            #if _UNITY_RENDER
                half particleMask = color.a;
            #else
                half4 mask = GetMask(i.texcoord);
                half particleMask = GetMaskParticle(mask);
            #endif

            intensity = EffLerp(_IterIntensity, _IterIntensityEff, particleMask);
            color.rgb *= intensity;
            return Prefilter(SafeHDR(color), i.texcoord, particleMask);
        }

        // ----------------------------------------------------------------------------------------
        // Downsample

        // half4 FragDownsample13(VaryingsDefault i) : SV_Target
        // {
        //     half4 color = DownsampleBox13Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
        //     return color;
        // }

        half4 FragDownsample4(VaryingsDefault i) : SV_Target
        {
            half4 color = DownsampleBox4Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy);
            // half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            // half intensity = _IterIntensity;

            // // mrt mask
            // #if _UNITY_RENDER
            //     half particleMask = color.a;
            // #else
            //     half4 mask = GetMask(i.texcoord);
            //     half particleMask = GetMaskParticle(mask);
            // #endif

            // intensity = EffLerp(_IterIntensity, _IterIntensityEff, particleMask);
            // color.rgb *= intensity;
            return color;
        }

        // ----------------------------------------------------------------------------------------
        // Upsample & combine

        half4 Combine(half4 bloom, half2 uv)
        {
            half4 color = SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, uv);
            return half4(bloom.rgb + color.rgb, color.a);
        }

        // half4 FragUpsampleTent(VaryingsDefault i) : SV_Target
        // {
        //     half4 bloom = UpsampleTent(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
        //     return Combine(bloom, i.texcoordStereo);
        // }

        half4 FragUpsampleBox(VaryingsDefault i) : SV_Target
        {
            half4 bloom = UpsampleBox(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), i.texcoord, UnityStereoAdjustedTexelSize(_MainTex_TexelSize).xy, _SampleScale);
            // half4 bloom = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            return Combine(bloom, i.texcoordStereo);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        // 0: Prefilter 13 taps
        // Pass
        // {
        //     HLSLPROGRAM

        //         #pragma vertex VertDefault
        //         #pragma fragment FragPrefilter13

        //     ENDHLSL
        // }

        // 1: Prefilter 4 taps
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragPrefilter4

            ENDHLSL
        }

        // 2: Downsample 13 taps
        // Pass
        // {
        //     HLSLPROGRAM

        //         #pragma vertex VertDefault
        //         #pragma fragment FragDownsample13

        //     ENDHLSL
        // }

        // 3: Downsample 4 taps
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragDownsample4

            ENDHLSL
        }

        // 4: Upsample tent filter
        // Pass
        // {
        //     HLSLPROGRAM

        //         #pragma vertex VertDefault
        //         #pragma fragment FragUpsampleTent

        //     ENDHLSL
        // }

        // 5: Upsample box filter
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUpsampleBox

            ENDHLSL
        }
    }
}
