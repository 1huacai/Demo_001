//@@@DynamicShaderInfoStart
//Lod1 角色睫毛、视网膜
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Lit/Character/Lod1-Character-Retina"
{
    Properties
    {
        _MainTex ("Texture, R: Lash, G: Retina", 2D) = "white" {}
        _ColorLash ("Color: Lash", Color) = (0, 0, 0, 1)
        _ColorRetina ("Color: Retina", Color) = (0, 0, 0, 1)
        _AlphaLash ("Alpha: Lash", Range(0, 5)) = 1
        _AlphaRetina ("Alpha: Retina", Range(0, 5)) = 1
        

        [Space(30)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

        Blend[_SrcBlend][_DstBlend]
        ZWrite[_zWrite]
        ZTest[_zTest]
        Cull[_cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "../../Library/YoukiaCommon.cginc"
            #include "../../Library/YoukiaMrt.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                fixed2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                fixed2 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            fixed4 _ColorLash, _ColorRetina;
            half _AlphaLash, _AlphaRetina;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            FragmentOutput frag(v2f i)
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                half alphaLash = col.r * _AlphaLash;
                half alphaRetina = col.g * _AlphaRetina;

                half3 colorLash = _ColorLash * alphaLash;
                half3 colorRetina = _ColorRetina * alphaRetina;

                col.rgb = saturate(colorLash + colorRetina);

                col.a = saturate(alphaLash + alphaRetina);

                return OutPutCharacterLod1(col);
            }
            ENDCG
        }
    }

    Fallback "Unlit/Transparent"
}
