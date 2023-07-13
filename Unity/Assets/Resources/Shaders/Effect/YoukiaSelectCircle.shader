Shader "YoukiaEngine/Effect/YoukiaSelectCircle"
{
    Properties
    {
        [HDR]_Color ("Color", Color) = (1, 1, 1, 1)
        _Radius ("半径", Range(0, 0.5)) = 0.5
        _Width ("宽度", Range(0, 1)) = 0.1
        [Space(5)]
        _Count ("虚线数量", Range(1, 100)) = 10
        _Space ("虚线间隔", Range(0, 1)) = 0.1
        [Space(5)]
        _RotateSpeed ("旋转速度", float) = 0

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

                half _Radius;
                half _Width;
                half _Count;
                half _Space;
                half _RotateSpeed;

                half _CircleScale;

                struct appdata_
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f_
                {
                    float4 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f_ vert (appdata_ v)
                {
                    v2f_ o;
                    UNITY_INITIALIZE_OUTPUT(v2f_, o);
                    UNITY_SETUP_INSTANCE_ID(v);

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                    o.uv.zw = v.uv;
                    return o;
                }

                fixed4 frag (v2f_ i) : SV_Target
                {
                    // 模型缩放值
                    // half3 scale = ObjectScale();
                    half scale = _CircleScale;

                    // 极坐标uv
                    half2 uvPolar = UVPolar(i.uv);
                    float speed = _RotateSpeed / scale;

                    // 计算虚线
                    half count = _Count * scale;
                    half unit = 1 / floor(count);
                    float uvRotate = abs(uvPolar.y + _Time.x * speed);
                    half uvSlice = saturate((uvRotate - floor(uvRotate / unit) * unit) / unit);
                    half space = saturate(uvSlice - _Space);

                    // 计算圆半径和宽度
                    half2 center = half2(0.5, 0.5);
                    half dis = distance(i.uv.zw, center);

                    float width = _Width / scale;
                    half radius = _Radius;

                    half flag = (dis - (radius - width)) * (radius - dis) * space;

                    half4 color = lerp(0, _Color, saturate(ceil(flag)));

                    return color;
                }
            ENDCG
        }
    }
}
