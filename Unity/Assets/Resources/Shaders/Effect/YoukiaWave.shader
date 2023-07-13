Shader "YoukiaEngine/Effect/YoukiaWave"
{
    Properties
    {
        [HDR]_ColorTop("ColorTop", Color) = (1,1,1,1)
        [HDR]_MidColor("MidColor", Color) = (0.5, 0.5, 0.5, 1)
        [HDR]_ButtomColor0("ButtomColor0", Color) = (1,1,1,1)
        [HDR]_ButtomColor1("ButtomColor1", Color) = (0, 0, 0, 1)
        _AlphaMask ("Alpha mask (a: alpha 通道)", 2D) = "white" {}
        
        [Space(10)]
        _FillAmount("FillAmount",Range(-0.1, 1.1)) = 0

        [Header(Wave)]
        _EdgeWidth("Edge Wdith", Range(0, 1)) = 0
        [Space(5)]
        _Amplitude1("振幅 1", Range(-0.4, 0.4)) = 1
        _Frequency1("频率 1", Range(-30, 30)) = 1
        _Speed1("Speed 1", Range(-20,20)) = 1
        [Space(5)]
        _Amplitude2("振幅 2", range(-0.4, 0.4)) = 0.1
        _Frequency2("频率 2", Range(-30, 30)) = 1
        _Speed2("Speed 2", Range(-20,20)) = 1

        [Header(Noise)]
        _MainTex("MainTex", 2D) = "white" {}
        _MainTexSpeedX ("Main SpeedX", Range(-10, 10)) = 0
        _MainTexSpeedY ("Main SpeedY", Range(-10, 10)) = 0
        [Space(10)]
        _NoiseTex("NoiseTex", 2D) = "white" {}
        _NoiseSpeedX("NoiseSpeedX", Range(-10, 10)) = 1
        _NoiseSpeedY("NoiseSpeedY", Range(-10, 10)) = 1
        _NoiseStrength ("Noise strength", Range(0, 1)) = 0
        _NoisePower("Noise power", Range(0.00001, 5)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }

        Pass
        {
            ZWrite Off
			ZTest On
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha 
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                half4 uv : TEXCOORD1;
                float4 uv1 : TEXCOORD2;
            };

            sampler2D _AlphaMask;
            sampler2D _MainTex;
            half4 _MainTex_ST;
            sampler2D _NoiseTex;
            half4 _NoiseTex_ST;

            float _MainTexSpeedX, _MainTexSpeedY;
            fixed _EdgeWidth,_MidOffset;
            fixed _ColorStrength;
            half4 _ColorTop, _MidColor;
            half4 _ButtomColor0, _ButtomColor1;
            half _FillAmount;
            float _Amplitude1, _Frequency1, _Speed1;
            float _Amplitude2, _Frequency2, _Speed2;

            fixed _NoiseSpeedX, _NoiseSpeedY, _NoiseStrength, _NoisePower;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.uv;
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _NoiseTex);

                o.uv1.xy = o.uv.xy + float2(_MainTexSpeedX, _MainTexSpeedY) * _Time.y;
                o.uv1.zw = o.uv.zw + float2(_NoiseSpeedX, _NoiseSpeedY) * _Time.y;
                return o;
            }
            

            half4 frag(v2f i) : SV_Target
            {
                half alpha = tex2D(_AlphaMask, i.texcoord).a;

                float height = i.texcoord.y;
                float width = i.texcoord.x;

                float wave1 = _Amplitude1 * sin(width * _Frequency1 + _Speed1 * _Time.y);
                float wave2 = _Amplitude2 * sin(width * _Frequency2 + _Speed2 * _Time.y);
                float wave = wave1 + wave2;

                float f = _FillAmount + wave;
                
                half4 color = 0;

                UNITY_BRANCH
                if (height <= f)
                    color = _ColorTop;
                else if(height > f && height < saturate(f + _EdgeWidth))
                    color = _MidColor;
                else 
                {
                    half noise = tex2D(_NoiseTex, i.uv1.zw).r;
                    noise *= _NoiseStrength;
                    half4 col = tex2D(_MainTex, i.uv1.xy + noise); // 加上扰动UV后再采样主纹理
                    col.r = pow(col.r, _NoisePower);
                    color = lerp(_ButtomColor0, _ButtomColor1, col.r);
                }

                color.a *= alpha;
                return color; 
            }
            ENDCG
        }
    }

    FallBack "VertexLit"
}

