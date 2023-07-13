//@@@DynamicShaderInfoStart
//几何消散效果, 需要设备支持 geometry shader
//@@@DynamicShaderInfoEnd
Shader "YoukiaEngine/Effect/Disintegration"
{
    Properties
    {
        [Header(Base)]
        _MainTex ("Texture", 2D) = "white" {}
    
        [Header(Dissolve)]
        _DissolveStrength("DissolveWeight", Range(0, 1)) = 0

        [Header(Particle)]
        [HDR]_ParticleColor("Particle Color", Color) = (1, 1, 1, 1)
        [HDR]_ParticleColorDisapear("Particle Color Dissapear", Color) = (1, 1, 1, 1)
        _ParticleShape("Shape", 2D) = "white" {} 
        _ParticleRadius("Radius Start", Range(0, 1)) = .1
        _ParticleRadiusEnd("Radius End", Range(0, 1)) = .1
        _ParticleCount("Count", Range(0, 1)) = 1

        [Header(FlowMap)]
        _FlowMap("Flow (RG)", 2D) = "black" {}
        _Exapnd("Expand", Range(0, 10)) = 1
        _Direction("Direction", Vector) = (0, 0, 0, 0)
        _FlowMapSpeed("FlowMapSpeed", Range(-10, 10)) = 0

        [Header(Anim)]
        _AnimMap("AnimMap", 2D) = "white" {}
        _AnimControl("AnimControl", vector) = (0,0,0,0)
        _AnimStatus("_AnimStatus", vector) = (0,0,0,0)
        [Header(GPU)]
        [Toggle(_GPUAnimation)] _GPUAnimation("Enable GPUAnimation", Float) = 0

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
            #pragma geometry geom

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "../Library/YoukiaCommon.cginc"
            #include "Assets/Resources/Shaders/Library/YoukiaTools.cginc"
            #include "Assets/Resources/Shaders/Library/GPUSkinningLibrary.cginc"
            #include "../Library/YoukiaMrt.cginc"

            #pragma shader_feature _GPUAnimation

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
                float4 uv3 : TEXCOORD2;
                half4 tangent : TANGENT;
            };

            struct v2g
            {
                float4 objPos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _DeathDissolveTex;
            float4 _DeathDissolveTex_ST;
            float _DissolveStrength;

            float _Exapnd;
            float4 _Direction;
            
            sampler2D _ParticleShape;
            half _ParticleRadius;
            half _ParticleRadiusEnd;
            half4 _ParticleColor;
            half4 _ParticleColorDisapear;
            half _ParticleCount;

            float4 _DissolveParam;

             v2g vert (appdata v)
             {
                v2g o;
                half4 vertex = v.vertex;
                fixed3 normal = v.normal;
                half4 tangent = v.tangent;

                #ifdef _GPUAnimation
                    vertex = AnimationSkinning(v.uv2, v.vertex, normal, tangent);
                #endif

                // o.objPos = UnityObjectToClipPos(vertex);
                o.objPos = vertex;
                o.uv = v.uv;
                o.normal = normal;

                return o;
            }

            float random (float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float remap (float value, float from1, float to1, float from2, float to2) 
            {
                return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            }

            float randomMapped(float2 uv, float from, float to)
            {
                return remap(random(uv), 0, 1, from, to);
            }

            float4 remapFlowTexture(float4 tex)
            {
                return float4
                (
                    remap(tex.x, 0, 1, -1, 1),
                    remap(tex.y, 0, 1, -1, 1),
                    0,
                    remap(tex.w, 0, 1, -1, 1)
                );
            }

            [maxvertexcount(20)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                float2 avgUV = (IN[0].uv + IN[1].uv + IN[2].uv) / 3;
                float3 avgPos = (IN[0].objPos + IN[1].objPos + IN[2].objPos) / 3;
                half3 avgNormal = (IN[0].normal + IN[1].normal + IN[2].normal) / 3;
                float3 avgWPos = mul(unity_ObjectToWorld, avgPos);
                half3 avgWNormal = UnityObjectToWorldNormal(avgNormal);

                float2 dissolveUV = TRANSFORM_TEX(avgUV, _DeathDissolveTex);
                float dissolve = tex2Dlod(_DeathDissolveTex, float4(dissolveUV, 0, 0)).r;
                float t = saturate(_DissolveParam.y - dissolve);
                
                half3 flowNormal = dot(avgNormal, half3(0, 1, 0)) > 0 ? -avgNormal : avgNormal;
                float2 flowUV = TRANSFORM_TEX(avgWPos.xz, _FlowMap);
                half4 flowMap = tex2Dlod(_FlowMap, float4(flowUV + _Time.xx * _FlowMapSpeed, 0, 0));
                float4 flowVector = remapFlowTexture(flowMap);
                // float3 pseudoRandomPos = (avgPos) + (_Direction * flowNormal + flowVector.xyz * _Exapnd) * t;
                flowMap = flowMap * 2 - 1;
                float3 pseudoRandomPos = (avgPos) + (flowNormal + (half3(flowMap.r, 1, flowMap.g)) * _Exapnd) * _Direction * t;

                float3 p = lerp(avgPos, pseudoRandomPos, t);
                float radius = lerp(_ParticleRadiusEnd, _ParticleRadius, (pow(t, 0.25f) * (1 - t)) * 2);

                half flg = saturate(ceil(random(avgUV) - (1 - _ParticleCount)));

                // billboard
                if((t * flg) > 0)
                {
                    float3 right = UNITY_MATRIX_IT_MV[0].xyz;
                    float3 up = UNITY_MATRIX_IT_MV[1].xyz;

                    float4 v[4];
                    v[0] = float4(p + radius * right - radius * up, 1.0f);
                    v[1] = float4(p + radius * right + radius * up, 1.0f);
                    v[2] = float4(p - radius * right - radius * up, 1.0f);
                    v[3] = float4(p - radius * right + radius * up, 1.0f);

                    g2f vert;
                    vert.pos = UnityObjectToClipPos(v[0]);
                    vert.uv = float2(1.0f, 0.0f);
                    vert.normal = avgWNormal;
                    vert.worldPos = mul(unity_ObjectToWorld, v[0]);
                    vert.worldPos.w = t;
                    triStream.Append(vert);

                    vert.pos = UnityObjectToClipPos(v[1]);
                    vert.uv = float2(1.0f, 1.0f);
                    vert.normal = avgWNormal;
                    vert.worldPos = mul(unity_ObjectToWorld, v[1]);
                    vert.worldPos.w = t;
                    triStream.Append(vert);

                    vert.pos = UnityObjectToClipPos(v[2]);
                    vert.uv = float2(0.0f, 0.0f);
                    vert.normal = avgWNormal;
                    vert.worldPos = mul(unity_ObjectToWorld, v[2]);
                    vert.worldPos.w = t;
                    triStream.Append(vert);

                    vert.pos = UnityObjectToClipPos(v[3]);
                    vert.uv = float2(0.0f, 1.0f);
                    vert.normal = avgWNormal;
                    vert.worldPos = mul(unity_ObjectToWorld, v[3]);
                    vert.worldPos.w = t;
                    triStream.Append(vert);
                }
                else
                {
                    for(int j = 0; j < 3; j++)
                    {
                        g2f o;
                        o.pos = UnityObjectToClipPos(IN[j].objPos);
                        o.uv = TRANSFORM_TEX(IN[j].uv, _DeathDissolveTex);
                        o.normal = UnityObjectToWorldNormal(IN[j].normal);
                        o.worldPos = float4(mul(unity_ObjectToWorld, IN[j].objPos).xyz, 0);
                        triStream.Append(o); 
                    }
                }
            }

            FragmentOutput frag (g2f i) : SV_Target
            {
                fixed4 col = 1;
                float4 s = tex2D(_ParticleShape, i.uv);
                clip(s.a - 0.5);

                half t = i.worldPos.w * (1 - i.worldPos.w);
                col = s * lerp(_ParticleColor, _ParticleColorDisapear, saturate(i.worldPos.w));
                col.a *= saturate(t);

                return OutPutParticle(col);
            }

            ENDCG
        }
    }
}
