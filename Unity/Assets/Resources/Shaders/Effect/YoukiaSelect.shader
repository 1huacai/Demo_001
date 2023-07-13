Shader "YoukiaEngine/Effect/YoukiaSelect"
{
    Properties
    {
        [Header(Rim)]
		[HDR]_RimColor ("边缘光颜色", Color) = (1, 1, 1, 1)
		_RimPower ("边缘光强度", Range(0, 10)) = 1

        [Header(XRay)]
        [HDR]_XRayColor ("XRay颜色", Color) = (1, 1, 1, 1)
		_XRayPower ("XRay强度", Range(0, 10)) = 1

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
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        LOD 100

        CGINCLUDE
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half3 viewDir : TEXCOORD4;
                half3 normal : TEXCOORD5;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }
        ENDCG

        Pass
        {
            // x - ray
            Blend [_SrcBlend] One
			ZWrite Off
			ZTest Greater
 
			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                half4 _XRayColor;
                half _XRayPower;

                fixed4 frag (v2f i) : SV_Target
                {
                    half4 col = 0;

                    half rim = 1.0 - saturate(dot(i.viewDir, i.normal));
                    rim = saturate(pow(rim, _XRayPower));

                    half4 rimCol = _XRayColor * pow(rim, _XRayPower);

                    col.rgb = rimCol.rgb;
                    col.a = rim;

                    return col;
                }
                
			ENDCG
        }

        Pass
        {
            Blend [_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite On
			ZTest LEqual
			Cull Back

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                half4 _RimColor;
                half _RimPower;

                fixed4 frag (v2f i) : SV_Target
                {
                    half4 col = 0;

                    half rim = 1.0 - saturate(dot(i.viewDir, i.normal));
                    rim = saturate(pow(rim, _RimPower));

                    half4 rimCol = _RimColor * pow(rim, _RimPower);

                    col.rgb = rimCol.rgb;
                    col.a = rim;

                    return col;
                }
            ENDCG
        }
    }
}
