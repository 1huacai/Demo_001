Shader "YoukiaEngine/Effect/YoukiaSceneBg"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../Library/YoukiaCommon.cginc"
            #include "../Library/YoukiaMrt.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 proj : TEXCOORD1;
            };

            sampler2D _gSceneTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.proj = ComputeScreenPos(o.vertex);

                return o;
            }

            FragmentOutput frag (v2f i) : SV_Target
            {
                half2 uv = i.proj.xy / i.proj.w;

                // sample the texture
                fixed4 col = tex2D(_gSceneTex, uv);
                return OutPutDefault(col);
            }
            ENDCG
        }
    }
}
