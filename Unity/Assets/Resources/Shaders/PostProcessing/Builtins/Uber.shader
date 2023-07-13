Shader "Hidden/PostProcessing/Uber"
{
    HLSLINCLUDE

        #pragma target 3.0

        #pragma multi_compile __ DISTORT
        // #pragma multi_compile __ CHROMATIC_ABERRATION
        #pragma multi_compile __ BLOOM
        #pragma multi_compile __ VIGNETTE
        // #pragma multi_compile __ FINALPASS
        #pragma multi_compile __ SCREENWAVE
        #pragma multi_compile __ _UNITY_RENDER
        #pragma multi_compile __ MASKBLUR MASKBLUR_TEXTURE
        #pragma multi_compile __ GRAY

        // the following keywords are handled in API specific SubShaders below
        // #pragma multi_compile __ COLOR_GRADING_LDR_2D COLOR_GRADING_HDR_2D COLOR_GRADING_HDR_3D
        // #pragma multi_compile __ STEREO_INSTANCING_ENABLED STEREO_DOUBLEWIDE_TARGET
        
        #pragma vertex VertUber
        #pragma fragment FragUber
    
		#include "../StdLib.hlsl"
        #include "../Colors.hlsl"
        #include "../Sampling.hlsl"
        #include "../MRT.hlsl"
        #include "../Mask.hlsl"
        #include "Distortion.hlsl"
        #include "Dithering.hlsl"
        #include "../Youkia/Gray.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;

        // Auto exposure / eye adaptation
        // TEXTURE2D_SAMPLER2D(_AutoExposureTex, sampler_AutoExposureTex);

        // Bloom
        TEXTURE2D_SAMPLER2D(_BloomTex, sampler_BloomTex);
        TEXTURE2D_SAMPLER2D(_Bloom_DirtTex, sampler_Bloom_DirtTex);
        float4 _BloomTex_TexelSize;
        float4 _Bloom_DirtTileOffset; // xy: tiling, zw: offset
        half4 _Bloom_Settings; // x: sampleScale, y: intensity, z: dirt intensity
        half3 _Bloom_Color;

        // Chromatic aberration
        // TEXTURE2D_SAMPLER2D(_ChromaticAberration_SpectralLut, sampler_ChromaticAberration_SpectralLut);
        // half _ChromaticAberration_Amount;

        // Color grading
        #if COLOR_GRADING_HDR_3D
            TEXTURE3D_SAMPLER3D(_Lut3D, sampler_Lut3D);
            float2 _Lut3D_Params;
        #else
            TEXTURE2D_SAMPLER2D(_Lut2D, sampler_Lut2D);
            float3 _Lut2D_Params;
        #endif

        half _PostExposure; // EV (exp2)

        // Vignette
        half3 _Vignette_Color;
        half2 _Vignette_Center; // UV space
        half4 _Vignette_Settings; // x: intensity, y: smoothness, z: roundness, w: rounded
        half _Vignette_Opacity;
        half _Vignette_Mode; // <0.5: procedural, >=0.5: masked
        TEXTURE2D_SAMPLER2D(_Vignette_Mask, sampler_Vignette_Mask);

        float _AlphaMask;

        // volumetric light
        half _gVolumetricLight;
        sampler2D g_rtVolumetricLight;

        // Misc
        half _LumaInAlpha;

        // wave 
        // x: wave width, y: wave speed, z: intensity, w: wave radius
        half4 _ScreenWaveParam0;
        // x: cur wave distance, y: wave loop
        half4 _ScreenWaveParam1;

        // 角色后处理修正
        half4 _gCharacterLightParam;
        #define _gCharacterColorGradingScale _gCharacterLightParam.w

        // 截屏参数 
        half _gCapture;
        TEXTURE2D_SAMPLER2D(_gGrabScreenTex, sampler_gGrabScreenTex);

        // ui effect
        half _UIEffect;

        VaryingsDefault VertUber(AttributesDefault v)
        {
            VaryingsDefault o;

            // #if STEREO_DOUBLEWIDE_TARGET
            //     o.vertex = float4(v.vertex.xy * _PosScaleOffset.xy + _PosScaleOffset.zw, 0.0, 1.0);
            // #else
                o.vertex = float4(v.vertex.xy, 0.0, 1.0);
            // #endif

            o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
            o.texcoord = o.texcoord * _UVTransform.xy + _UVTransform.zw;

            o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);
            // #if STEREO_INSTANCING_ENABLED
            //     o.stereoTargetEyeIndex = (uint)_DepthSlice;
            // #endif
            return o;
        }

        half4 FragUber(VaryingsDefault i) : SV_Target
        {
            half2 uv = i.texcoord;
            
            //>>> Automatically skipped by the shader optimizer when not used
            half2 uvDistorted = Distort(i.texcoord);
            half2 uvStereoDistorted = Distort(i.texcoordStereo);
            //<<<

            half4 color = (0.0).xxxx;

            #if SCREENWAVE
                half2 dv = half2(0.5, 0.5) - uv;
                dv = dv * float2(_ScreenParams.x / _ScreenParams.y, 1);
                half dis = (dv.x * dv.x + dv.y * dv.y);

                half sinFactor = sin(dis * _ScreenWaveParam0.x - _Time.y * _ScreenWaveParam0.y) * _ScreenWaveParam0.z * 0.1;
                half fade = saturate(_ScreenWaveParam0.w - abs(_ScreenWaveParam1.x - dis));
                fade = lerp(fade, 1, _ScreenWaveParam1.y);

                dv = normalize(dv);
                half2 offset = dv  * sinFactor * fade;
                half2 waveUV = offset + uv;

                color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, waveUV);
            #else
                color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvStereoDistorted);
            #endif

            half alpha = color.a;

            // mrt mask
            half4 mask = GetMask(uvDistorted);
            half characterMask = GetMaskCharacter(mask);
            #if _UNITY_RENDER
                half particleMask = color.a;
            #else
                half particleMask = GetMaskParticle(mask);
            #endif
            characterMask = lerp(characterMask, 0, _UIEffect);
            particleMask = lerp(particleMask, 0, _UIEffect);

            // Inspired by the method described in "Rendering Inside" [Playdead 2016]
            // https://twitter.com/pixelmager/status/717019757766123520
            // #if CHROMATIC_ABERRATION
            // {
            //     half2 coords = 2.0 * uv - 1.0;
            //     half2 end = uv - coords * dot(coords, coords) * _ChromaticAberration_Amount;
            //     half2 delta = (end - uv) / 3;

            //     half4 filterA = half4(SAMPLE_TEXTURE2D_LOD(_ChromaticAberration_SpectralLut, sampler_ChromaticAberration_SpectralLut, float2(0.5 / 3, 0.0), 0).rgb, 1.0);
            //     half4 filterB = half4(SAMPLE_TEXTURE2D_LOD(_ChromaticAberration_SpectralLut, sampler_ChromaticAberration_SpectralLut, float2(1.5 / 3, 0.0), 0).rgb, 1.0);
            //     half4 filterC = half4(SAMPLE_TEXTURE2D_LOD(_ChromaticAberration_SpectralLut, sampler_ChromaticAberration_SpectralLut, float2(2.5 / 3, 0.0), 0).rgb, 1.0);

            //     half4 texelA = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(Distort(uv)), 0);
            //     half4 texelB = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(Distort(delta + uv)), 0);
            //     half4 texelC = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, UnityStereoTransformScreenSpaceTex(Distort(delta * 2.0 + uv)), 0);

            //     half4 sum = texelA * filterA + texelB * filterB + texelC * filterC;
            //     half4 filterSum = filterA + filterB + filterC;
            //     color = sum / filterSum;
            // }
            // #else
            // {
            //     color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvStereoDistorted);
            // }
            // #endif

            // mask blur
            #if MASKBLUR
                color = Mask(color, uvDistorted);
            #elif MASKBLUR_TEXTURE
                color = MaskTexture(color, uvDistorted);
            #endif

            // Gamma space... Gah.
            // #if UNITY_COLORSPACE_GAMMA
            // {
            //     color = SRGBToLinear(color);
            // }
            // #endif

            // color.rgb *= autoExposure;

            #if BLOOM
            {
                half4 bloom = UpsampleBox(TEXTURE2D_PARAM(_BloomTex, sampler_BloomTex), uvDistorted, _BloomTex_TexelSize.xy, _Bloom_Settings.x);
                // half4 bloom = SAMPLE_TEXTURE2D(_BloomTex, sampler_BloomTex, uvDistorted);
                half bloomIntensity = lerp(_Bloom_Settings.y, _Bloom_Settings.w, saturate(particleMask * ALPHAFADE));

                // Additive bloom (artist friendly)
                bloom *= bloomIntensity;
                color += bloom * half4(_Bloom_Color, 1.0);
                
                // dirty
                // UVs should be Distort(uv * _Bloom_DirtTileOffset.xy + _Bloom_DirtTileOffset.zw)
                // but considering we use a cover-style scale on the dirt texture the difference
                // isn't massive so we chose to save a few ALUs here instead in case lens distortion
                // is active
                // UNITY_BRANCH
                // if (_Bloom_Settings.z > 0)
                // {
                //     half4 dirt = half4(SAMPLE_TEXTURE2D(_Bloom_DirtTex, sampler_Bloom_DirtTex, uvDistorted * _Bloom_DirtTileOffset.xy + _Bloom_DirtTileOffset.zw).rgb, 0.0);
                //     dirt *= _Bloom_Settings.z;
                //     color += dirt * bloom;
                // }
            }
            #endif

            #if VIGNETTE
            {
                UNITY_BRANCH
                if (_Vignette_Mode < 0.5)
                {
                    half2 d = abs(uvDistorted - _Vignette_Center) * _Vignette_Settings.x;
                    d.x *= lerp(1.0, _ScreenParams.x / _ScreenParams.y, _Vignette_Settings.w);
                    d = pow(saturate(d), _Vignette_Settings.z); // Roundness
                    half vfactor = pow(saturate(1.0 - dot(d, d)), _Vignette_Settings.y);
                    color.rgb *= lerp(_Vignette_Color, (1.0).xxx, vfactor);
                    // color.a = lerp(1.0, color.a, vfactor);
                }
                else
                {
                    half vfactor = SAMPLE_TEXTURE2D(_Vignette_Mask, sampler_Vignette_Mask, uvDistorted).a;

                    // #if !UNITY_COLORSPACE_GAMMA
                    // {
                        vfactor = SRGBToLinear(vfactor);
                    // }
                    // #endif

                    //half3 new_color = color.rgb * lerp(_Vignette_Color, (1.0).xxx, vfactor);
                    half3 new_color = lerp(_Vignette_Color, color.rgb, vfactor);
                    color.rgb = lerp(color.rgb, new_color, _Vignette_Opacity);
                    // color.a = lerp(1.0, color.a, vfactor);
                }
            }
            #endif

            // volumetric light
            UNITY_BRANCH
            if (_gVolumetricLight > 0)
            {
                half3 volumetric = tex2D(g_rtVolumetricLight, i.texcoord).rgb;
                color.rgb += volumetric;
            }

            half3 colTmp = color.rgb;
            #if COLOR_GRADING_HDR_3D
            {
                color *= _PostExposure;
                half3 colorLutSpace = saturate(LUT_SPACE_ENCODE(color.rgb));

                color.rgb = ApplyLut3D(TEXTURE3D_PARAM(_Lut3D, sampler_Lut3D), colorLutSpace, _Lut3D_Params);
            }
            #elif COLOR_GRADING_HDR_2D
            {
                color *= _PostExposure;
                half3 colorLutSpace = saturate(LUT_SPACE_ENCODE(color.rgb));
                color.rgb = ApplyLut2D(TEXTURE2D_PARAM(_Lut2D, sampler_Lut2D), colorLutSpace, _Lut2D_Params);
            }
            #elif COLOR_GRADING_LDR_2D
            {
                color = saturate(color);

                // LDR Lut lookup needs to be in sRGB - for HDR stick to linear
                color.rgb = LinearToSRGB(color.rgb);
                color.rgb = ApplyLut2D(TEXTURE2D_PARAM(_Lut2D, sampler_Lut2D), color.rgb, _Lut2D_Params);
                color.rgb = SRGBToLinear(color.rgb);
            }
            #endif

            // 角色遮罩-mrt
            color.rgb = lerp(color.rgb, colTmp, characterMask * _gCharacterColorGradingScale);
            // 粒子遮罩-alpha mask
            particleMask = saturate(particleMask * ALPHAFADE) * _AlphaMask;
            color.rgb = lerp(color.rgb, colTmp, particleMask);

            // color.rgb = Dither(color.rgb, i.texcoord);

            color.rgb = GrayEffect(color.rgb, i.texcoord, particleMask);

            half4 output = color;
            output.a = alpha;

            // #if FINALPASS
            // {
            //     #if UNITY_COLORSPACE_GAMMA
            //     {
            //         output = LinearToSRGB(output);
            //     }
            //     #endif

            // output.rgb = Dither(output.rgb, i.texcoord);
            // }
            // #else
            // {
            //     UNITY_BRANCH
            //     if (_LumaInAlpha > 0.5)
            //     {
            //         // Put saturated luma in alpha for FXAA - higher quality than "green as luma" and
            //         // necessary as RGB values will potentially still be HDR for the FXAA pass
            //         half luma = Luminance(saturate(output));
            //         output.a = luma;
            //     }

            //     #if UNITY_COLORSPACE_GAMMA
            //     {
            //         output = LinearToSRGB(output);
            //     }
            //     #endif
            // }
            // #endif

            // Output RGB is still HDR at that point (unless range was crunched by a tonemapper)
            // 截图
            #if UNITY_UV_STARTS_AT_TOP
                if (_gCapture > 0)
                {
                    half4 grab = SAMPLE_TEXTURE2D(_gGrabScreenTex, sampler_gGrabScreenTex, uvStereoDistorted);
                    // grab.a = ceil(grab.a);
                    output.a = max(grab.a, output.a);
                    // output.a = LinearToSRGB(output.a);
                }
            #else
                output.a = 0;
            #endif

            output.a = lerp(output.a, alpha, _UIEffect);

            return output;
        }

    ENDHLSL
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile __ COLOR_GRADING_LDR_2D COLOR_GRADING_HDR_2D COLOR_GRADING_HDR_3D
            ENDHLSL
        }
    }
}
