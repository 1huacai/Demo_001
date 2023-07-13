// ref: https://zhuanlan.zhihu.com/p/425233743
// ref: https://zhuanlan.zhihu.com/p/64993622
// 出于带宽、性能的考虑, 没有采样 motion vector 处理动态物体

Shader "Hidden/PostProcessing/YoukiaTAA"
{
    HLSLINCLUDE

        #include "../StdLib.hlsl"
        #include "../MRT.hlsl"

        static const float VarianceClipGamma = 1.0f;
        static const float BlendWeightLowerBound = 0.03f;
        static const float BlendWeightUpperBound = 1.0f;
        static const uint NeighCount = 8;
        static const uint LoopCount = 4;
        static const float MinValue = 0.0000152f; // (1.0 / 65536.0)

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_taaHistoryTex, sampler_taaHistoryTex);

        half4 _MainTex_TexelSize;
        // x: blend, y: sharpen
        half4 _taaParam;
        float4x4 _preViewProj;
        float2 _taaOffset3x3[NeighCount];
        float2 _taaOffset2x2[LoopCount];

        static const half2 Offset3x3[NeighCount] = 
        {
            half2(-1, -1),
            half2(-1, 0),
            half2(-1, 1),
            half2(0, -1),
            // half2(0, 0),
            half2(0, 1),
            half2(1, -1),
            half2(1, 0),
            half2(1, 1),
        };

        //--------------------------------------------------------------------------------------
        // rgb ycocg
        // This function take a rgb color (best is to provide color in sRGB space)
        // and return a YCoCg color in [0..1] space for 8bit (An offset is apply in the function)
        // Ref: http://www.nvidia.com/object/real-time-ycocg-dxt-compression.html
        #define YCOCG_CHROMA_BIAS (128.0 / 255.0)
        half3 RGBToYCoCg(half3 rgb)
        {
            half3 YCoCg;
            YCoCg.x = dot(rgb, half3(0.25, 0.5, 0.25));
            YCoCg.y = dot(rgb, half3(0.5, 0.0, -0.5)) + YCOCG_CHROMA_BIAS;
            YCoCg.z = dot(rgb, half3(-0.25, 0.5, -0.25)) + YCOCG_CHROMA_BIAS;

            return YCoCg;
        }

        half3 YCoCgToRGB(half3 YCoCg)
        {
            half Y = YCoCg.x;
            half Co = YCoCg.y - YCOCG_CHROMA_BIAS;
            half Cg = YCoCg.z - YCOCG_CHROMA_BIAS;

            half3 rgb;
            rgb.r = Y + Co - Cg;
            rgb.g = Y + Cg;
            rgb.b = Y - Co - Cg;

            return rgb;
        }

        float3 RGB2YCoCgR(float3 rgbColor)
        {
            float3 YCoCgRColor;

            YCoCgRColor.y = rgbColor.r - rgbColor.b;
            float temp = rgbColor.b + YCoCgRColor.y / 2;
            YCoCgRColor.z = rgbColor.g - temp;
            YCoCgRColor.x = temp + YCoCgRColor.z / 2;

            return YCoCgRColor;
        }

        float3 YCoCgR2RGB(float3 YCoCgRColor)
        {
            float3 rgbColor;

            float temp = YCoCgRColor.x - YCoCgRColor.z / 2;
            rgbColor.g = YCoCgRColor.z + temp;
            rgbColor.b = temp - YCoCgRColor.y / 2;
            rgbColor.r = rgbColor.b + YCoCgRColor.y;

            return rgbColor;
        }

        //--------------------------------------------------------------------------------------
        // tonemap
        float Luminance(in float3 color)
        {
            return dot(color, float3(0.25f, 0.50f, 0.25f));
        }

        float3 ToneMap(float3 color)
        {
            return color / (1 + Luminance(color));
        }

        float3 UnToneMap(float3 color)
        {
            return color / (1 - Luminance(color));
        }

        // Faster but less accurate luma computation. 
        // Luma includes a scaling by 4.
        float Luma4(float3 Color)
        {
            return (Color.g * 2.0) + (Color.r + Color.b);
        }

        //--------------------------------------------------------------------------------------
        // clip
        half3 ClipHistory(half3 History, half3 BoxMin, half3 BoxMax)
        {
            half3 Filtered = (BoxMin + BoxMax) * 0.5f;
            half3 RayOrigin = History;
            half3 RayDir = Filtered - History;
            RayDir = abs(RayDir) < MinValue ? MinValue : RayDir;
            half3 InvRayDir = rcp(RayDir);
            
            half3 MinIntersect = (BoxMin - RayOrigin) * InvRayDir;
            half3 MaxIntersect = (BoxMax - RayOrigin) * InvRayDir;
            half3 EnterIntersect = min(MinIntersect, MaxIntersect);
            half ClipBlend = max(EnterIntersect.x, max(EnterIntersect.y, EnterIntersect.z));
            ClipBlend = saturate(ClipBlend);
            return lerp(History, Filtered, ClipBlend);
        }

        float3 ClipAABB(float3 color, float3 minimum, float3 maximum)
        {
            // Note: only clips towards aabb center (but fast!)
            float3 center = 0.5 * (maximum + minimum);
            float3 extents = 0.5 * (maximum - minimum);

            // This is actually `distance`, however the keyword is reserved
            float3 offset = color.rgb - center;

            float3 ts = abs(extents / (offset + 0.0001));
            float t = saturate(Min3(ts.x, ts.y, ts.z));
            color.rgb = center + offset * t;
            return color;
        }

        //--------------------------------------------------------------------------------------
        // re proj
        half2 ReProjection(float3 worldPos)
        {
            float4 p = mul(_preViewProj, float4(worldPos, 1));
            p /= p.w;
            half2 uv = p * .5 + .5;

            // for phy camera
            uv += half2(unity_CameraProjection._m02, unity_CameraProjection._m12) * 0.5f;
            // Unjitter
            uv = uv - _taaJitter.zw + _taaJitter.xy;
            
            return uv;
        }
        //--------------------------------------------------------------------------------------
        // shadrpen
        half3 Sharpen(half3 color, half3 minRGB, half3 maxRGB)
        {
            float3 corners = 4.0 * (minRGB + maxRGB) - 2.0 * color.rgb;
            color.rgb += (color.rgb - (corners * 0.166667)) * 2.718282 * _taaParam.y;
            color.rgb = clamp(color.rgb, 0.0, HALF_MAX_MINUS1);

            return color;
        }

        struct OutputSolver
        {
            float4 destination : SV_Target0;
            float4 history     : SV_Target1;
        };

        OutputSolver Frag(VaryingsDefault i)
        {
            half2 uv = i.texcoord;
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

            // history
            // reprojection
            float depth = YoukiaDepth01(uv);
            float3 wsPos = GetWorldPositionFromLinearDepthValue(uv, depth).xyz;
            half2 historyUV = ReProjection(wsPos);
            half4 history = SAMPLE_TEXTURE2D(_taaHistoryTex, sampler_taaHistoryTex, historyUV);
            half3 preYCoCg = RGBToYCoCg(history.xyz);

            half3 AABBMin, AABBMax;
            AABBMax = AABBMin = RGBToYCoCg(color.xyz);

            [unroll]
            for (int j = 0; j < LoopCount; j++)
            {
                // float2 duv = Offset3x3[j] / _ScreenParams.xy;
                float2 duv = _taaOffset2x2[j];
                half3 c = RGBToYCoCg(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, saturate(uv + duv)).xyz);
                AABBMin = min(AABBMin, c);
                AABBMax = max(AABBMax, c);
            }

            // Sharpen output
            color.rgb = Sharpen(color.rgb, YCoCgToRGB(AABBMin), YCoCgToRGB(AABBMax));

            // clip
            // history.xyz = YCoCgToRGB(ClipHistory(preYCoCg, AABBMin, AABBMax));
            // history.xyz = YCoCgToRGB(clamp(preYCoCg, AABBMin, AABBMax));
            history.xyz = YCoCgToRGB(ClipAABB(preYCoCg, AABBMin, AABBMax).rgb);

            half blendFactor = _taaParam.x;
            color.rgb = lerp(history, color.rgb, blendFactor);

            color.rgb = clamp(color.rgb, 0.0, HALF_MAX_MINUS1);

            OutputSolver output;
            output.destination = color;
            output.history = color;

            return output;
        }

        OutputSolver FragVarianceClip(VaryingsDefault i)
        {
            half2 uv = i.texcoord;
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            float3 colorCoCgR = ToneMap(color);
            colorCoCgR = RGB2YCoCgR(colorCoCgR);

            const uint N = 5;
            float3 m1 = colorCoCgR;
            float3 m2 = colorCoCgR * colorCoCgR;

            [unroll]
            for (int j = 0; j < LoopCount; j++)
            {
                // half2 offset = Offset3x3[j];
                // float2 sampleOffset = offset / _ScreenParams.xy;
                float2 sampleOffset = _taaOffset2x2[j];
                float2 sampleUV = uv + sampleOffset;
                sampleUV = saturate(sampleUV);

                float3 colorNeighborhood = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV);
                colorNeighborhood = max(colorNeighborhood, 0.0f);
                colorNeighborhood = ToneMap(colorNeighborhood);
                colorNeighborhood = RGB2YCoCgR(colorNeighborhood);

                m1 += colorNeighborhood;
                m2 += colorNeighborhood * colorNeighborhood;
            }

            // Variance clip.
            float3 mu = m1 / N;
            float3 sigma = sqrt(abs(m2 / N - mu * mu));
            float3 minc = mu - VarianceClipGamma * sigma;
            float3 maxc = mu + VarianceClipGamma * sigma;

            // Sharpen output
            color.rgb = Sharpen(color.rgb, UnToneMap(YCoCgR2RGB(minc)), UnToneMap(YCoCgR2RGB(maxc)));
            half3 curColor = ToneMap(color);
            curColor = RGB2YCoCgR(curColor);

            // history
            // reprojection
            float depth = YoukiaDepth01(uv);
            float3 wsPos = GetWorldPositionFromLinearDepthValue(uv, depth).xyz;
            half2 historyUV = ReProjection(wsPos);
            half4 history = SAMPLE_TEXTURE2D(_taaHistoryTex, sampler_taaHistoryTex, historyUV);
            history.rgb = ToneMap(history);
            history.rgb = RGB2YCoCgR(history);

            // clip
            history.rgb = ClipAABB(history, minc, maxc).rgb;

            // float weightCurr = lerp(BlendWeightLowerBound, BlendWeightUpperBound, _taaParam.x);
            float weightCurr = _taaParam.x;
            float weightPrev = 1.0f - weightCurr;

            float RcpWeight = rcp(weightCurr + weightPrev);

            curColor = (curColor * weightCurr + history.rgb * weightPrev) * RcpWeight;
            curColor = YCoCgR2RGB(curColor);
            color.rgb = UnToneMap(curColor);

            color.rgb = clamp(color.rgb, 0.0, HALF_MAX_MINUS1);

            OutputSolver output;
            output.destination = color;
            output.history = color;

            return output;
        }

    ENDHLSL 

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        
        Pass
        {
            Name "TAA Default"

            HLSLPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }

        Pass
        {
            Name "TAA VarianceClip"

            HLSLPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma vertex VertDefault
                #pragma fragment FragVarianceClip

            ENDHLSL
        }
    }
}
