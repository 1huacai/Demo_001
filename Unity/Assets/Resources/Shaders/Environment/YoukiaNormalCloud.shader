Shader "YoukiaEngine/Environment/MenPai/NormalCloud" 
{
    Properties 
    {
        _MainColor ("MainColor", Color) = (1,1,1,1)
        _CloudTexA ("Cloud Tex(A：不透明度)", 2D) = "white" {}
        _Noise ("Noise", 2D) = "white" {}
        _Speed1 ("Speed1", Float ) = 1
        _Speed2 ("Speed2", Float ) = 0
        _Opacity ("Opacity", Float ) = 0
        _OpacityPower ("Opacity Power", Range(0, 5)) = 1
        _Warp1 ("Warp1", Range(0, 1)) = 0
        _Warp2 ("Warp2", Range(0, 1)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

        [Header(Soft Particle)]
        [Toggle(_USE_SOFTPARTICLE)]_Toggle_USE_SOFTPARTICLE_ON("软粒子", float) = 0
        _SoftFade ("柔和度", Range(0.01, 3)) = 1.0

        [Header(FogOfWar)]
        [Toggle(Mp_CloudFog)]_CloudFog_Toggle ("迷雾 #是否受场景可见性影响", float) = 0
    }
    SubShader 
    {
        Tags 
        {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 100
        Pass 
        {
            Name "FORWARD"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ColorMask RGB
            
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "Assets/Resources/Shaders/Library/YoukiaEffect.cginc"
                #include "Assets/Resources/Shaders/Library/YoukiaEnvironment.cginc"

                #pragma multi_compile __ _UNITY_RENDER
                #pragma shader_feature _USE_SOFTPARTICLE
                #pragma shader_feature Mp_CloudFog

                const static half CONST1 = 1.3f;
                const static half2 CONST2 = half2(0.02, 0.005);
                const static half2 CONST3 = half2(0.02, -0.007);

                sampler2D _Noise; 
                half4 _Noise_ST;
                sampler2D _CloudTexA; 
                half4 _CloudTexA_ST;
                half _Speed1;
                half4 _MainColor;
                half _OpacityPower;
                half _Warp2;
                half _Warp1;
                half _Speed2;
                half _Opacity;

                struct VertexInput 
                {
                    float4 vertex : POSITION;
                    half2 texcoord0 : TEXCOORD0;
                };
                struct VertexOutput 
                {
                    float4 pos : SV_POSITION;
                    half2 uv0 : TEXCOORD0;
                    float3 worldPos : TEXCOORD2;
                    #if (defined(_USE_SOFTPARTICLE))
                        half4 projPos : TEXCOORD3;
                    #endif
                };

                VertexOutput vert (VertexInput v) 
                {
                    VertexOutput o = (VertexOutput)0;
                    o.uv0 = v.texcoord0;
                    o.pos = UnityObjectToClipPos(v.vertex );
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    UNITY_TRANSFER_FOG(o,o.pos);

                    #if _USE_SOFTPARTICLE
                        o.projPos = ComputeScreenPos(o.pos);
                        COMPUTE_EYEDEPTH(o.projPos.z);
                    #endif
                    
                    return o;
                }
                
                half4 frag(VertexOutput i) : COLOR 
                {
                    float speed1 = (_Time.g * _Speed1);

                    half noise0 = tex2D(_Noise, TRANSFORM_TEX(i.uv0 + speed1 * CONST2, _Noise)).r;
                    half noise1 = tex2D(_Noise, TRANSFORM_TEX(i.uv0 * CONST1 + speed1 * CONST3, _Noise)).r;

                    half2 uvOffset0 = half2(noise0, noise1);
                    half4 cloud = tex2D(_CloudTexA, TRANSFORM_TEX(i.uv0 + (uvOffset0 * _Warp1), _CloudTexA));
                    half3 finalColor = (_MainColor.rgb * cloud.rgb);

                    float speed2 = (_Time.g * _Speed2);
                    half2 uvOffset1 = i.uv0 + (uvOffset0 * _Warp2);

                    half noise2 = tex2D(_Noise, TRANSFORM_TEX(uvOffset1 + speed2 * CONST2, _Noise)).r;
                    half noise3 = tex2D(_Noise, TRANSFORM_TEX(uvOffset1 * CONST1 + speed2 * CONST3, _Noise)).r;

                    fixed4 finalRGBA = fixed4(finalColor, saturate((cloud.a * pow((noise2 + noise3 + _Opacity), _OpacityPower))));

                    #if _USE_SOFTPARTICLE
                        finalRGBA.a *= SoftParticle(i.projPos);
                    #endif

                    half alpha = finalRGBA.a;
                    #ifdef Mp_CloudFog
                        half2 uv = UVOrthographic(i.worldPos, _gFowCamPos.xyz, _gFowCamPos.w);
                        alpha *= tex2D(_gFowMask, uv + (noise3 + noise2) * 0.015).r;
                    #endif

                    // 环境亮度
                    finalRGBA.rgb = EnvirLumChange(finalRGBA.rgb);

                    return fixed4(finalRGBA.rgb, alpha);
                }
            ENDCG
        }
    }
}
