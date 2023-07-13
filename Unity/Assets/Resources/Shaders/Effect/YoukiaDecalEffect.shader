//@@@DynamicShaderInfoStart
//贴花特效 注意控制使用个数-贴花存在额外建立世界坐标系等运算开销
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Effect/YoukiaDecalEffect"
{
    Properties
    {
        [HDR]_Color ("颜色", Color) = (1, 1, 1, 1)
		_Alpha ("Alpha", Range(0, 5)) = 1
        _MainTex ("Texture", 2D) = "white" {}

        // 极坐标
		[Header(UV Polar)]
		[MaterialToggle]_Polar("uv 极坐标(中心方向)", float) = 0
        [MaterialToggle]_PolarUVFlip ("极坐标 uv 翻转", float) = 0

        [Header(Speed)]
        _UVSpeed_U ("UV Speed U", float) = 0
        _UVSpeed_V ("UV Speed V", float) = 0

        // mask
        [Header(Mask)]
        // [Toggle(_USE_MASK)]_USE_MASK("遮罩纹理", float) = 0
        _MaskTex ("Mask Texture", 2D) = "white" {}

        // character mask
        // [Toggle(_USE_CHARACTER_MASK)]_USE_CHARACTER_MASK("角色剔除", float) = 0

        // noise
        [Header(Noise)]
        // [Toggle(_USE_NOISE)]_Toggle_USE_NOISE_ON("扭曲", float) = 0
        _NoiseTex ("Noise Texture", 2D) = "black" {}
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0
        // 极坐标
		[Header(UV Polar)]
		_Noise_Polar("noise uv 极坐标(中心方向)", float) = 0
        // speed
        _Noise_UVSpeed_U ("Noise UV Speed U", float) = 0
        _Noise_UVSpeed_V ("Noise UV Speed V", float) = 0

        [Space(30)]
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Stencil
        {
            Ref 1
            Comp Greater
        }

        Pass
        {
            Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite Off
			ZTest Always
			Cull front
			Lighting Off

            CGPROGRAM
                #pragma multi_compile_instancing
                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _YOUKIA_REVERSED_Z
                #pragma shader_feature _USE_MASK
                // #pragma shader_feature _USE_CHARACTER_MASK
                #pragma shader_feature _USE_NOISE

                #include "UnityCG.cginc"
                #include "../Library/YoukiaCommon.cginc"
                #include "../Library/YoukiaMrt.cginc"
                #include "../Library/YoukiaTools.cginc"
                #include "../Library/YoukiaDecal.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float4 proj : TEXCOORD1;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                float _UVSpeed_V;
                float _UVSpeed_U;

                half _UVAngle;
                half _Polar;
                half _Alpha;
                half _PolarUVFlip;

                sampler2D _NoiseTex;
                half4 _NoiseTex_ST;
                half _NoiseStrength;
                half _Noise_Polar;
                float _Noise_UVSpeed_U;
                float _Noise_UVSpeed_V;

                v2f vert (appdata v)
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.proj = ComputeScreenPos(o.pos);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(i);
                    float2 screenUV = i.proj.xy / i.proj.w;
                    float depth = YoukiaDepth(screenUV);

                    #if _YOUKIA_REVERSED_Z
                    #else
                        depth = depth * 2 - 1;
                    #endif

                    float3 wsPos = GetWorldPositionFromDepthValue(screenUV, depth).xyz;
                    half2 uv = DecalUV(wsPos);
                    
                    half4 noise = 0;
                    #if _USE_NOISE
                        half2 uvNoisePolar = UVPolar(uv, _NoiseTex_ST.zw, _NoiseTex_ST.x, _NoiseTex_ST.y);
                        half2 uvNoise = lerp(TRANSFORM_TEX(uv, _NoiseTex), uvNoisePolar, _Noise_Polar);
                        uvNoise -= _Time.y * float2(_Noise_UVSpeed_U, _Noise_UVSpeed_V);
                        noise = tex2D(_NoiseTex, uvNoise) * _NoiseStrength;
                        noise = noise.a * noise.r;
                    #endif

                    half2 uvPolar = UVPolar(uv, _MainTex_ST.zw, _MainTex_ST.x, _MainTex_ST.y);
                    uvPolar = lerp(uvPolar, uvPolar.yx, _PolarUVFlip);
                    half2 uvMain = lerp(TRANSFORM_TEX(uv, _MainTex), uvPolar, _Polar) + noise;
                    
                    uvMain -= _Time.y * float2(_UVSpeed_U, _UVSpeed_V);

                    fixed4 col = tex2D(_MainTex, uvMain) * _Color;
                    col.a = saturate(col.a * _Alpha);

                    #if _USE_MASK
                        half2 uvMask = TRANSFORM_TEX(uv, _MaskTex);
                        half4 mask = tex2D(_MaskTex, uvMask);
                        col *= mask;
                    #endif

                    return col;
                }
            ENDCG
        }
    }

    CustomEditor "YoukiaDecalEffectInspector"
}
