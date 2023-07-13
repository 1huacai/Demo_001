Shader "YoukiaEngine/Effect/StencilPlane"
{
    Properties
    {
        _Ref("参考值", Range(0,255)) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("比较操作", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _OP("模板操作", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1000" "ShadowType" = "ST_StencilPanel"}

        Pass
        {
            Stencil
            {
                Ref [_Ref]
                Comp [_Comp]
                Pass [_OP]
            }


            //ColorMask 0
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                //float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return half4(1,1,1,0);
            }
            ENDCG
        }
    }
}
