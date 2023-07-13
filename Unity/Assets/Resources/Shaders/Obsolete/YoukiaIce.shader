Shader "YoukiaEngine/Obsolete/YoukiaIce_Bigmap"
{
    Properties
    {
        _IceTex ("Ice Texture", 2D) = "white" {}
        _IceNormal ("Ice Normal", 2D) = "white" {}
        _SnowTex ("Snow Texture", 2D) = "white" {}
        [HDR]_Emisson ("Emisson Color", Color) = (0,0,0,0)
        // _MaskClip("MaskClip Value", Float) = 0
        _HeightScale("Height Scale", Range(0, 0.1)) = 0
        _Opacity("Opacity", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="Ture" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ _UNITY_RENDER
            #pragma multi_compile __ _HEIGHTFOG
            #pragma multi_compile __ _SKYENABLE

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEnvironment.cginc"
            #include "../Library/Atmosphere.cginc"
            #include "../Library/YoukiaEaryZ.cginc"
            #include "../Library/YoukiaEffect.cginc"
            #include "YoukiaObsolete.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 TtoW0 : TEXCOORD1;
                half4 TtoW1 : TEXCOORD2;
                half4 TtoW2 : TEXCOORD3;
                half3 viewDir : TEXCOORD4;
                half3 lightDir : TEXCOORD5;
                half3 sh : TEXCOORD6;
                half3 giColor : TEXCOORD7;

                YOUKIA_ATMOSPERE_DECLARE(8, 9)
                YOUKIA_LIGHTING_COORDS(10, 11)
                YOUKIA_HEIGHTFOG_DECLARE(12)
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _IceTex;
            float4 _IceTex_ST;
            sampler2D _IceNormal;
            sampler2D _SnowTex;

            half _MaskClip;
            half4 _Emisson;
            half _HeightScale;
            half _Opacity;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);


                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _IceTex);
                half4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                o.lightDir = normalize(UnityWorldSpaceLightDir(worldPos));

                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                o.TtoW0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.TtoW1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.TtoW2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

                #ifdef _UNITY_RENDER
                    UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                #else
                    o.screenPos = ComputeScreenPos(o.pos);
                #endif

                YoukiaVertSH(worldNormal, worldPos, o.sh, o.giColor);

                // height fog
                #ifdef _HEIGHTFOG
                    o.heightfog.rgb = YoukiaHeightFog(worldPos, 0, o.heightfog.a);
                #endif

                // atmosphere
                #ifdef _SKYENABLE
                    o.inscatter = InScattering(_WorldSpaceCameraPos, worldPos, o.extinction);
                #endif
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return OBSOLETECOLOR;

                half3 lightDir_ws = i.lightDir;
                half3 viewDir_ws = i.viewDir;
                half3 worldPos = half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
                
                
                half4 iceColor = tex2D(_IceTex, i.uv);
                half4 snowColor = tex2D(_SnowTex, i.uv);
                half opaticy = saturate((1 - iceColor.a) * (10 * _MaskClip));

                //alpha
                half l = saturate(_Opacity * 2 - 1 + iceColor.a);
                half alpha = lerp(l, 1, opaticy);
                // half alphaTest = iceColor.a;

                // clip(alphaTest);


                //metallic
                half metalic = 0;

                //normal
                half4 normalMap = tex2D(_IceNormal, i.uv);
                half3 normal_ts = UnpackNormalYoukia(normalMap, 1);
                half3 normal_ws = normalize(half3(dot(i.TtoW0.xyz, normal_ts), dot(i.TtoW1.xyz, normal_ts), dot(i.TtoW2.xyz, normal_ts)));


                //smoothness
                half smoothness = lerp(normalMap.b, 0, opaticy);

                //metallic
                half metallic = 0;


                //parallax
                half height = normalMap.a;
                half heightScale = _HeightScale;
                //worldToTangent
                half3 base_x = i.TtoW0.xyz;
                half3 base_y = i.TtoW1.xyz;
                half3 base_z = i.TtoW2.xyz;

                //view Dir
                half3 viewDir_ts = normalize(base_x * viewDir_ws.x + base_y * viewDir_ws.y + base_z * viewDir_ws.z);

                //uv
                float2 parallaxUV = ParallaxOffset(height, heightScale, viewDir_ts);
                half2 texcoord = i.uv + parallaxUV;

                //albedo
                iceColor.rgb = tex2D(_IceTex, texcoord).rgb;
                half3 albedo = lerp(iceColor.rgb, snowColor.rgb, opaticy);


                #if defined (UNITY_UV_STARTS_AT_TOP)
                    i.sh = YoukiaGI_IndirectDiffuse(normal_ws, worldPos, i.giColor);
                #endif

                //shadow
                #ifdef _UNITY_RENDER
                    UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
                    fixed3 colShadow = lerp(_gShadowColor, 1, atten);
                #else
                    half atten = 0;
                    fixed3 colShadow = YoukiaScreenShadow(i.screenPos, atten);
                #endif


                //gi
                UnityGI gi = GetUnityGI(albedo, normal_ws, worldPos, lightDir_ws, viewDir_ws, colShadow, metallic, smoothness, i.sh, i.giColor);
                

                //brdf
                half oneMinusReflectivity;
                half3 specColor;
                albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

                BRDFPreData brdfPreData = (BRDFPreData)0;
                PrepareBRDF(smoothness, normal_ws, viewDir_ws, oneMinusReflectivity, brdfPreData);

                half4 finalColor;
                finalColor.rgb = BRDF_Unity_PBS(albedo, specColor, normal_ws, viewDir_ws, brdfPreData, gi.light, gi.indirect, colShadow, atten);

                finalColor.a = alpha;

                //emission
                half3 emission = iceColor.rgb * _Emisson.rgb;
                emission = lerp(emission, half3(0,0,0), opaticy);

                finalColor.rgb += emission;


                // height fog
                #ifdef _HEIGHTFOG
                    finalColor.rgb = lerp(finalColor.rgb, i.heightfog.rgb, i.heightfog.a);
                #endif

                #ifdef _SKYENABLE
                    finalColor.rgb = finalColor.rgb * i.extinction + i.inscatter;
                #endif


                // lum
                finalColor.rgb = SceneLumChange(finalColor.rgb);
                
                return finalColor;
            }
            ENDCG
        }
    }
}
