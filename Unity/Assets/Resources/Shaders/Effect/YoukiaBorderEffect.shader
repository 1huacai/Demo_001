//@@@DynamicShaderInfoStart
//边缘特效 注意控制使用个数-贴花存在额外建立世界坐标系等运算开销
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Effect/YoukiaBorderEffect"
{
    Properties
    {
        [HDR]_Color ("颜色", Color) = (1, 1, 1, 1)
		_Alpha ("Alpha", Range(0, 5)) = 1
        _MainTex ("Texture", 2D) = "white" {}

        [Header(Speed)]
        _UVSpeed_U ("UV Speed U", float) = 0
        _UVSpeed_V ("UV Speed V", float) = 0

        [Header(Fade)]
        _Height ("Height", Range(0.000001, 10)) = 1

        // mask
        [Header(Mask)]
        // [Toggle(_USE_MASK)]_USE_MASK("遮罩纹理", float) = 0
        _MaskTex ("Mask Texture", 2D) = "white" {}

        // noise
        [Header(Noise)]
        // [Toggle(_USE_NOISE)]_Toggle_USE_NOISE_ON("扭曲", float) = 0
        _NoiseTex ("Noise Texture", 2D) = "black" {}
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0
        // speed
        _Noise_UVSpeed_U ("Noise UV Speed U", float) = 0
        _Noise_UVSpeed_V ("Noise UV Speed V", float) = 0

        [Space(30)]
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite Off
			ZTest[_zTest]
			Cull [_cull]
			Lighting Off

            CGPROGRAM
                #pragma multi_compile_instancing
                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile __ _UNITY_RENDER
                #pragma multi_compile __ _YOUKIA_REVERSED_Z
                #pragma shader_feature _USE_MASK
                #pragma shader_feature _USE_NOISE

                #define SKYBOX 0.99

                #include "UnityCG.cginc"
                #include "../Library/YoukiaCommon.cginc"
                #include "../Library/YoukiaMrt.cginc"
                #include "../Library/YoukiaTools.cginc"
                #include "../Library/YoukiaDecal.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    half2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float4 texcoord : TEXCOORD0;
                    float2 texcoord1 : TEXCOORD1;
                    float4 proj : TEXCOORD2;
                    float4 wsPos : TEXCOORD3;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                float _UVSpeed_V;
                float _UVSpeed_U;

                half _Alpha;

                half _Height;
                half _HeightOffset;

                sampler2D _NoiseTex;
                half4 _NoiseTex_ST;
                half _NoiseStrength;
                float _Noise_UVSpeed_U;
                float _Noise_UVSpeed_V;

                v2f vert (appdata v)
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex, o.wsPos);
                    o.proj = ComputeScreenPos(o.pos);
                    o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex) - _Time.y * half2(_UVSpeed_U, _UVSpeed_V);
                    #if _USE_NOISE
                        o.texcoord.zw = TRANSFORM_TEX(v.texcoord, _NoiseTex) - _Time.y * half2(_Noise_UVSpeed_U, _Noise_UVSpeed_V);
                    #endif
                    #if _USE_MASK
                        o.texcoord1.xy = TRANSFORM_TEX(v.texcoord, _MaskTex);
                    #endif
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    float2 screenUV = i.proj.xy / i.proj.w;
                    float depth = YoukiaDepth(screenUV);
                    float depth01 = Linear01Depth(depth);

                    #if _YOUKIA_REVERSED_Z
                    #else
                        depth = depth * 2 - 1;
                    #endif

                    float3 wsPos = GetWorldPositionFromDepthValue(screenUV, depth).xyz;

                    float height = max(0, i.wsPos.y - wsPos.y);
                    half fade = exp(-_Height * height);

                    half skybox = depth01 > SKYBOX ? 1 : 0;
                    fade = lerp(fade, 0, skybox);

                    half4 noise = 0;
                    #if _USE_NOISE
                        half2 uvNoise = i.texcoord.zw;
                        uvNoise -= _Time.y * float2(_Noise_UVSpeed_U, _Noise_UVSpeed_V);
                        noise = tex2D(_NoiseTex, uvNoise) * _NoiseStrength;
                        noise = noise.a * noise.r;
                    #endif

                    half2 uvMain = i.texcoord.xy + noise;
                    fixed4 col = tex2D(_MainTex, uvMain) * _Color;
                    col.a = saturate(col.a * _Alpha * fade);

                    #if _USE_MASK
                        half2 uvMask = i.texcoord1;
                        half4 mask = tex2D(_MaskTex, uvMask);
                        col *= mask;
                    #endif

                    return col;
                }
            ENDCG
        }
    }

    CustomEditor "YoukiaBorderEffectInspector"
}
