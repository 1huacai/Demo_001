// ref: https://zhuanlan.zhihu.com/p/248406797
// 体积云

Shader "Hidden/PostProcessing/YoukiaVolumetricClouds"
{
    HLSLINCLUDE
    

        #include "../StdLib.hlsl"
        #include "../Youkia.hlsl"
        #include "../MRT.hlsl"
        #pragma multi_compile __ _UNITY_RENDER

        #define _LIGHTMARCH_COUNT 8
        #define _LIGHTMARCH_STEP 10

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;

        // down sample 
        TEXTURE2D_SAMPLER2D(_CloudsDownSampleDepthTex, sampler_CloudsDownSampleDepthTex);
        TEXTURE2D_SAMPLER2D(_CloudsDownSampleColor, sampler_CloudsDownSampleColor);

        float3 _CloudsBoundsMin, _CloudsBoundsMax;

        half _CloudsStep;
        half _CloudsRayStepCount;
        half _CloudsRayStepLength;
        
        TEXTURE3D_SAMPLER3D(_CloudsNoiseTex, sampler_CloudsNoiseTex);
        half _CloudsNoiseTexScale;
        half3 _CloudsNoiseSpeed;

        // shade
        // light
        half _LightAbsorptionTowardSun, _LightAbsorptionThroughCloud;
        // color
        half4 _ColA, _ColB;
        half _ColorOffset1, _ColorOffset2;
        half _DarknessThreshold;

        // hg phase
        half4 _PhaseParams;

        // shape
        sampler2D _WeatherMap;
        half _HeightWeights;
        half4 _ShapeNoiseWeights;
        half _DensityOffset;
        half _EdgeFadeDst;
        half _DensityMultiplier;

        // detail
        TEXTURE3D_SAMPLER3D(_CloudsNoiseTexDetail, sampler_CloudsNoiseTexDetail);
        half _CloudsNoiseDetailTexScale;
        half3 _CloudsNoiseDetailSpeed;
        half _ShapeNoiseDetailWeights;
        half _NoiseDetailWeight;

        // blue noids
        sampler2D _CloudsBlueNoise;
        float4 _CloudsBlueNoiseCoords;
        half _CloudsBlueNoiseStrength;

        // 包围盒计算
        // nvdia改进算法: http://jcgt.org/published/0007/03/04/
        // bounds min value
        // bounds max value
        // camera position
        // inv ray dir
        float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOri, float3 invRayDir)
        {
            float3 t0 = (boundsMin - rayOri) * invRayDir;
            float3 t1 = (boundsMax - rayOri) * invRayDir;
            float3 tmin = min(t0, t1);
            float3 tmax = max(t0, t1);

            float dstA = max(max(tmin.x, tmin.y), tmin.z); //进入点
            float dstB = min(tmax.x, min(tmax.y, tmax.z)); //出去点

            // 相机到容器的距离
            float dstToBox = max(0, dstA);
            // 返回ray是否在容器中
            float dstInsideBox = max(0, dstB - dstToBox);

            return float2(dstToBox, dstInsideBox);
        }

        // 使用 HG phase function 来代替复杂的米氏散射
        float hg(float a, float g) 
        {
            float g2 = g * g;
            return (1 - g2) / (4 * 3.1415 * pow(1 + g2 - 2 * g * (a), 1.5));
        }

        float phase(float a) 
        {
            float blend = 0.5;
            float hgBlend = hg(a, _PhaseParams.x) * (1 - blend) + hg(a, -_PhaseParams.y) * blend;
            return _PhaseParams.z + hgBlend * _PhaseParams.w;
        }

        float remap(float original_value, float original_min, float original_max, float new_min, float new_max)
        {
            return new_min + (((original_value - original_min) / (original_max - original_min)) * (new_max - new_min));
        }

        // sample 3d tex
        float sampleDensity(float3 rayPos)
        {
            float3 boundsCentre = (_CloudsBoundsMin + _CloudsBoundsMax) * 0.5;
            float3 size = _CloudsBoundsMax - _CloudsBoundsMin;

            // uv
            float2 uv = (size.xz * 0.5f + (rayPos.xz - boundsCentre.xz)) / max(size.x, size.z);
        
            // weather map
            float4 weatherMap = tex2D(_WeatherMap, uv + _Time.x * _CloudsNoiseSpeed.xz);

            // soft bottom
            float gMin = remap(weatherMap.x, 0, 1, 0.1, 0.6);
            float heightPercent = (rayPos.y - _CloudsBoundsMin.y) / size.y;

            float heightGradient = saturate(remap(heightPercent, 0.0, weatherMap.r, 1, 0)) * saturate(remap(heightPercent, 0.0, gMin, 0, 1));

            // edge fade
            float dstEdgeX = min(_EdgeFadeDst, min(rayPos.x - _CloudsBoundsMin.x, _CloudsBoundsMax.x - rayPos.x));
            float dstEdgeZ = min(_EdgeFadeDst, min(rayPos.z - _CloudsBoundsMin.z, _CloudsBoundsMax.z - rayPos.z));
            float edgeWeight = min(dstEdgeZ, dstEdgeX) / _EdgeFadeDst;
            heightGradient *= edgeWeight;

            float3 uvw = rayPos * _CloudsNoiseTexScale;
            uvw += _Time.x * _CloudsNoiseSpeed * 0.5f;
            float4 noise = SAMPLE_TEXTURE3D(_CloudsNoiseTex, sampler_CloudsNoiseTex, uvw);

            float4 normalizeShapeWeights = _ShapeNoiseWeights / dot(_ShapeNoiseWeights, 1);
            float shapeFBM = dot(noise, normalizeShapeWeights) * heightGradient;
            float baseShapeDensity = shapeFBM + _DensityOffset * 0.01f;
            // return baseShapeDensity;

            // detail
            if (baseShapeDensity > 0)
            {
                float3 uvwDetail = rayPos * _CloudsNoiseDetailTexScale;
                uvwDetail += _Time.x * _CloudsNoiseDetailSpeed;
                float4 detailNoise = SAMPLE_TEXTURE3D(_CloudsNoiseTexDetail, sampler_CloudsNoiseTexDetail, uvwDetail);
                float detailFBM = pow(detailNoise.r, _ShapeNoiseDetailWeights);
                float oneMinusShape = 1 - baseShapeDensity;
                float detailErodeWeight = oneMinusShape * oneMinusShape * oneMinusShape;
                float cloudDensity = baseShapeDensity - detailFBM * detailErodeWeight * _NoiseDetailWeight;

                return saturate(cloudDensity * _DensityMultiplier);
            }

            return 0;
        }

        // light march
        float3 lightmarch(float3 position)
        {
            float3 dir2Light = _WorldSpaceLightPos0.xyz;
            float dstInsideBox = rayBoxDst(_CloudsBoundsMin, _CloudsBoundsMax, position, 1 / dir2Light).y;
            float stepSize = dstInsideBox / _LIGHTMARCH_STEP;
            float sum = 0;

            // march
            for (int i = 0; i < _LIGHTMARCH_COUNT; i++)
            {
                position += dir2Light * stepSize;
                sum += max(0, sampleDensity(position) * stepSize);
            }

            float transmittance = exp(-sum * _LightAbsorptionTowardSun);

            // 将亮->暗映射为 3段颜色, 亮->灯光颜色 中->ColorA 暗->ColorB
            float3 cloudColor = lerp(_ColA, _LightColor0, saturate(transmittance * _ColorOffset1));
            cloudColor = lerp(_ColB, cloudColor, saturate(pow(transmittance * _ColorOffset2, 3)));
            
            float3 lightTransmittance;
            lightTransmittance.xyz = _DarknessThreshold + transmittance * (1 - _DarknessThreshold) * cloudColor;
            return lightTransmittance;
        }

        float4 CloudRayMarching(float dstLimit, float3 entryPoint, float3 rayDir, float stepSize, float phaseVal, half blueNoise)
        {
            float sum = 1;
            float dstTravelled = blueNoise * _CloudsBlueNoiseStrength;
            float3 lightEnergy = 0;
            [unroll(32)]
            for (int i = 0; i < _CloudsRayStepCount; i++)
            {
                if (dstTravelled < dstLimit)
                {
                    float3 rayPos = entryPoint + (rayDir * dstTravelled * _CloudsRayStepLength);
                    float density = sampleDensity(rayPos);
                    if (density > 0)
                    {
                        // sum += pow(density, _CloudsNoisePow);
                        float3 lightTransmittance = lightmarch(rayPos);
                        lightEnergy += density * stepSize * lightTransmittance * sum * phaseVal;
                        sum *= exp(-density * stepSize * _LightAbsorptionThroughCloud);
                        if (sum < 0.01f)
                            break;

                    }
                }
                dstTravelled += stepSize;
            }

            sum = saturate(sum);
            lightEnergy = saturate(lightEnergy);
            return float4(lightEnergy, sum);
        }

        float4 Frag(VaryingsDefault i) : SV_Target
        {   
            // down sample 
            float depth = SAMPLE_DEPTH_TEXTURE(_CloudsDownSampleDepthTex, sampler_CloudsDownSampleDepthTex, i.texcoordStereo);
            float linearDepth = LinearEyeDepth(depth);
            depth = Linear01Depth(depth);

            float4 color = SAMPLE_TEXTURE2D(_CloudsDownSampleColor, sampler_CloudsDownSampleColor, i.texcoord);

            // world pos
            float3 wsPos = GetWorldPositionFromLinearDepthValue(i.texcoord, depth);
            // ray dir
            float3 rayDir = normalize(wsPos - _WorldSpaceCameraPos.xyz);
            float depthEyeLinear = length(wsPos - _WorldSpaceCameraPos.xyz);

            float2 rayInfo = rayBoxDst(_CloudsBoundsMin, _CloudsBoundsMax, _WorldSpaceCameraPos.xyz, 1 / rayDir);
            float dst2Box = rayInfo.x;
            float dstInsideBox = rayInfo.y;

            float dstLimit = min(depthEyeLinear - dst2Box, dstInsideBox);

            // 射线入点
            float3 entryPoint = _WorldSpaceCameraPos.xyz + rayDir * dst2Box;

            // 散射
            float rdotl = dot(rayDir, _WorldSpaceLightPos0.xyz);
            float3 phaseVal = phase(rdotl);

            // blue noise
            float blueNoise = tex2D(_CloudsBlueNoise, i.texcoord * _CloudsBlueNoiseCoords.xy + _CloudsBlueNoiseCoords.zw).r;

            // ray marching
            float stepSize = exp(_CloudsStep) * _CloudsRayStepLength;
            float4 cloud = CloudRayMarching(dstLimit, entryPoint, rayDir, stepSize, phaseVal, blueNoise);
            // return float4(cloud, 1);
            // color.rgb *= cloud.a;
            // color.rgb += cloud.rgb;
            return cloud;
        }

        // 深度降采样，用于优化降采样导致的边缘锯齿
        // ref: https://developer.nvidia.com/gpugems/gpugems3/part-iv-image-effects/chapter-23-high-speed-screen-particles
        float4 DownsampleDepth(VaryingsDefault i) : SV_Target
        {
            float2 texSize = 0.5f * YoukiaDepthTextureTexelSize().xy;

            float2 taps[4] = { 	
                float2(i.texcoord + float2(-1,-1) * texSize),
                float2(i.texcoord + float2(-1,1) * texSize),
                float2(i.texcoord + float2(1,-1) * texSize),
                float2(i.texcoord + float2(1,1) * texSize)};

            float depth1 = YoukiaDepth(taps[0]);
            float depth2 = YoukiaDepth(taps[1]);
            float depth3 = YoukiaDepth(taps[2]);
            float depth4 = YoukiaDepth(taps[3]);

            float result = min(depth1, min(depth2, min(depth3, depth4)));

            return result;
        }

        float4 FragCombine(VaryingsDefault i) : SV_Target
        {
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            float4 cloudsColor = SAMPLE_TEXTURE2D(_CloudsDownSampleColor, sampler_CloudsDownSampleColor, i.texcoord);

            color.rgb *= cloudsColor.a;
            color.rgb += cloudsColor.rgb;

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

        // down sample
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment DownsampleDepth

            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragCombine

            ENDHLSL
        }
    }
}
