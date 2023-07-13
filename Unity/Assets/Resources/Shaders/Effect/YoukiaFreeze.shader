Shader "YoukiaEngine/Effect/YoukiaFreeze"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_BumpMap ("Bump texture", 2D) = "white" {}
        [Header(Parallax)]
        _Parallax ("视差", Range(0, 1)) = 0

        [Header(Freeze)]
        _FreezeIntensity("Freeze intensity", Range(0, 1)) = 0
        [NoScaleOffset]_FreezeTex ("Freeze texture", 2D) = "white" {}
        _FreezeTexScale ("Freeze texture scale", Range(0, 10)) = 1
        _FreezeScale ("Freeze scale", Range(0, 1)) = 0.1
        _FreezePow ("Freeze power", Range(1, 5)) = 1
        [HDR]_FreezeColor0 ("Freeze color 0", Color) = (1, 1, 1, 1)
        [HDR]_FreezeColor1 ("Freeze color 1", Color) = (1, 1, 1, 1)
        _FreezeShinniess ("Freeze shinness", Range(1, 256)) = 0.9
        _FreezeGloss ("Freeze gloss", Range(1, 50)) = 1
        _FreezeColorPow ("Freeze color power", Range(0, 20)) = 1
        [Header(Rim)]
        _FreezeRim ("边缘强度", Range(0, 10)) = 1
        _FreezeRimPow ("边缘范围", Range(0, 10)) = 1
        [Header(Depth)]
        _FreezeDepth ("深度", Range(0.001, 1)) = 1
        _FreezeDepthOffset ("深度偏移", Range(0, 1)) = 0.1

        [Header(Anim)]
		_AnimMap("AnimMap", 2D) = "white" {}
		_AnimControl("AnimControl", vector) = (0,0,0,0)
		_AnimStatus("_AnimStatus", vector) = (0,0,0,0)

        [Header(GPU)]
        [Toggle(_GPUAnimation)] _GPUAnimation("Enable GPUAnimation", Float) = 0
		// [Toggle(_TwoBoneWeight)] _TwoBoneWeight("Enable Two Bone Weight(default is four)", Float) = 0
    }
    SubShader
    {
        Name "Freeze"
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite On
        ZTest LEqual
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "../Library/YoukiaLightPBR.cginc"
            #include "../Library/YoukiaEffect.cginc"
            #include "../Library/YoukiaMrt.cginc"
            #include "../Library/GPUSkinningLibrary.cginc"
            #include "../Library/Atmosphere.cginc"

            #pragma multi_compile __ _UNITY_RENDER
            #pragma multi_compile __ _SKYENABLE
            #pragma shader_feature _GPUAnimation
            // #pragma shader_feature _TwoBoneWeight

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
				float4 uv3 : TEXCOORD2;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float3 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD1;

                // UNITY_FOG_COORDS(2)
                float4 TtoW[3] : TEXCOORD3;  
                float3 sh : TEXCOORD6;

                float2 uv : TEXCOORD0;
                fixed4 screenPos : TEXCOORD7;

                float3 inscatter : TEXCOORD11;
                float3 extinction : TEXCOORD12;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            half _Parallax;
            half _FreezeRim, _FreezeRimPow;

            fixed _Freeze, _FreezeIntensity;
            sampler2D _FreezeTex;
            fixed _FreezeTexScale, _FreezeScale, _FreezePow;
            fixed4 _FreezeColor0, _FreezeColor1;
            fixed _FreezeColorPow;
            fixed _FreezeDepth, _FreezeDepthOffset, _FreezeShinniess, _FreezeGloss;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                fixed3 normal = v.normal;
                float4 vertex = v.vertex;
                half4 tangent = v.tangent;
                #ifdef _GPUAnimation
                    // #ifdef _TwoBoneWeight
                        vertex = AnimationSkinning(v.uv2, v.vertex, normal, tangent);
                    // #else
                    //     vertex = AnimationSkinningFourBoneWeight(v.uv2, v.uv3, v.vertex, normal);
                    // #endif
                #endif

                half freezeNoise = tex2Dlod(_FreezeTex, fixed4(v.texcoord * _FreezeTexScale, 0, 0));
                
                // o.worldPos = mul(unity_ObjectToWorld, vertex);
                vertex.xyz += lerp(0, normal * _FreezeScale * pow(freezeNoise, _FreezePow), _FreezeIntensity);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

                o.pos = UnityObjectToClipPos(vertex);
                o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, vertex);
                half height = o.worldPos.y + _FreezeIntensity;
                
                fixed3 worldNormal = UnityObjectToWorldNormal(normal);  
                fixed3 worldTangent = UnityObjectToWorldDir(tangent.xyz);  
                fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangent.w; 

                // 切线空间view dir
                TANGENT_SPACE_ROTATION;
                float3 viewDirTangent = mul(rotation, ObjSpaceViewDir(vertex)).xyz;

                o.TtoW[0] = float4(worldTangent.x, worldBinormal.x, worldNormal.x, viewDirTangent.r);
                o.TtoW[1] = float4(worldTangent.y, worldBinormal.y, worldNormal.y, viewDirTangent.g);
                o.TtoW[2] = float4(worldTangent.z, worldBinormal.z, worldNormal.z, viewDirTangent.b);
                
                #if defined (UNITY_UV_STARTS_AT_TOP)
                    o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                #endif
                o.sh += YoukiaGI_IndirectDiffuse(worldNormal);

                o.screenPos = ComputeScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.screenPos.z);

                // atmosphere
                #ifdef _SKYENABLE
                    float3 extinction = 0;
                    // float3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
                    o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, extinction);
                    o.extinction = extinction;
                #endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float3 worldPos = i.worldPos;
                fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                half3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));

                // return saturate(worldPos.y + _FreezeIntensity);

                // tex
                // mask
                fixed mask = tex2D(_FreezeTex, i.uv * _FreezeTexScale);
                // parallax 
                fixed parallax = tex2D(_MainTex, i.uv).r;
                float2 parallaxOffset = ParallaxOffset(parallax, _Parallax, (float3(i.TtoW[0].w, i.TtoW[1].w, i.TtoW[2].w)));
                // rgb：color, a: alpha
                fixed4 c = tex2D(_MainTex, i.uv + parallaxOffset);
                // rg: normal
                fixed4 n = tex2D(_BumpMap, i.uv);

                // normal
                fixed3 normal = UnpackNormal(n);
                normal = normalize(half3(dot(i.TtoW[0].xyz, normal), dot(i.TtoW[1].xyz, normal), dot(i.TtoW[2].xyz, normal)));

                half rim = 1.0 - saturate(dot(viewDir, normal));
                
                fixed freezeLerp = max(rim, mask);
                freezeLerp = saturate(pow(freezeLerp, _FreezeColorPow));
                float d = YoukiaDepthPROJ(i.screenPos);
                d = LinearEyeDepth(d);

                float depth = saturate(d - i.screenPos.z);
                fixed4 color = lerp(_FreezeColor0 * (pow(depth, _FreezeDepth) + _FreezeDepthOffset), _FreezeColor1, freezeLerp) * c;
                color = saturate(color);
                half3 h = normalize(lightDir + viewDir + mask);
                half ndotl = max(0, dot(normal, lightDir));
                ndotl = ndotl * 0.5 + 0.5;
                float nh = max(0, dot(normalize(normal), h));
                float spec = max(0, pow(nh, _FreezeGloss * 128)) * _FreezeShinniess;   
            
                color.rgb = color.rgb * ndotl + spec * _LightColor0.rgb + i.sh * (pow(rim, _FreezeRimPow) * _FreezeRim);
                color = lerp(0, color, _FreezeIntensity);

                #ifdef _SKYENABLE
                    color.rgb = color.rgb * i.extinction + i.inscatter;
                #endif

                return color;
            }
            ENDCG
        }
    }
    FallBack "VertexLit"
}
