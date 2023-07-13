Shader "Hidden/PostProcessing/CopyDepth"
{
    HLSLINCLUDE
        #include "../StdLib.hlsl"

        TEXTURE2D_SAMPLER2DFLOAT(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;

        TEXTURE2D_SAMPLER2D(_gBufferOri_0, sampler_gBufferOri_0);

        half _gDepthDownSample;


        float4 Frag(VaryingsDefault i, out float outDepth : SV_Depth) : SV_Target
        {
            float depth = SAMPLE_DEPTH_TEXTURE(_MainTex, sampler_MainTex, i.texcoordStereo);

            outDepth = depth;
            return SAMPLE_TEXTURE2D(_gBufferOri_0, sampler_gBufferOri_0, i.texcoordStereo);
        }

        float4 FragKillNaN(VaryingsDefault i, out float outDepth : SV_Depth) : SV_Target
        {
            float depth = SAMPLE_DEPTH_TEXTURE(_MainTex, sampler_MainTex, i.texcoordStereo);

            if (IsNan(depth))
            {
                depth = (0.0);
            }

            outDepth = depth;
            return SAMPLE_TEXTURE2D(_gBufferOri_0, sampler_gBufferOri_0, i.texcoordStereo);
        }

        float4 FragDownSample(VaryingsDefault i, out float outDepth : SV_Depth) : SV_Target
        {
            float2 texSize = _gDepthDownSample * _MainTex_TexelSize.xy;

            float2 taps[4] = 
            { 	
                float2(i.texcoordStereo + float2(-1, -1) * texSize),
                float2(i.texcoordStereo + float2(-1, 1) * texSize),
                float2(i.texcoordStereo + float2(1, -1) * texSize),
                float2(i.texcoordStereo + float2(1, 1) * texSize)
            };

            float depth1 = SAMPLE_DEPTH_TEXTURE(_MainTex, sampler_MainTex, taps[0]);
            float depth2 = SAMPLE_DEPTH_TEXTURE(_MainTex, sampler_MainTex, taps[1]);
            float depth3 = SAMPLE_DEPTH_TEXTURE(_MainTex, sampler_MainTex, taps[2]);
            float depth4 = SAMPLE_DEPTH_TEXTURE(_MainTex, sampler_MainTex, taps[3]);

            #if UNITY_REVERSED_Z
                outDepth = 1.0f;
                outDepth = min(depth1, min(depth2, min(depth3, depth4)));
            #else
                outDepth = 0.0f;
                outDepth = max(depth1, max(depth2, max(depth3, depth4)));
            #endif

            return SAMPLE_TEXTURE2D(_gBufferOri_0, sampler_gBufferOri_0, i.texcoordStereo);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite On ZTest Always

        // 0 - Fullscreen triangle copy
        Pass
        {
            Name "Fullscreen Depth Copy"

            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }

        // 1 - Fullscreen triangle copy + NaN killer
        Pass
        {
            Name "Fullscreen NaN killer Depth Copy"

            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragKillNaN

            ENDHLSL
        }

        // 2 - DownSample triangle copy 
        Pass
        {
            Name "DownSample Depth Copy"

            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragDownSample

            ENDHLSL
        }
    }
}
