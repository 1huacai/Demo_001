Shader "Hidden/PostProcessing/YoukiaFogOfWar"
{
    HLSLINCLUDE

        #include "HeightFog.hlsl"

        #pragma multi_compile __ _UNITY_RENDER
        #pragma multi_compile __ _YOUKIA_REVERSED_Z

        half4 _sampleOffset;
        float4 _gFowMask_TexelSize;
        TEXTURE2D_SAMPLER2D(_gFowMask, sampler_gFowMask);

        // x: intensity, y: height, z: fall off, w: guid scene
        float4 _FowParam;
        half4 _FowColor;

        // xyz: pos, w: size
        float4 _gFowCamPos;
         
        half4 Frag(VaryingsDefault i) : SV_Target
        {
            float depth = YoukiaDepth(i.texcoord);
            
            #if _YOUKIA_REVERSED_Z
            #else
                depth = depth * 2 - 1;
            #endif

            float3 wsPos = GetWorldPositionFromDepthValue(i.texcoord, depth).xyz;
            half fog = WarFog(_FowParam.x, _FowParam.y, _FowParam.z, wsPos, _FowParam.w);

            half2 fowUV = saturate(UVOrthographicWithOffset(wsPos, _gFowCamPos.xyz, _gFowCamPos.w, _gFowMask_TexelSize.xy, _sampleOffset.x, _sampleOffset.y));

            half2 fogMask = SAMPLE_TEXTURE2D(_gFowMask, sampler_gFowMask, fowUV).rg;
            half fow = 1 - fogMask.y;

            return half4(fog, fow, 0, 0);
        }

    ENDHLSL 

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        
        Pass
        {
            HLSLPROGRAM
                #pragma fragmentoption ARB_precision_hint_fastest

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}
