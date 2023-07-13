Shader "YoukiaEngine/Effect/YoukiaSkillWarning/Ring"
{
    Properties
    {
        _WarningProgress("预警范围", Range(0, 1)) = 0
        _WarningWidth("预警条宽度", Range(0.01, 0.5)) = 0.05

        [Space(5)]
        _Radius ("圆环外半径", Range(0, 1)) = 0.5
        _InnerRadius ("圆环内半径", Range(0, 0.9)) = 0.2

        [Space(5)]
        _EdgeWidth ("渐变宽度", Range(0.01, 0.2)) = 0.05
        _FrameWidth ("外框宽度", Range(0, 0.2)) = 0.05

        [Space(5)]
        [HDR]_BaseColor ("底色", Color) = (1,1,1,1)
        _BaseAlpha ("底半透度", Range(0, 0.5)) = 0.1


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
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

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
                #pragma multi_compile_instancing

                #include "../Library/ParticleLibrary.cginc"

                struct appdata_
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f_
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };


                half _Radius;
                half _InnerRadius;
                half _EdgeWidth;
                half4 _BaseColor;
                half _BaseAlpha;
                half _WarningProgress;
                half _WarningWidth;
                half _FrameWidth;


                float Remap(float input, float oldMin, float oldMax, float newMin, float newMax)
                {
                    return ((newMin + (input - oldMin) * (newMax - newMin) / (oldMax - oldMin)));
                }


                v2f_ vert (appdata_ v)
                {
                    v2f_ o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
           
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag (v2f_ i) : SV_Target
                {

                    half2 uv = UVPolar(i.uv);


                    //圆环范围
                    half circleRange = 1 - step(_Radius, uv.x);
                    half InnerRange = 1 - step(_InnerRadius, uv.x);
                    circleRange = circleRange - InnerRange;


                    //外渐变
                    half d = distance(uv.x, _Radius);
                    half edgeGradient = (d > _EdgeWidth) ? 0 : Remap(d, 0, _EdgeWidth, 0.5, 0);
                    //内渐变
                    d = distance(uv.x, _InnerRadius) * circleRange;
                    half innerGradient = (d > _EdgeWidth) ? 0 : Remap(d, 0, _EdgeWidth, 0.5, 0);

                    edgeGradient = max(edgeGradient, innerGradient) * circleRange;



                    //边缘框
                    half frameRange = circleRange - (1 - step(_Radius - _FrameWidth, uv.x));
                    half InnerFrame = (1 - step(_InnerRadius + _FrameWidth, uv.x)) - InnerRange;
                    frameRange = max(frameRange, InnerFrame);


                    //预警条
                    _WarningProgress = Remap(_WarningProgress, 0, 1, _InnerRadius, 1);
                    half warningRange = 1 - step(_WarningProgress, uv.x);
                    d = distance(uv.x, _WarningProgress);
                    half skillWaring = (d > _WarningWidth) ? 0 : Remap(d, 0, _WarningWidth, 0.75, 0) * warningRange;


                    _BaseAlpha = (uv.x > _WarningProgress) ? _BaseAlpha : _BaseAlpha * 3;

                    half finalAlpha = max(max(max(edgeGradient, skillWaring), _BaseAlpha), frameRange) * circleRange;
                    return half4(_BaseColor.rgb, finalAlpha);

                }
            ENDCG
        }
    }
}
