Shader "YoukiaEngine/Effect/YoukiaGrid"
{
    Properties
    {
        [HDR]_Color ("Color", Color) = (1, 1, 1, 1)
        _WorldScale ("世界坐标缩放", float) = 0.14
        _WorldOffsetX ("world Pos offset X", float) = 0
        _WorldOffsetZ ("world Pos offset Z", float) = 0
        _LineWidth ("Line Width", Range(0, 2)) = 0.5

        [Header(Noise)]
        [NoScaleOffset]_NoiseTex ("NoiseTex", 2D) = "white" {}
        _NoiseScale ("NoiseScale", Range(0, 1)) = 0.14
        _NoiseStrength ("Noise strength", Range(0, 1)) = 0.5
        _NoiseSpeedX ("NoiseSpeed", Range(-5, 5)) = 1
        _NoiseSpeedZ ("NoiseSpeed", Range(-5, 5)) = 1

        [MaterialToggle]_GridStyle ("格子类型(勾选: 菱形格子)", float) = 1

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags { "IgnoreProjector"="True" "Queue"="Transparent-2" "RenderType"="Transparent" }

        Pass
        {
            Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
			Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define ONE_DEGREE (UNITY_PI / 180.0)

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            half4 _Color;
            sampler2D _NoiseTex;
            
            half _WorldScale, _LineWidth;
            half _NoiseStrength, _NoiseScale, _NoiseSpeedX, _NoiseSpeedZ;

            half _WorldOffsetX, _WorldOffsetZ;
            half _GridStyle;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = float2(_NoiseSpeedX, _NoiseSpeedZ) * _Time.x;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 wUV = i.worldPos.xz;
                float2 uvNoise = wUV * _NoiseScale + i.uv;
                
                half noise = tex2D(_NoiseTex, uvNoise);
                noise = lerp(1, noise, _NoiseStrength);

                wUV += float2(_WorldOffsetX + _LineWidth / 2, _WorldOffsetZ + _LineWidth / 2);
                half size = sqrt(_WorldScale * _WorldScale + _WorldScale * _WorldScale);
                half4 col = 0;

                half width = lerp(wUV.x, abs(wUV.x - wUV.y), _GridStyle);
                half height = lerp(wUV.y, abs(wUV.x + wUV.y), _GridStyle);

                if (width % size < _LineWidth)
                    col = _Color;

                if (height % size < _LineWidth)
                    col = _Color;

                col *= noise;

                return col;
            }
            ENDCG
        }
    }
}
