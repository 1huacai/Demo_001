Shader "YoukiaEngine/Effect/Hologram"
{
    Properties
    {
        [Header(Common)]
        _Alpha("Alpha", Range(0, 1)) = 1
        [NoScaleOffset]_NormalMap("法线纹理", 2D) = "bump" {}
        _NormalScale("法线强度", Range(0, 2)) = 1

        [Header(Hologram)]
        [Header(Line1)]
        [HDR]_LineColor1("Line 1 颜色", Color) = (1, 1, 1, 0.5)
        _LineScale1("Line 1 频率", Range(0, 1000)) = 100
        _LineWidth1("Line 1 宽度", Range(0, 2)) = 0
        _LineSpeed1("Line 1 速度", float) = 0

        [Header(Line2)]
        [HDR]_LineColor2("Line 2 颜色", Color) = (1, 1, 1, 1)
        _LineScale2("Line 2 频率", Range(0, 1000)) = 100
        _LineWidth2("Line 2 宽度", Range(0, 2)) = 0
        _LineSpeed2("Line 2 速度", float) = 0

        [Header(ColorGlitch)]
        [NoScaleOffset]_NoiseMap("闪烁噪声图", 2D) = "white" {}
        _ColorGlitch("闪烁强度", Range(0, 1)) = 0.5
        _ColorGlitchSpeed("闪烁频率", Range(0, 2)) = 0.5

        [Header(LineGlitch)]
        [NoScaleOffset]_LineGlitchTex("LineGlitch tex", 2D) = "white" {}
        _LineGlitchSpeed("LineGlitch 速度", Float) = 0.26
        _LineGlitchFrequency("LineGlitch 频率", Float) = 0.2
        _LineGlitchThickness("LineGlitch 偏移宽度", Range(0, 1)) = 0.125
        _LineGlitchOffset("LineGlitch 偏移方向", Vector) = (0.03, 0, 0, 0)

        [Header(Rim)]
        [HDR]_RimColor ("边缘光颜色", Color) = (0, 0, 0, 0)
		_RimPower ("边缘光强度", Range(0, 10)) = 1
        _RimAlpha ("边缘半透", Range(0, 1)) = 0

        [Header(Others)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Transparent"}

        Blend SrcAlpha OneMinusSrcAlpha
		ZWrite[_zWrite]
        ZTest[_zTest]
        Cull[_cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "../Library/YoukiaLightPBR.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                half2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                half3 TtoW[3] : TEXCOORD2;
                half3 viewDir : TEXCOORD6;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //common
            half _Alpha;
            sampler2D _NormalMap;
            half _NormalScale;
            sampler2D _NoiseMap;

            // rim
            half4 _RimColor;
            half _RimPower;
            half _RimAlpha;

            // line
            half4 _LineColor1;
            half _LineScale1;
            float _LineSpeed1;
            half _LineWidth1;

            half4 _LineColor2;
            half _LineScale2;
            float _LineSpeed2;
            half _LineWidth2;

            //Color Glitch
            half _ColorGlitch;
            half _ColorGlitchSpeed;

            //Line Glitch
            sampler2D _LineGlitchTex;
            half _LineGlitchSpeed;
            half _LineGlitchFrequency;
            half _LineGlitchThickness;
            half4 _LineGlitchOffset;


            float HologramLine(sampler2D lineTex, float position, half lineSpeed, half lineFrequency, half lineThickness)
            {
                half lineUV = position * lineFrequency - _Time.y * lineSpeed;
                float l = tex2Dlod(lineTex, half4(lineUV.xx, 0, 0)).r;
                l = saturate(l - (1 - lineThickness));

                return l;
            }

            half HologramLine(half lineScale, float lineSpeed, half lineWidth, float4 linePos)
            {
                half l = sin(linePos * lineScale - _Time.y * lineSpeed);
                l = saturate(l + lineWidth);

                return l;
            }

            half4 HologramLine(half4 lineColor, half lineScale, float lineSpeed, half lineWidth, float4 linePos)
            {
                return HologramLine(lineScale, lineSpeed, lineWidth, linePos) * lineColor;
            }


            float3 VertexOffset(float3 worldPos)
            {
                float linePos = worldPos.y;
                float glitchLine = HologramLine(_LineGlitchTex, linePos, _LineGlitchSpeed, _LineGlitchFrequency, _LineGlitchThickness);
                float3 objectScale = half3(length(unity_ObjectToWorld[0].xyz), length(unity_ObjectToWorld[1].xyz), length(unity_ObjectToWorld[2].xyz));
                float3 glitchOffset = mul( UNITY_MATRIX_T_MV, _LineGlitchOffset).xyz;
                glitchOffset = glitchOffset / objectScale; 
                float3 lineOffset = glitchLine * glitchOffset;

                return lineOffset;
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));

                half3 worldNormal = 0;
                T2W(v.normal, v.tangent, o.TtoW, worldNormal);

                float3 vertexOffset = VertexOffset(o.worldPos);
                v.vertex.xyz += vertexOffset;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldPos = i.worldPos;
                half3 viewDir = i.viewDir;

                //normal
                fixed3 normal = UnpackNormalYoukia(tex2D(_NormalMap, i.uv), _NormalScale);
                normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                //rim
                half rim = 1 - saturate(dot(normal, viewDir));
                rim = saturate(pow(rim, _RimPower));
                half4 rimColor = rim * _RimColor;

                //line1
                float linePos = worldPos.y;
                half4 line1 = HologramLine(_LineColor1, _LineScale1, _LineSpeed1, _LineWidth1, linePos);
                half4 line2 = HologramLine(_LineColor2, _LineScale2, _LineSpeed2, _LineWidth2, linePos);
                half4 lineColor = line1 + line2;

                //Color Glitch
                half noise = tex2D(_NoiseMap, half2(_Time.y * _ColorGlitchSpeed, 1));
                half ColorGlitch = lerp(1, noise, _ColorGlitch);

                //final
                half4 finalColor = 0;
                finalColor.rgb = (lineColor.rgb + rimColor) * ColorGlitch;
                finalColor.a = saturate(_Alpha * lerp(1, rim, _RimAlpha));
                return finalColor;
            }
            ENDCG
        }
    }
}
