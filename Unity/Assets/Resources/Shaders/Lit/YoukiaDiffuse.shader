Shader "YoukiaEngine/Lit/YoukiaDiffuse" 
{
	Properties 
    {
        _Color("颜色", Color) = (1, 1, 1, 1)
		_MainTex("纹理", 2D) = "white" {}

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
		Tags { "Queue"="Geometry" "RenderType" = "Opaque" "ShadowType" = "ST_Opaque" }
		
		Pass 
        {
			Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]

			CGPROGRAM
			
                #pragma vertex vert
                #pragma fragment frag
                // #pragma multi_compile_fog
                #pragma multi_compile_instancing
                #pragma multi_compile_fwdbase

                //#pragma target 2.0
                #include "../Library/YoukiaLight.cginc"
                #include "../Library/YoukiaEnvironment.cginc"

                struct appdata_t 
                {
                    half4 vertex : POSITION;
                    fixed2 texcoord : TEXCOORD0;
                    half3 normal : NORMAL;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f 
                {
                    half4 pos : SV_POSITION;
                    half2 texcoord : TEXCOORD0;
                    half3 normal : TEXCOORD1;
                    float4 worldPos : TEXCOORD2;
                    half3 sh : TEXCOORD3;
                    fixed3 viewDir : TEXCOORD64;
                    fixed3 lightDir : TEXCOORD5;

                    #ifdef _UNITY_RENDER
                        UNITY_LIGHTING_COORDS(6, 7)
                    #else
                        fixed4 screenPos : TEXCOORD6;
                    #endif

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                v2f vert(appdata_t v) 
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.normal = UnityObjectToWorldNormal(v.normal);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                    o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));

                    #ifdef _UNITY_RENDER
                        UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    #else
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif

                    #if defined (UNITY_UV_STARTS_AT_TOP)
                        o.sh = ShadeSHPerVertex(o.normal, o.sh);
                    #endif
                    o.sh += YoukiaGI_IndirectDiffuse(o.normal);

                    return o;
                }
                
                FragmentOutput frag(v2f i)
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    half3 viewDir = i.viewDir;
                    half3 lightDir = i.lightDir;

                    fixed4 c = tex2D(_MainTex, i.texcoord) * _Color;
                    fixed3 albedo = c.rgb;
                    half alpha = c.a;

                    fixed3 normal = i.normal;

                    // shadow
                    #ifdef _UNITY_RENDER
                        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                        fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                    #else
                        half atten = 0;
                        fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten);
                    #endif

                    // youkia light
					UnityGI gi = GetUnityGI_simplify(normal, i.worldPos, lightDir, viewDir, i.sh);
					YoukiaLightingData data = YoukiaData(albedo, normal, viewDir, colShadow, gi.indirect, gi.light);
					
					half4 col;
                    col.rgb = YoukiaLight(data);
                    col.a = alpha;

                    return OutPutDefault(col);
                }
			
			ENDCG
		}

	}
	FallBack "VertexLit"
}
