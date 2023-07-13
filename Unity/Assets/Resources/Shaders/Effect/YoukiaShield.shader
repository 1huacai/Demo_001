Shader "YoukiaEngine/Effect/YoukiaShield"
{
    Properties
    {
        _MainTex ("主贴图", 2D) = "white" {}
        [HDR]_MainColor("主颜色", Color) = (1, 1, 1, 1)
        [Header(Wave)]
        [HDR]_WaveContainColor("波纹颜色", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_WaveTex ("波纹贴图(黑白图，控制波纹边缘过渡)", 2D) = "white" {}

        [Space(10)]
        [Header(Distort)]
        _NoiseTex("扰动贴图", 2D) = "white"{}
        _WaveDistortContain("波中内容扭曲", Range(0, 1)) = 0
        _WaveDistort("波纹扭曲", Range(0, 1)) = 0

        [Space(10)]
        [Header(Mask)]
        [Toggle(_USE_MASK)]_Toggle_USE_MASK_ON("遮罩开关", float) = 0
        _MaskTex("遮罩贴图", 2D) = "white"{}
        _UV_Speed_U_Mask ("纹理U方向速度", Range(-1, 1)) = 0
		_UV_Speed_V_Mask ("纹理V方向速度", Range(-1, 1)) = 0
		[HDR]_MaskTexColor ("颜色", Color) = (0, 0, 0, 1)

        [Space(10)]
        [Header(Rim)]
        [HDR]_RimColor("边缘光颜色", Color) = (1, 1, 1, 1)
        _RimPower("边缘光范围", Range(0, 10)) = 4

        [Space(10)]
        [Header(Alpha)]
        _AlphaTotal("整体透明度", Range(0, 1)) = 1
        _AlphaFresnelPower("中心透明度", Range(0, 24)) = 1

        [Header(Soft Particle)]
        [Toggle(_USE_SOFTPARTICLE)]_Toggle_USE_SOFTPARTICLE_ON("软粒子", float) = 0
        _SoftFade ("柔和度", Range(0.01, 3)) = 1.0

        [Space(30)]
		[Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		//[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        LOD 100

        CGINCLUDE
            #include "UnityCG.cginc"
            #include "Assets/Resources/Shaders/Library/YoukiaEffect.cginc"

            #pragma shader_feature _USE_MASK
            #pragma shader_feature _USE_SOFTPARTICLE
            #pragma multi_compile __ _UNITY_RENDER

            struct appdata
            {
                float4 vertex : POSITION;
                half3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                // float4 hitDistance : TEXCOORD1;
                half3 normalDir : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float2 noiseUV : TEXCOORD4;

                #if _USE_SOFTPARTICLE
                    half4 projPos : TEXCOORD5;
                #endif
            };

            float4 _HitPoint1;
            float4 _HitInfo1;
            float4 _HitPoint2;
            float4 _HitInfo2;
            float4 _HitPoint3;
            float4 _HitInfo3;
            float4 _HitPoint4;
            float4 _HitInfo4;

            half4 _MainColor;

            sampler2D _WaveTex;
            half4 _WaveContainColor;

            half _RimPower;
            half4 _RimColor;

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            half _WaveDistortContain;
            half _WaveDistort;
            
            half _AlphaTotal;
            half _AlphaFresnelPower;

            half _UV_Speed_U_Mask, _UV_Speed_V_Mask;
            half4 _MaskTexColor;

            inline half2 UVSpeed(half speedU, half speedV)
            {
                half2 uvSpeed = _Time.y * (half2(speedU, speedV));
                return uvSpeed;
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normalDir = UnityObjectToWorldNormal(v.normal);
               
                o.noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);

                #if _USE_MASK
                    o.uv.zw = TRANSFORM_TEX(v.uv, _MaskTex);
                    half2 uvMaskSpeed = UVSpeed(_UV_Speed_U_Mask, _UV_Speed_V_Mask);
                    o.uv.zw += uvMaskSpeed;
                #endif

                #if _USE_SOFTPARTICLE
                    o.projPos = ComputeScreenPos(o.vertex);
                    COMPUTE_EYEDEPTH(o.projPos.z);
                #endif
                return o;
            }

            half Wave (half disturb, half distance, float4 hitInfo)
            {
                disturb *= _WaveDistort;
                distance += disturb;
                half dis = max(distance - hitInfo.x, 0);
                dis = dis > hitInfo.y ? 0 : dis;
                half wave = saturate(dis / hitInfo.y);
                half2 uv = half2(wave, 0);
                wave = tex2D(_WaveTex, uv).r;

                return wave;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //wave
                half4 hitDistance = half4(distance(i.worldPos.xyz, _HitPoint1.xyz), distance(i.worldPos.xyz, _HitPoint2.xyz), distance(i.worldPos.xyz, _HitPoint3.xyz), distance(i.worldPos.xyz, _HitPoint4.xyz));

                half4 noiseColor = tex2D(_NoiseTex, i.noiseUV);
                half4 waveRange = half4(Wave(noiseColor.r, hitDistance.x, _HitInfo1),
                                        Wave(noiseColor.r, hitDistance.y, _HitInfo2),
                                        Wave(noiseColor.r, hitDistance.z, _HitInfo3),
                                        Wave(noiseColor.r, hitDistance.w, _HitInfo4));

                half4 waveStrength = waveRange * half4(_HitInfo1.z, _HitInfo2.z, _HitInfo3.z, _HitInfo4.z);
                half waveRangeTotal = saturate(dot(waveRange, half4(1, 1, 1, 1)));
                half waveStrengthTotal = saturate(dot(waveStrength, half4(1, 1, 1, 1)));

                fixed4 mainTexColor = tex2D(_MainTex, i.uv + noiseColor.r * _WaveDistortContain * waveRangeTotal * waveStrengthTotal);

                //Rim
                half3 normalDir = normalize(i.normalDir);
                half3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                half NdotV = 1 - abs(dot(normalDir, viewDir));
                half rimFactor = pow(NdotV, _RimPower);
                half3 rimColor = _RimColor * rimFactor;
                half alphaFresnel = pow(NdotV, _AlphaFresnelPower);

                fixed4 finalColor = 1;
                finalColor.rgb = mainTexColor.r * lerp(_MainColor, _WaveContainColor, saturate(waveRangeTotal * waveStrengthTotal)) + rimColor;
                finalColor.a = lerp(lerp(mainTexColor.r * _AlphaTotal * alphaFresnel, mainTexColor.r, saturate(waveRangeTotal * waveStrengthTotal)), 1, saturate(rimFactor));
                finalColor.a = saturate(finalColor.a);
                
                #if _USE_MASK
                    //Mask
                    fixed4 mask = tex2D(_MaskTex, i.uv.zw);
                    fixed4 alphaMask = mask * _MaskTexColor;
                    finalColor *= alphaMask;
                #endif

                #if _USE_SOFTPARTICLE
                    finalColor.a *= SoftParticle(i.projPos);
                #endif

                return finalColor;
            }
        ENDCG

        Pass
        {
            Cull Off
            Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
            ZWrite[_zWrite]
            ZTest[_zTest]
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
