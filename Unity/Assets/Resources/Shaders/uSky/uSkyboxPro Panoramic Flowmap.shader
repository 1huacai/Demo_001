// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "YoukiaEngine/uSkyPro/uSkyboxPro Panoramic Flowmap" 
{
    Properties 
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _Tex ("MainTex (HDR)", 2D) = "grey" {}

        [Space(5)]
		[Header(FlowMap)]
        _MaskTex("MaskTex", 2D) = "white" {}
		[KeywordEnum(None, F 1, F 4)] _Flowmap("flowmap", Float) = 0
        _FlowStrength ("流动强度(0: 不流动)", Range(0, 1)) = 1
		_Speed("流动速度", Range(-1, 1)) = 0.1
		[Space(5)]
		[Header(Bloom)]
		[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
		_BloomThreshold("BloomThreshold", Range(0 , 10)) = 5
		_BloomRange("BloomRange", Range(0 , 20)) = 0
    }

    SubShader 
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        // ColorMask RGB

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "../Library/YoukiaTools.cginc"
			#include "../Library/YoukiaEffect.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/YoukiaMrt.cginc"

            #pragma multi_compile _FLOWMAP_NONE _FLOWMAP_F_1 _FLOWMAP_F_4

            // youkia height fog
            #pragma multi_compile __ _HEIGHTFOG
            
            sampler2D _Tex;
            half4 _Tex_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;

            half _BloomThreshold, _BloomRange;

            struct appdata_t 
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
				float3 texcoord : TEXCOORD0;

                #ifdef _HEIGHTFOG
                    float4 worldPos : TEXCOORD1;
                #endif

				float2 image180ScaleAndCutoff : TEXCOORD3;
            	float4 layout3DScaleAndOffset : TEXCOORD4;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegreesCube(v.vertex, _Rotation);
                o.pos = UnityObjectToClipPos(rotated);
				o.texcoord = v.vertex.xyz;

				o.image180ScaleAndCutoff = float2(1.0, 1.0);
				o.layout3DScaleAndOffset = float4(0, 0, 1, 1);

                #ifdef _HEIGHTFOG
                    o.worldPos = mul(unity_ObjectToWorld, rotated);
                #endif
                return o;
            }

            FragmentOutput frag(v2f i)
            {
                float2 tc = ToRadialCoords(i.texcoord);
                UNITY_BRANCH
				if (tc.x > i.image180ScaleAndCutoff[1])
					return OutPutDefault(half4(0, 0, 0, 1));

				tc.x = fmod(tc.x*i.image180ScaleAndCutoff[0], 1);
				tc = (tc + i.layout3DScaleAndOffset.xy) * i.layout3DScaleAndOffset.zw;

                half3 c = Flowmap(_Tex, _Tex_HDR, tc);

                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;

                half lum = Luminance(c.rgb);
				float BloomRange = lerp(lum, _BloomRange, lum);
				c.rgb = _Exposure * c.rgb + BloomRange * _BloomThreshold * c.rgb;
				c.rgb = max(0, c.rgb);

                // height fog
                #ifdef _HEIGHTFOG
                    // height fog
                    half fog = 0;
                    half3 heightfog = YoukiaHeightFog(i.worldPos, 1, fog);
                    c.rgb = lerp(c.rgb, heightfog, fog);
                #endif

                // 环境亮度
				c = EnvirLumChange(c);

                return OutPutDefault(half4(c, 1));
            }
            ENDCG
        }
    }

    Fallback "YoukiaEngine/uSkyPro/uSkyboxPro"
}
