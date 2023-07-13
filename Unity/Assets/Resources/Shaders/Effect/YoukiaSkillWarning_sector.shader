Shader "YoukiaEngine/Effect/YoukiaSkillWarning/Sector"
{
    Properties
    {
        _WarningProgress("预警进度", Range(0, 1)) = 0
        _WarningWidth("预警条宽度", Range(0.01, 0.5)) = 0.05

        [Space(10)]
        [HDR]_BaseColor ("底色", Color) = (1,1,1,1)
        _BaseAlpha ("底半透度", Range(0, 0.5)) = 0.1

        [Space(10)]
        _Radius ("扇形半径", Range(0, 1)) = 0.5
        _SectorAngle ("扇形角度", Range(1, 180)) = 1

        [Space(10)]
        _EdgeWidth ("圆形边框宽度", Range(0, 1)) = 0.5
        _SideFrame ("扇形两边框宽度", Range(0,0.2)) = 0.05

        [Space(10)]
        _EdgeGradientRange ("圆形边渐变范围", Range(0.01, 0.2)) = 0.1
        _SideGradientRange ("扇形边渐变范围", Range(0.001, 0.05)) = 0.005


        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpsha Blend Mode", Float) = 1
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
                half _EdgeWidth;
                half4 _BaseColor;
                half _BaseAlpha;
                half _WarningProgress;

                half _SectorAngle;

                half _SideFrame;
                half _EdgeGradientRange;
                half _SideGradientRange;
                half _WarningWidth;


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
                    half2 uv3 = UVPolar(half2(i.uv.x, i.uv.y - _SideFrame));

                    //基本范围确定
                    //outer0 最大外圈范围
                    //circleFrame 圆形边框
                    half outer0 = 1 - step(_Radius, uv.x);
                    half inner0 = 1 - step(_Radius - _EdgeWidth, uv.x);
                    half circleFrame = outer0 - inner0;


                    //扇形范围
                    half OuterSector0 = step(0, uv.y - _SectorAngle/360);
                    half OuterSector1 = step(0, (1-uv.y) - _SectorAngle/360);
                    half OuterSectorRange = (1 - OuterSector0 + (1 - OuterSector1));

                    half SectorRange = OuterSectorRange * outer0;
                    
                    half InnerSector0 = step(0, uv3.y - _SectorAngle/360);
                    half InnerSector1 = step(0, (1-uv3.y) - _SectorAngle/360);
                    half InnerSectorRange = (1 - InnerSector0 + (1 - InnerSector1));

                    //SectorFrame 扇形描边
                    half SectorFrame = max(OuterSectorRange - InnerSectorRange, circleFrame);// * SectorRange;

                                 

                    //外圆环渐变
                    half d = distance(uv.x, _Radius);
                    half edgeGradient = (d > _EdgeGradientRange) ? 0 : Remap(d, 0, _EdgeGradientRange, 0.75, 0);// * SectorRange;


                    //扇形两边渐变               
                    _SectorAngle = 90 - _SectorAngle;
                    half2 uvOffset = half2(i.uv.x - 0.5, i.uv.y - 0.5);
                    half y = uvOffset.y - (tan(radians(_SectorAngle)) * uvOffset.x);
                    half right = cos(radians(_SectorAngle)) * y;

                    y = uvOffset.y - (tan(radians(_SectorAngle)) * uvOffset.x * -1);
                    half left = cos(radians(_SectorAngle)) * y;

                    half sideGradient = min(right, left);
                    sideGradient = (sideGradient > _SideGradientRange) ? 0 : Remap(sideGradient, 0, _SideGradientRange, 0.75, 0);// * SectorRange;


                    //技能预警
                    half warningRange = 1 - step(0, uv.x - _WarningProgress);
                    d = distance(uv.x, _WarningProgress);
                    half skillWarning = (d > _WarningWidth) ? 0 : Remap(d, 0, _WarningWidth, 0.75, 0)  * warningRange;

                   
                    _BaseAlpha = (uv.x > _WarningProgress) ? _BaseAlpha : _BaseAlpha * 3;
                    half finalAlpha = max(max(max(_BaseAlpha, max(edgeGradient, sideGradient)), skillWarning), SectorFrame) * SectorRange;
                    

                    return half4(_BaseColor.rgb, finalAlpha);

                }
            ENDCG
        }
    }
}
