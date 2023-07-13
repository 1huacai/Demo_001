Shader "Hidden/PostProcessing/YoukiaBeforeTransparent"
{
    HLSLINCLUDE

        #include "../StdLib.hlsl"
        #include "../Sampling.hlsl"
        #include "HeightFog.hlsl"

        #pragma multi_compile __ _PP_HEIGHTFOG
        #pragma multi_compile __ _HEIGHTFOG_NOISE
        // #pragma multi_compile __ _PP_SSS
        #pragma multi_compile __ _PP_FOGOFWAR
        #pragma multi_compile __ _PP_SSAO
        #pragma multi_compile __ _UNITY_RENDER

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;

        TEXTURE2D_SAMPLER2D(_HeightFogTex, sampler_HeightFogTex);
        float4 _HeightFogTex_TexelSize;
        
        // TEXTURE2D_SAMPLER2D(_SkinSSSTex, sampler_SkinSSSTex);

        // fog of war
        TEXTURE2D_SAMPLER2D(_gFowTex, sampler_gFowTex);
        float4 _gFowTex_TexelSize;
        half4 _FowColor;

        // ssao 
        TEXTURE2D_SAMPLER2D(_SSAOTex, sampler_SSAOTex);
        float4 _SSAOTex_TexelSize;
        half4 _SSAOColor;

        half4 Frag(VaryingsDefault i) : SV_Target
        {
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

            #if _PP_SSAO
                half3 ssao = SAMPLE_TEXTURE2D(_SSAOTex, sampler_SSAOTex, i.texcoord);
                // half3 ssao = UpsampleBox(TEXTURE2D_PARAM(_SSAOTex, sampler_SSAOTex), i.texcoord, _SSAOTex_TexelSize.xy, 0.5f);
                color.rgb *= lerp(_SSAOColor, 1, ssao.r);
            #endif

            // // skin
            // #if _PP_SSS
            //     half4 skin = SAMPLE_TEXTURE2D(_SkinSSSTex, sampler_SkinSSSTex, i.texcoord);
            //     color.rgb = lerp(color.rgb, skin.rgb, FastSign(color.a - 1));
            // #endif

            // post fog
            #if _PP_HEIGHTFOG
                // half4 fog = SAMPLE_TEXTURE2D(_HeightFogTex, sampler_HeightFogTex, i.texcoord);
                // half4 fog = UpsampleBox(TEXTURE2D_PARAM(_HeightFogTex, sampler_HeightFogTex), i.texcoord, _HeightFogTex_TexelSize.xy, 0.5f);
                half4 fog = HeightFog(i.texcoord);
                color.rgb = lerp(color.rgb, fog.rgb, fog.a);
            #endif
            
            // fog of war
            #if _PP_FOGOFWAR
                // x: fog, y: fow
                // half4 fow = UpsampleBox(TEXTURE2D_PARAM(_gFowTex, sampler_gFowTex), uvDistorted, _gFowTex_TexelSize.xy, 0.5f);
                half4 fow = SAMPLE_TEXTURE2D(_gFowTex, sampler_gFowTex, i.texcoord);
                half fogOfWar = lerp(0, fow.x * _FowColor.a, fow.y);
                color.rgb = lerp(color.rgb, _FowColor.rgb, fogOfWar);

                // particleMask = lerp(particleMask, 0, fow.y);
                // characterMask = lerp(characterMask, 0, fow.y);
            #endif

            color.a = 0;
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
                #pragma fragmentoption ARB_precision_hint_fastest

            ENDHLSL
        }
    }
}
