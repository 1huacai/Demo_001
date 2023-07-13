Shader "YoukiaEngine/Effect/YoukiaDecalMarchLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha 
            Cull Back

            CGPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ _UNITY_RENDER
            #pragma multi_compile __ _YOUKIA_REVERSED_Z

            #include "UnityCG.cginc"
            #include "../Library/YoukiaCommon.cginc"
            #include "../Library/YoukiaMrt.cginc"
            #include "../Library/YoukiaTools.cginc"
            #include "../Library/YoukiaDecal.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 proj : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.proj = ComputeScreenPos(o.pos);
                // COMPUTE_EYEDEPTH(o.proj.z);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                float2 screenUV = i.proj.xy / i.proj.w;

                // terrain mask
                // half4 mrtMask = GetMask(screenUV);
                // half terrainMask = GetMaskTerrain(mrtMask);

                float depth = YoukiaDepth(screenUV);

                #if _YOUKIA_REVERSED_Z
                #else
                    depth = depth * 2 - 1;
                #endif

                float3 wsPos = GetWorldPositionFromDepthValue(screenUV, depth).xyz;
                half2 uv = DecalUV(wsPos);

                fixed4 col = tex2D(_MainTex, uv);
                // col *= terrainMask;

                return col;
            }
            ENDCG
        }
    }
}
