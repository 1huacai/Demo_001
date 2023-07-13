Shader "YoukiaEngine/Effect/YoukiaCloudFlow" 
{
    Properties 
    {
        _MainColor ("MainColor", Color) = (1,1,1,1)
        _Tex1 ("Tex1", 2D) = "white" {}
        _Tex2 ("Tex2", 2D) = "white" {}
        _Speed ("Speed", Range(0, 100)) = 1

        [Header(Soft Particle)]
		[Toggle(_USE_SOFTPARTICLE)]_Toggle_USE_SOFTPARTICLE_ON("软粒子", float) = 0
		_SoftFade ("柔和度", Range(0.01, 3)) = 1.0
    }
    SubShader 
    {
        Tags 
        { "IgnoreProjector"="True" "Queue"="Transparent" "RenderType"="Transparent" }

        Pass 
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile __ _UNITY_RENDER
			    #pragma shader_feature _USE_SOFTPARTICLE

                #include "UnityCG.cginc"
                #include "../Library/YoukiaEffect.cginc"
                #include "../Library/YoukiaEnvironment.cginc"

                #pragma multi_compile_instancing
                #pragma target 3.0

                sampler2D _Tex1, _Tex2; 
                float4 _Tex1_ST, _Tex2_ST;
                float4 _MainColor;

                struct VertexInput 
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct VertexOutput 
                {
                    float4 pos : SV_POSITION;
                    float4 uv : TEXCOORD0;

                    #if (defined(_USE_SOFTPARTICLE))
					    half4 projPos : TEXCOORD3;
				    #endif

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                VertexOutput vert (VertexInput v) 
                {
                    VertexOutput o = (VertexOutput)0;

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.uv.xy = TRANSFORM_TEX(v.uv, _Tex1);
                    o.uv.zw = TRANSFORM_TEX(v.uv, _Tex2);

                    o.pos = UnityObjectToClipPos(v.vertex);

                    #if _USE_SOFTPARTICLE
                        o.projPos = ComputeScreenPos(o.pos);
                        COMPUTE_EYEDEPTH(o.projPos.z);
                    #endif

                    return o;
                }

                float4 frag(VertexOutput i) : COLOR 
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    fixed4 col1 = tex2D(_Tex1, i.uv.zw);
                    fixed4 col2 = tex2D(_Tex2, i.uv.xy + (_Time.x * _Speed * 0.1f));

                    fixed4 col = fixed4(_MainColor.rgb, lerp(0.0, col2.r, col1.r));

                    #if _USE_SOFTPARTICLE
                        col.a *= SoftParticle(i.projPos);
                    #endif

                    // 环境亮度
				    col.rgb = EnvirLumChange(col.rgb);

                    return col;
                }
            ENDCG
        }
    }
}
