Shader "YoukiaEngine/Obsolete/YoukiaHueChange"
{
    Properties
    {
        _HueWidth("色相变化宽度", Range(0, 1)) = 0.4
        _HueChangeSpeed("HueChange", Range(0, 1)) = 0.5
        [Toggle]_IsInvert("IsInvert", Float) = 0
        [Header(Animation key)]
        _HueChangeValue("色相变化", Range(0, 1)) = 0
        _GrayValue("置灰", Range(0, 1)) = 0

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100
        GrabPass{"_CustomGrabPass"}
        Pass
        {
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_zWrite]
            ZTest[_zTest]
            Cull[_cull]
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "../Library/YoukiaTools.cginc"
            #include "YoukiaObsolete.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 SGrabPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _CustomGrabPass;
            half _HueWidth, _HueChangeSpeed;
            half _IsInvert;
            half _HueChangeValue, _GrayValue;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.SGrabPos = ComputeScreenPos(o.vertex);//世界顶点转屏幕顶点
                return o;
            }
            //反色
            half3 IsVertColor(half3 c)
            {
                half3 invertColor = 1 - c;
                return invertColor;
            }

            half3 HueChange(half3 rgbColor)
            {
                half3 hsvColor = rgb2hsv(rgbColor);
                half pingPongFactor = sin(UNITY_TWO_PI * frac(_Time.y * _HueChangeSpeed)) * 0.5 + 0.5;//值域限制
                hsvColor.x = lerp(hsvColor.x, hsvColor.x + _HueWidth, saturate(pingPongFactor));
                hsvColor.x = frac(hsvColor.x);
                return hsv2rgb(hsvColor);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return OBSOLETECOLOR;
                
                half2 uvGrab = half2(i.SGrabPos.xy / i.SGrabPos.w);
                half3 colGrab = tex2D(_CustomGrabPass, uvGrab).rgb;//截屏
                half3 finalColor = colGrab;

                half3 hueChangeCol = HueChange(finalColor);
                half3 invertColor = IsVertColor(hueChangeCol);//0-1转换
                hueChangeCol = _IsInvert * invertColor + (1 - _IsInvert) * hueChangeCol;//可以通过Lerp（）转换

                finalColor = lerp(finalColor, hueChangeCol, _HueChangeValue);

                half3 grayColor = Luminance(finalColor);//转灰
                finalColor = lerp(finalColor,grayColor, _GrayValue);//

                half4 color = half4(finalColor, 1);
                return color;
            }
            ENDCG
        }
    }
}
