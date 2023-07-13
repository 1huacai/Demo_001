Shader "YoukiaEngine/Effect/YoukiaDepth"
{
    Properties
    {
        [HDR]_Color ("颜色1 (RGB: color, A: Alpha)", Color) = (1, 1, 1, 1)
        [HDR]_ColorDepth ("颜色2 (RGB: color, A: Alpha)", Color) = (1, 1, 1, 1)
        _DepthFade ("深度过渡", Range(0.001, 1)) = 1
        [Header(Noise)]
        _MainTex ("噪声纹理", 2D) = "black" {}
        _NoiseSpeedU ("噪声流动速度 U", float) = 0
        _NoiseSpeedV ("噪声流动速度 V", float) = 0
        _NoiseStrength ("噪声强度", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IngoreProjector"="True" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha 
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ _UNITY_RENDER

            #include "UnityCG.cginc"
            #include "../Library/YoukiaCommon.cginc"
            #include "../Library/YoukiaMrt.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 proj : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            half _DepthFade;
            float _NoiseSpeedU, _NoiseSpeedV;
            half _NoiseStrength;

            v2f vert (appdata v)
            {
                v2f o;
                float2 uvSpeed = float2(_NoiseSpeedU, _NoiseSpeedV) * _Time.y;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex) + uvSpeed;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.proj = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.proj.z);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half noise = tex2D(_MainTex, i.texcoord).r;
                noise = lerp(1, noise, _NoiseStrength);

                float sceneZ = LinearEyeDepth(YoukiaDepthPROJ(i.proj));
                half partZ = i.proj.z;
                half fade = saturate(_DepthFade * (sceneZ - partZ)) * noise;

                half4 col = lerp(_Color, _ColorDepth, fade) * fade;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
