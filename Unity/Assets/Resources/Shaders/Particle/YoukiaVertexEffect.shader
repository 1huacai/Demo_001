//@@@DynamicShaderInfoStart
//ss
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Particle/YoukiaVertexEffect"
{
    Properties
    {
        [HDR]_Color ("颜色1", Color) = (1, 1, 1, 1)
        [HDR]_Color_0 ("颜色2", Color) = (0, 0, 0, 1)
        _MainTex ("纹理", 2D) = "white" {}
        [Header(Vert strength)]
        _MainVertStrength ("顶点偏移强度", Range(-5, 5)) = 0
        [MaterialToggle]_UV2("计算自定义强度(UV2 x值)", float) = 0

        [Header(UV speed)]
		_UV_Speed_U_Main ("纹理U方向速度", float) = 0
		_UV_Speed_V_Main ("纹理V方向速度", float) = 0
        [Header(UV rotation)]
		[MaterialToggle]_UVRot("uv 旋转", float) = 0
		_MainTexAngle ("纹理旋转", Range(0, 1)) = 0

        [Header(Mask)]
		// [Toggle(_USE_MASK)]_Toggle_USE_MASK_ON("遮罩纹理", float) = 0
		_MaskTex ("遮罩纹理", 2D) = "white" {}
		_UV_Speed_U_Mask ("纹理U方向速度", float) = 0
		_UV_Speed_V_Mask ("纹理V方向速度", float) = 0
		[MaterialToggle]_UVRot_Mask("遮罩纹理uv 旋转", float) = 0
		_MaskTexAngle ("遮罩纹理旋转", Range(0, 1)) = 0

        [Header(Rim)]
        [HDR]_RimColor("边缘光颜色", Color) = (1,1,1,1)
        _RimPower("范围调整01", Range(0.1, 5)) = 1
        _RimRangeMin("范围调整02", Range(0, 0.5)) = 0.1
        _RimRangeExtra("轮廓光", Range(0.5, 0.6)) = 0.5
        [MaterialToggle(_MultiTex)] _MultiTexture("是否收纹理影响", float) = 0


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

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend],[_srcAlphaBlend][_dstAlphaBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
            Lighting Off

            CGPROGRAM
            #pragma vertex vertVertex
            #pragma fragment fragVertexEffect
            // make fog work
            // #pragma multi_compile_fog

            #pragma shader_feature _USE_SOFTPARTICLE
            #pragma shader_feature _USE_MASK
            #pragma shader_feature _USE_RIM
            // #pragma shader_feature _MultiTex

            #include "../Library/ParticleLibrary.cginc"

            ENDCG
        }
    }

    CustomEditor "YoukiaParticleInspectorVert"
    Fallback "Transparent/VertexLit"
}
