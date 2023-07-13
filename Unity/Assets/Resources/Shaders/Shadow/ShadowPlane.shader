Shader "YoukiaEngine/Shadow/ShadowPlane"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Transparent" "ShadowType" = "ST_Terrain" }

        CGINCLUDE
                #include "UnityCG.cginc"
                #include "Lighting.cginc"
                #include "AutoLight.cginc"
                #include "../Library/ShadowLibrary.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 worldPos : TEXCOORD1;
                    float4 pos : SV_POSITION;

                    YOUKIA_LIGHTING_COORDS(2, 3)
                };

                v2f vert (appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                    YOUKIA_TRANSFER_LIGHTING(o, v.uv);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                    YOUKIA_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz, colShadow);
                    col.rgb *= atten;
                    col.a *= (1 - atten);

                    return col;
                }
        ENDCG

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha 
            ZWrite off

            CGPROGRAM
                #pragma multi_compile __ _UNITY_RENDER

                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase

            ENDCG
        }
    }
    Fallback "VertexLit"
}
