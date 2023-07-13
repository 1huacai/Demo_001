Shader "YoukiaEngine/Depth/DepthTexture"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 pos : SV_POSITION;
				float4 depth : TEXCOORD0;
            };

			uniform half _Cutoff;
			uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
			uniform fixed4 _Color;

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.depth.xy = o.pos.zw;
				o.depth.zw = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texcol = tex2D(_MainTex, i.depth.zw);
				clip(texcol.a * _Color.a - _Cutoff);

                float depth = i.depth.x / i.depth.y;

				#if defined (SHADER_TARGET_GLSL) 
					//(-1, 1)-->(0, 1)
					depth = depth * 0.5 + 0.5;
				#elif defined (UNITY_REVERSED_Z)
				  	//(1, 0)-->(0, 1)
					depth = 1 - depth;  
				#endif

                return depth;
            }
            ENDCG
        }
    }
}
