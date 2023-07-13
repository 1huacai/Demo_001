Shader "YoukiaEngine/Environment/MenPai/Fog"
{
    Properties
    {
        [HDR]_Color ("颜色1 (RGB: color, A: Alpha)", Color) = (1, 1, 1, 1)
        [HDR]_ColorDepth ("颜色2 (RGB: color, A: Alpha)", Color) = (1, 1, 1, 1)
        _FogThickness("迷雾厚度", Range(0, 0.5)) = 0
        _DepthFade ("深度过度-动画控制", Range(0.001, 1)) = 1
        [Header(Hole)]
        _HoleMask("挖洞Mask", 2D) = "black" {}
        _RimFade("边缘过度", Float) = 1
        _uvScale("UV缩放", Range(0, 0.01)) = 0.005
        _xOffset("xoffset", Float) = 0
        _yOffset("yoffset", Float) = 0
        //[Header(Noise)]
        //_MainTex ("噪声纹理", 2D) = "black" {}
        //_NoiseSpeedU ("噪声流动速度 U", float) = 0
        //_NoiseSpeedV ("噪声流动速度 V", float) = 0
        //_NoiseStrength ("噪声强度", Range(0, 1)) = 0

        [Header(Parallax)]
        _CloudTex("视差高度图(alpha通道)", 2D) = "white" {}
        _H("高度", Range(0,1)) = 0
        _HeightSpeed("Speed", Vector) = (0,0,0,0)
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
            #include "../Library/YoukiaTools.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 color : COLOR;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 proj : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float vertexColor : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            half _FogThickness;
            half _DepthFade;
            //float _NoiseSpeedU, _NoiseSpeedV;
            //half _NoiseStrength;
            sampler2D _HoleMask;
            half4 _HoleMask_ST;
            half _RimFade;
            half _uvScale;
            half _xOffset;
            half _yOffset;

            sampler2D _CloudTex;
            float4 _CloudTex_ST;
            half _H;
            half4 _HeightSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                //float2 uvSpeed = float2(_NoiseSpeedU, _NoiseSpeedV) * _Time.y;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);// + uvSpeed;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertexColor = v.color;

                o.proj = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.proj.z);
                
                return o;
            }

            FragmentOutput frag (v2f i) : SV_Target
            {
                half2 uvScreen = i.proj.xy / i.proj.w;
                float depth = YoukiaDepthPROJ(i.proj);
                float3 wsPos = GetWorldPositionFromLinearDepthValue(uvScreen, Linear01Depth(depth)).xyz;

                //noise
                //half noise = tex2D(_MainTex, i.texcoord - frac(_Time.y * _HeightSpeed.zw)).r;
                //noise = lerp(1, noise, _NoiseStrength);
                //return half4(noise.xxx, 1);

                //hole
                half2 maskUV = half2((wsPos.x + _xOffset) * _uvScale, (wsPos.z + _yOffset) * _uvScale);
                half holeArea = 1 - tex2D(_HoleMask, maskUV).r;

                holeArea = saturate(pow(holeArea.xxx * 2, _RimFade));


                //parallax
                half height = tex2D(_CloudTex, TRANSFORM_TEX(i.texcoord, _CloudTex)).a;
                half h = _H;
                half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                half2 parallaxUV = ParallaxOffset(_H, height, viewDir);
                
                half c1 = tex2D(_CloudTex, parallaxUV + frac(_Time.y * _HeightSpeed.xy)).a;
                half c2 = tex2D(_CloudTex, parallaxUV - frac(_Time.y * _HeightSpeed.zw)).a;
                half c = c1 * c2;
                //return c;



                _DepthFade = (_DepthFade + _FogThickness)* (holeArea) * (c);

                float sceneZ = LinearEyeDepth(depth);
                half partZ = i.proj.z;
                half fade = saturate(_DepthFade * 0.25 * (sceneZ - partZ));


                half4 col = lerp(_Color, _ColorDepth, fade) * fade;

                return OutPutParticle(col);
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
