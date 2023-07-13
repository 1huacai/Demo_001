Shader "YoukiaEngine/Effect/YoukiaSkillWarning/rect"
{
    Properties
    {
        [HDR]_BaseColor ("底色", Color) = (1,1,1,1)
        _BaseAlpha ("底半透度", Range(0, 0.5)) = 0.1

        [Space(5)]
        _WarningProgress("技能预警进度", Range(0, 1)) = 0
        _WarningWidth("预警条宽度", Range(0.1, 1)) = 0.1

        [Space(5)]
        [HideInInspector]_localXScale("矩形宽", float) = 1
        [HideInInspector]_localYScale("矩形长", float) = 1

        [space(5)]
        _FBWidth("前后宽度", Range(0, 0.5)) = 0.1
        _LRWidth("左右宽度", Range(0, 0.5)) = 0.1

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
                    float3 objPos : TEXCOORD1;
                    float4 vertex : SV_POSITION;
                };

                half4 _BaseColor;
                half _BaseAlpha;

                half _localXScale;
                half _localYScale;

                half _FBWidth;
                half _LRWidth;
                half _WarningProgress;
                half _WarningWidth;


                float Remap(float input, float oldMin, float oldMax, float newMin, float newMax)
                {
                    return ((newMin + (input - oldMin) * (newMax - newMin) / (oldMax - oldMin)));
                }


                v2f_ vert (appdata_ v)
                {
                    v2f_ o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.objPos = v.vertex;
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag (v2f_ i) : SV_Target
                {

                    //预警条
                    half warningRange = 1 - step(0, i.uv.y - _WarningProgress);

                    _WarningProgress -= 0.5;
                    
                    half d = distance(i.objPos.y * _localYScale, _WarningProgress * _localYScale);
                    half warning = (d > _WarningWidth) ? 0 : Remap(d, 0, _WarningWidth, 0.75, 0) * warningRange;
                    
 
                    //外边框
                    half f = (i.objPos.y + 0.5) * _localYScale;
                    d = distance(f, _localYScale);
                    float r_f = (d > _FBWidth) ? 0 : Remap(d, _FBWidth, 0, 0, 1);


                    half b = (i.objPos.y + 0.5) * _localYScale;
                    d = distance(b, 0);
                    float r_b = (d > _FBWidth) ? 0 : Remap(d, 0, _FBWidth, 1, 0);


                    half r = (i.objPos.x + 0.5) * _localXScale;
                    d = distance(r, _localXScale);
                    float r_r = (d > _LRWidth) ? 0 : Remap(d, _LRWidth, 0, 0, 1);


                    half l = (i.objPos.x + 0.5) * _localXScale;
                    d = distance(l, 0);
                    float r_l = (d > _LRWidth) ? 0 : Remap(d, 0, _LRWidth, 1, 0);
                    
                    _BaseAlpha = (i.objPos.y < _WarningProgress) ? max(0.05, _BaseAlpha * 5 * (i.objPos.y + 0.5)) : _BaseAlpha;
                    half finalAlpha = max(max(_BaseAlpha, max(max(max(r_f, r_b), r_r), r_l)), warning);


                    return half4(_BaseColor.rgb, finalAlpha); 

                }
            ENDCG
        }
    }
}
