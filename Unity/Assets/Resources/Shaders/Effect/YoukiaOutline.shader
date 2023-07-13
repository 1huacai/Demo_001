//@@@DynamicShaderInfoStart
//描边(描边会导致面数翻倍, 请注意！！！
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Effect/YoukiaOutline" 
{
	Properties 
    {
        // _Color("颜色", Color) = (1, 1, 1, 1)
		// _MainTex("纹理 (rgb: color, a: AO(顶面贴花Mask))", 2D) = "white" {}   
        [Header(Outline)]
        [HDR]_OutlineColor("描边颜色", Color) = (1, 1, 1, 1)
        _OutlineWidth("描边宽度", Range(0, 10)) = 1
        _SwitchAngle("描边修正参数", Range(0.0, 180.0)) = 89
        [Header(Breath)]
        _OutlineBreath("呼吸频率", Range(0, 10)) = 0
        _BreathMinValue("呼吸亮度最小值", Range(0, 1)) = 0.5

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
	    [Space(10)]
        _Ref("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare OP", Float) = 8
        [Enum(UnityEngine.Rendering.StencilOp)] _Pass("Stencil OP", Float) = 0
    }
	SubShader 
    {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        Stencil
        {
            Ref [_Ref]
            Comp [_Comp]
            Pass [_Pass]
        }

        CGINCLUDE
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 4.0

            #include "UnityCG.cginc"

            #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK
            
        ENDCG
        
        Pass
        {
            // outline
            Name "Outline"
            Blend[_SrcBlend][_DstBlend]
            ZWrite Off
            Cull [_cull]
            ZTest[_zTest]

            CGPROGRAM
                #include "../Library/YoukiaOutline.cginc"

                #pragma vertex vertOutline
                #pragma fragment fragOutline

            ENDCG
        }

	}
	FallBack "VertexLit"
}
