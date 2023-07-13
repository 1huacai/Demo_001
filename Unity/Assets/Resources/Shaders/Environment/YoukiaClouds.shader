Shader "YoukiaEngine/Environment/YoukiaClouds"
{
    Properties
    {
        [Header(Color)]
        [HDR]_CloudsColor1 ("Clouds color 1", Color) = (1, 1, 1, 1)
        [HDR]_CloudsColor2 ("Clouds color 2", Color) = (1, 1, 1, 1)
        _LightStrength ("Light strength", Range(0, 1)) = 0.5
        [Header(Noise)]
        [NoScaleOffset]_NoiseTex1 ("Noise texture 1", 2D) = "white" {}
        [NoScaleOffset]_NoiseTex2 ("Noise texture 2", 2D) = "white" {}
        _NoiseScale1 ("Noise texture scale 1", Range(0, 10)) = 1.5
        _NoiseScale2 ("Noise texture scale 2", Range(0, 10)) = 0.8
        _NoiseStrength1 ("Noisse strength 1", Range(0, 5)) = 2
        _NoiseStrength2 ("Noisse strength 2", Range(0, 5)) = 1

        _Speed ("Speed", vector) = (0, 0, 0, 0)

        [Space(5)]
        _StepLength ("Step length", Range(0, 10)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IngoreProjector"="True" }
        LOD 100

        Pass
        {
            ZWrite Off
			Cull Back
			Blend SrcAlpha One 

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                fixed2 uv : TEXCOORD0;
            };

            struct v2f
            {
                fixed2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                half4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _NoiseTex1, _NoiseTex2;
            half4 _NoiseTex1_TexelSize, _NoiseTex2_TexelSize;

            half _NoiseScale1, _NoiseScale2;
            half _NoiseStrength1, _NoiseStrength2;
            half _StepLength;
            half4 _Speed;
            half4 _CloudsColor1, _CloudsColor2;
            half _LightStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldPos = i.worldPos;
                half3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));

                half4 cloudsColor = lerp(_CloudsColor1, _LightColor0, _LightStrength);

                half4 noise1 = tex2D(_NoiseTex1, i.uv * _NoiseScale1 + _Time.x * _Speed.xy) * _NoiseStrength1 * cloudsColor;
                half n = noise1 * 0.1f;
                half4 noise2 = tex2D(_NoiseTex2, i.uv * _NoiseScale2 + _Time.x * _Speed.zw + n) * _NoiseStrength2 * cloudsColor;

                half weight = 0.8f;

                half stepCount = 2;
                for (int k = 0; k < stepCount; k++)
                {
                    cloudsColor = lerp(_CloudsColor2, _LightColor0, 1 - weight);
                    noise1 += saturate(tex2D(_NoiseTex1, i.uv * _NoiseScale1 + _Time.x * _Speed.xy + _StepLength * _NoiseTex1_TexelSize * lightDir.xy) * _NoiseStrength1 * weight) * cloudsColor;
                    noise2 += saturate(tex2D(_NoiseTex2, i.uv * _NoiseScale2 + _Time.x * _Speed.zw + _StepLength * _NoiseTex2_TexelSize * lightDir.xy + n) * _NoiseStrength2 * weight) * cloudsColor;
                    weight *= 0.6f;
                }

                half4 noise = saturate((noise1 + noise2) * 0.5);

                half4 color = saturate(noise);
                return color;
            }
            ENDCG
        }
    }
}
