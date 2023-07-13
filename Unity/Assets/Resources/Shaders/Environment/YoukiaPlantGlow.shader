Shader "YoukiaEngine/Environment/YoukiaPlantGlow"
{
    Properties
    {
        _Color("颜色(rgb, a: alpha)", Color) = (1, 1, 1, 1)
        _MainTex("纹理", 2D) = "white" {}
        [Header(Normal Metallic)]
        [NoScaleOffset] _BumpMetaMap("法线纹理", 2D) = "white" {}
        _NormalStrength ("法线强度", Range(0, 5)) = 1
        [Header(Diffuse)]
        _DiffThreshold ("Diffuse threshold", Range(0, 0.5)) = 0
        [Header(Rim)]
        [HDR]_RimColor("边缘光", Color) = (1, 1, 1, 1)
        _RimPower("边缘光范围", Range(0.000001, 10)) = 1
        _RimVertFade("边缘光顶点色过度", Range(0, 2)) = 1
        [HDR]_RimCenterColor("中间颜色(RGBA)", Color) = (0, 0, 0, 1)
        [Header(Wind)]
        _WindAtten("全局风力衰减", Range(0,1)) = 1

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
        Tags { "RenderType" = "Transparent" "Queue"="Transparent" "ShadowType" = "ST_Tree"}
        LOD 100

        CGINCLUDE
        #pragma multi_compile_instancing

        #define _DISABLE_GISPEC 1

        #include "../Library/YoukiaLightPBR.cginc"
        #include "../Library/YoukiaEnvironment.cginc"
        #include "../Library/Atmosphere.cginc"

        #pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
        #pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK
        
        struct appdata_t 
        {
            half4 vertex : POSITION;
            fixed2 texcoord : TEXCOORD0;
            half3 normal : NORMAL;
            half4 tangent : TANGENT;
            half3 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        ENDCG

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

            #pragma multi_compile_fwdbase nolightmap
            #pragma fragmentoption ARB_precision_hint_fastest

            #pragma multi_compile __ _HEIGHTFOG
            #pragma multi_compile __ _SKYENABLE
            #pragma multi_compile __ _PP_HEIGHTFOG
            #pragma multi_compile __ _GWIND

            struct v2f
            {
                half4 pos : SV_POSITION;
                fixed2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;

                fixed4 screenPos : TEXCOORD2;

                half4 heightfog : TEXCOORD4;
                half4 TtoW0 : TEXCOORD5;
                half4 TtoW1 : TEXCOORD6; 
                half4 TtoW2 : TEXCOORD7;

                fixed3 viewDir : TEXCOORD9;
                fixed3 lightDir : TEXCOORD10;

                half3 inscatter : TEXCOORD11;
                half3 extinction : TEXCOORD12;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // half _NormalStrength;

            half4 _RimColor, _RimCenterColor;
            half _RimPower, _RimVertFade;

            // post process fog
            sampler2D _HeightFogTex;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                #ifdef _GWIND
                    float4 windValue = YoukiaWind(v.color, v.vertex.xyz);
                    v.vertex.xyz += windValue.xyz;
                #endif


                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                o.lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));

                o.screenPos = ComputeScreenPos(o.pos);

                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 

                o.TtoW0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, v.color.r);
                o.TtoW1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, v.color.g);
                o.TtoW2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, v.color.b);

                // height fog
                #ifdef _HEIGHTFOG
                    half fog = 0;
                    o.heightfog.rgb = YoukiaHeightFog(o.worldPos, 0, fog);
                    o.heightfog.a = fog;
                #endif

                // atmosphere
                #ifdef _SKYENABLE
                    half3 extinction = 0;
                    // float3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
                    o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, extinction);
                    o.extinction = extinction;
                #endif

                return o;
            }

            FragmentOutput frag(v2f i)
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 worldPos = i.worldPos;
                fixed3 viewDir = i.viewDir;
                half3 lightDir = i.lightDir;
                fixed3 vertColor = fixed3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);

                // tex
                // rgb: color, a: AO
                fixed4 c = tex2D(_MainTex, i.uv);
                // rg: normal, b: Metallic, a: Roughness
                fixed4 n = tex2D(_BumpMetaMap, i.uv);

                fixed4 col;
                col.rgb = c.rgb * _Color.rgb;
                half alpha = c.a * _Color.a;
                fixed3 abledo = col.rgb;

                fixed3 normal = UnpackNormalYoukia(n, _NormalStrength);
                normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));

                // diffuse
                half nl = saturate(dot(normal, lightDir));
                nl = max(_DiffThreshold, nl);

                half rim = 1.0 - saturate(dot(viewDir, normal));
                half rimRange = pow(rim, _RimPower);
                half4 rimColor = _RimColor * rimRange;
                half vertFade = saturate(pow(1 - vertColor.r, _RimVertFade));

                col.rgb = col.rgb * nl + lerp(_RimCenterColor.rgb, rimColor.rgb, saturate(rimRange)) * vertFade;
                col.a = lerp(_RimCenterColor.a, rimColor.a, saturate(rimRange)) * vertFade * alpha;

                #ifdef _SKYENABLE
                    col.rgb = col.rgb * i.extinction + i.inscatter;
                #endif

                // height fog
                #ifdef _HEIGHTFOG
                    col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
                #endif

                // post fog
                #if _PP_HEIGHTFOG
                    half4 fog = PostHeightFog(worldPos);
                    col.rgb = lerp(col.rgb, fog.rgb, fog.a);
                #endif

                return OutPutDefault(col);
            }
            ENDCG
        }

        Pass 
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            ZWrite On 
            ZTest LEqual
            Cull back
            
            CGPROGRAM
            #pragma vertex vertShadowWind
            #pragma fragment fragShadow
            // #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            #pragma fragmentoption ARB_precision_hint_fastest
            
            ENDCG
        }
    }

    FallBack "Transparent/Cutout/VertexLit"
}
