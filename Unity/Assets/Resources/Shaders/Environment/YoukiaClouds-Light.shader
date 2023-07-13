Shader "YoukiaEngine/Environment/YoukiaClouds-Light"
{
    Properties
    {
        [HDR]_Color0 ("Color0", Color) = (1, 1, 1, 1)
        _Color1 ("Color1", Color) = (0.5, 0.5, 0.5, 1)
        _CloudsTex ("R: right, G: top, B: left, A: bottom", 2D) = "white" {}
        _CloudsAlpha ("R: front, G: back, B: Alpha", 2D) = "white" {}
        _ColorScale ("Color Scale", Range(0, 5)) = 1
        
        _InscatterClouds ("Inscatter", Range(0, 10)) = 1
        _InscatterCloudsExponent ("Inscatter exponent", Range(0, 64)) = 1

        [Header(Parallax)]
        _Parallax ("Parallax", Range(-1, 1)) = 0
        _ParallaxPow ("Parallax pow", Range(0, 2)) = 1

        [Header(CustomLight)]
        [MaterialToggle]_CustomLight ("自定义光源", float) = 0
        [HDR]_CustomLightColor ("光源颜色", Color) = (1, 1, 1, 1)
        _CustomLightDir ("光源方向(x, y, z)", Vector) = (0, 0, 0, 0)

        [Header(Others)]
        [Enum(Off, 0, On, 1)] _zWrite("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTest("ZTest", Float) = 4
		[Enum(UnityEngine.Rendering.CullMode)] _cull("Cull Mode", Float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _srcAlphaBlend("Src Alpha Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _dstAlphaBlend("Dst Alpha Blend Mode", Float) = 10

        [Space(10)]
		[MaterialToggle]_CollectVariants("_CollectVariants(强行收集材质球)", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IngoreProjector"="True" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
			ZWrite[_zWrite]
			ZTest[_zTest]
			Cull[_cull]
            ColorMask RGB
        
            CGPROGRAM
                // #pragma multi_compile_fwdbase
                #pragma multi_compile_instancing

                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "Lighting.cginc"
                #include "AutoLight.cginc"
                #include "../Library/YoukiaCommon.cginc"
                #include "../Library/YoukiaMrt.cginc"
                #include "../Library/YoukiaEnvironment.cginc"
                #include "../Library/Atmosphere.cginc"

                // youkia height fog
                #pragma multi_compile __ _HEIGHTFOG
                #pragma multi_compile __ _SKYENABLE

                struct appdata
                {
                    half4 vertex : POSITION;
                    fixed2 uv : TEXCOORD0;
                    half3 normal : NORMAL;
                    half4 tangent : TANGENT;
                    half4 color : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    #if _UVMove
                        fixed4 uv : TEXCOORD0;
                    #else
                        fixed2 uv : TEXCOORD0;
                    #endif

                    half4 vertex : SV_POSITION;
                    float4 worldPos : TEXCOORD1;
                    half3 lightDir : TEXCOORD2;
                    half3 viewDir : TEXCOORD3;

                    half3 inscatter : TEXCOORD4;
                    half3 extinction : TEXCOORD5;
                    half4 color : TEXCOORD6;
                    half3 lightColor : TEXCOORD7;

                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                sampler2D _CloudsTex;
                sampler2D _CloudsAlpha;
                half4 _CloudsTex_ST;
                half4 _Color0, _Color1;
                half _ColorScale;

                half _InscatterClouds;
                half _InscatterCloudsExponent;

                half _Parallax;
                half _ParallaxPow;

                half _CustomLight;
                half4 _CustomLightColor;
                half4 _CustomLightDir;

                half _CollectVariants;

                v2f vert (appdata v)
                {
                    v2f o;
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv.xy = TRANSFORM_TEX(v.uv, _CloudsTex);
                    #if _UVMove
                         o.uv.zw = TRANSFORM_TEX(v.uv, _UVMoveNoise);
                    #endif

                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                    // tangent light dir to rotate
                    fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
                    fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 
                    float3x3 worldToTangent = float3x3(worldTangent, worldBinormal, worldNormal);
                    // o.lightDir = mul(worldToTangent, WorldSpaceLightDir(v.vertex));

                    TANGENT_SPACE_ROTATION;
                    UNITY_BRANCH
                    if (_CustomLight)
                    {
                        o.lightDir = mul(rotation, normalize(_CustomLightDir)).xyz;
                        o.lightColor = _CustomLightColor;
                    }
                    else
                    {
                        o.lightDir = mul(rotation, normalize(ObjSpaceLightDir(v.vertex))).xyz;
                        o.lightColor = _LightColor0.rgb;
                    }
                        
                    o.viewDir = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

                    // atmosphere
                    #ifdef _SKYENABLE
                        half3 extinction = 0;
                        // float3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
                        o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, extinction);
                        o.extinction = extinction;
                    #endif

                    o.color = v.color;

                    return o;
                }

                half4 frag (v2f i) : SV_Target
                {
                    half3 lightDir = i.lightDir;
                    half3 viewDir = i.viewDir;
                    half3 lightColor = i.lightColor;

                    fixed4 cloud1 = tex2D(_CloudsAlpha, i.uv);
                    half parallaxPow = lerp(1, _ParallaxPow, cloud1.b);
                    // parallax
                    fixed2 parallaxOffset = ParallaxOffset(pow(cloud1.r, parallaxPow), _Parallax, (viewDir));

                    fixed4 cloud0 = tex2D(_CloudsTex, i.uv + parallaxOffset);
                    cloud1 = tex2D(_CloudsAlpha, i.uv + parallaxOffset);
                    
                    // half seed = (i.worldPos.x + i.worldPos.y + i.worldPos.z) / 3;
                    half alpha = cloud1.b;
                    half front = cloud1.r;
                    half back = cloud1.g;
                    // half hMap = lerp(cloud0.z, cloud0.x, ceil(saturate(lightDir.x)));   // Picks the correct horizontal side.
                    // half vMap = lerp(cloud0.w, cloud0.y, ceil(saturate(lightDir.y)));   // Picks the correct Vertical side.
                    // half dMap = lerp(back, front, ceil(saturate(lightDir.z)));          // Picks the correct Front/back Pseudo Map
                    half hMap = (lightDir.x > 0.0f) ? cloud0.x : cloud0.z;   // Picks the correct horizontal side.
                    half vMap = (lightDir.y > 0.0f) ? cloud0.y : cloud0.w;   // Picks the correct Vertical side.
                    half dMap = (lightDir.z > 0.0f) ? front : back;          // Picks the correct Front/back Pseudo Map
                    half lightMap = hMap * lightDir.x * lightDir.x + vMap * lightDir.y * lightDir.y + dMap * lightDir.z * lightDir.z;   // Pythagoras!
                    lightMap = pow(lightMap, _ColorScale);

                    half inscatter = pow(saturate(dot(viewDir, lightDir)), _InscatterCloudsExponent) * _InscatterClouds * (1 - pow(front, 0.25f));
                    
                    fixed4 col;
                    col = lerp(_Color1, _Color0, lightMap);
                    col.rgb *= lightColor;
                    col.rgb = lerp(col.rgb, lightColor, saturate(inscatter));
                    col.a *= alpha;

                    col *= i.color;
                    
                    #ifdef _SKYENABLE
                        col.rgb = col.rgb * i.extinction + i.inscatter;
                    #endif

                     // height fog
                    #ifdef _HEIGHTFOG
                        // height fog
                        half fog = 0;
                        half3 heightfog = YoukiaHeightFog(i.worldPos, 0, fog);
                        col.rgb = lerp(col.rgb, heightfog, fog);
                    #endif

                    return col;
                }
            ENDCG
        }
    }

    FallBack "Transparent/VertexLit"
}
