Shader "YoukiaEngine/Effect/YoukiaSector"
{
    Properties
    {
        [HDR]_Color ("Color", color) = (1, 1, 1, 1)
        [HDR]_ColorEdge ("边缘颜色", color) = (1, 1, 1, 1)
        _Radius ("半径", Range(0, 1)) = 0.5
        _EdgeWidth ("圆边缘宽度", Range(0, 1)) = 0.5

        _Amount ("Amount", Range(0, 0.9999)) = 0.5
        _AmountWidth ("夹角边缘宽度", Range(0, 0.5)) = 0.1

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

                half4 _ColorEdge;

                half _Radius;
                half _Amount;

                half _EdgeWidth;
                half _AmountWidth;

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
                    
                    half radius = 1 - saturate(uv.x - _Radius);
                    half amount = saturate(uv.y - _Amount * 0.5);
                    amount *= saturate((1 - uv.y) - _Amount * 0.5);
                    half alpha = ceil(amount * (radius > _EdgeWidth));
                    
                    half4 colorEdge = _ColorEdge * alpha;

                    if (radius < 1 && radius > _EdgeWidth)
                        return colorEdge;

                    amount /= max(0.0001f, 1 - _Amount);
                    if (amount > (1 - _AmountWidth))
                        return colorEdge;
                    if (amount < (_AmountWidth))
                        return colorEdge;

                    return _Color * alpha;
                }
            ENDCG
        }
    }
}
