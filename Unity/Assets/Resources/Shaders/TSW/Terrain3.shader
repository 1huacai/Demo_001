// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:1,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.6397059,fgcg:0.7912779,fgcb:1,fgca:1,fgde:0.01,fgrn:0,fgrf:800,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:4220,x:33021,y:32604,varname:node_4220,prsc:2|diff-850-OUT,spec-5545-OUT,gloss-9598-OUT,normal-7066-OUT;n:type:ShaderForge.SFN_Tex2d,id:6992,x:32190,y:32833,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:node_6992,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e07a00177e4416c488186f85d2872090,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Tex2d,id:4337,x:28197,y:32449,ptovrint:False,ptlb:Alpha1,ptin:_Alpha1,varname:node_4337,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:7203fcc19537a8143bb9f6dbb63f0666,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:8932,x:27968,y:31423,ptovrint:False,ptlb:Rock,ptin:_Rock,varname:node_8932,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:971647c30c0015e4694d695d212a216e,ntxv:0,isnm:False|UVIN-8491-OUT;n:type:ShaderForge.SFN_Tex2d,id:7188,x:27971,y:31742,ptovrint:False,ptlb:Grass,ptin:_Grass,varname:node_7188,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:903e120956363da4da229ad0377af8e8,ntxv:0,isnm:False|UVIN-8491-OUT;n:type:ShaderForge.SFN_Lerp,id:3076,x:30141,y:31896,varname:node_3076,prsc:2|A-8932-RGB,B-3592-OUT,T-3903-OUT;n:type:ShaderForge.SFN_TexCoord,id:3984,x:27540,y:31535,varname:node_3984,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:8491,x:27717,y:31535,varname:node_8491,prsc:2|A-3984-UVOUT,B-586-OUT;n:type:ShaderForge.SFN_ValueProperty,id:586,x:27540,y:31707,ptovrint:False,ptlb:Tiling,ptin:_Tiling,varname:node_586,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Lerp,id:3592,x:29027,y:31915,varname:node_3592,prsc:2|A-7188-RGB,B-907-OUT,T-4337-R;n:type:ShaderForge.SFN_Vector3,id:8116,x:27971,y:31992,varname:node_8116,prsc:2,v1:0.1729022,v2:0.2867647,v3:0.2093382;n:type:ShaderForge.SFN_Tex2d,id:306,x:28411,y:32864,ptovrint:False,ptlb:Alpha2,ptin:_Alpha2,varname:node_306,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:55720279e9881ea40896a716ee8da199,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:1227,x:30889,y:32304,varname:node_1227,prsc:2|A-3076-OUT,B-7195-RGB,T-4337-A;n:type:ShaderForge.SFN_Tex2d,id:6873,x:30146,y:32785,ptovrint:False,ptlb:sha,ptin:_sha,varname:node_6873,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:b4d89ea2ae40c14428e39b15025b5794,ntxv:0,isnm:False|UVIN-8491-OUT;n:type:ShaderForge.SFN_Lerp,id:3271,x:30392,y:32649,varname:node_3271,prsc:2|A-8932-RGB,B-6873-RGB,T-306-R;n:type:ShaderForge.SFN_Vector3,id:5715,x:30377,y:32919,varname:node_5715,prsc:2,v1:0.6470588,v2:0.6470588,v3:0.6470588;n:type:ShaderForge.SFN_Lerp,id:5067,x:30851,y:32864,varname:node_5067,prsc:2|A-3271-OUT,B-2304-OUT,T-306-G;n:type:ShaderForge.SFN_Lerp,id:850,x:31205,y:32855,varname:node_850,prsc:2|A-1227-OUT,B-5067-OUT,T-306-B;n:type:ShaderForge.SFN_Multiply,id:907,x:28195,y:31974,varname:node_907,prsc:2|A-7188-RGB,B-8116-OUT;n:type:ShaderForge.SFN_Multiply,id:2304,x:30621,y:32875,varname:node_2304,prsc:2|A-3271-OUT,B-5715-OUT;n:type:ShaderForge.SFN_Tex2d,id:7195,x:30025,y:32217,ptovrint:False,ptlb:Grass2,ptin:_Grass2,varname:node_7195,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c19d7c0cdff0f5c43bd6f8afc3afca25,ntxv:0,isnm:False|UVIN-8491-OUT;n:type:ShaderForge.SFN_Multiply,id:1246,x:28733,y:32139,varname:node_1246,prsc:2|A-8932-A,B-8459-OUT;n:type:ShaderForge.SFN_OneMinus,id:8459,x:28501,y:32165,varname:node_8459,prsc:2|IN-4337-G;n:type:ShaderForge.SFN_Multiply,id:1406,x:28733,y:32269,varname:node_1406,prsc:2|A-7188-A,B-4337-G;n:type:ShaderForge.SFN_Step,id:3903,x:28930,y:32173,varname:node_3903,prsc:2|A-1246-OUT,B-1406-OUT;n:type:ShaderForge.SFN_Tex2d,id:6406,x:28023,y:30351,ptovrint:False,ptlb:Grass2_n,ptin:_Grass2_n,varname:node_6406,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:4bd808b0c868dcd41aa2b24e719eb55e,ntxv:0,isnm:False|UVIN-8491-OUT;n:type:ShaderForge.SFN_Tex2d,id:154,x:28024,y:29913,ptovrint:False,ptlb:Grass_n,ptin:_Grass_n,varname:node_154,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:4e9485c9ee44202498ae60d855fb2773,ntxv:0,isnm:False|UVIN-8491-OUT;n:type:ShaderForge.SFN_Tex2d,id:1377,x:28029,y:29425,ptovrint:False,ptlb:Rock_n,ptin:_Rock_n,varname:node_1377,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:833a89774bc2dc049a9c5add0129d97f,ntxv:0,isnm:False|UVIN-8491-OUT;n:type:ShaderForge.SFN_Tex2d,id:1296,x:28022,y:30858,ptovrint:False,ptlb:Sha_n,ptin:_Sha_n,varname:node_1296,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5a46ee8baec14914e99ac176b734d208,ntxv:0,isnm:False|UVIN-8491-OUT;n:type:ShaderForge.SFN_Lerp,id:374,x:30890,y:31771,varname:node_374,prsc:2|A-3187-OUT,B-8506-OUT,T-4337-A;n:type:ShaderForge.SFN_Lerp,id:5121,x:31218,y:32236,varname:node_5121,prsc:2|A-374-OUT,B-922-OUT,T-306-B;n:type:ShaderForge.SFN_NormalBlend,id:7066,x:32489,y:32684,varname:node_7066,prsc:2|BSE-5121-OUT,DTL-6992-RGB;n:type:ShaderForge.SFN_Power,id:5249,x:28865,y:29384,varname:node_5249,prsc:2|VAL-8730-OUT,EXP-5486-OUT;n:type:ShaderForge.SFN_Vector1,id:5486,x:28188,y:29498,varname:node_5486,prsc:2,v1:2;n:type:ShaderForge.SFN_Subtract,id:5147,x:29252,y:29384,varname:node_5147,prsc:2|A-2661-OUT,B-2027-OUT;n:type:ShaderForge.SFN_Power,id:2027,x:28865,y:29528,varname:node_2027,prsc:2|VAL-679-OUT,EXP-5486-OUT;n:type:ShaderForge.SFN_Vector1,id:2931,x:28350,y:29498,varname:node_2931,prsc:2,v1:1;n:type:ShaderForge.SFN_Subtract,id:2661,x:29041,y:29333,varname:node_2661,prsc:2|A-2931-OUT,B-5249-OUT;n:type:ShaderForge.SFN_Sqrt,id:4073,x:29431,y:29384,varname:node_4073,prsc:2|IN-5147-OUT;n:type:ShaderForge.SFN_Append,id:6380,x:29655,y:29374,varname:node_6380,prsc:2|A-8730-OUT,B-679-OUT,C-4073-OUT;n:type:ShaderForge.SFN_Power,id:2378,x:28868,y:29838,varname:node_2378,prsc:2|VAL-1959-OUT,EXP-3203-OUT;n:type:ShaderForge.SFN_Vector1,id:3203,x:28192,y:29951,varname:node_3203,prsc:2,v1:2;n:type:ShaderForge.SFN_Subtract,id:3936,x:29255,y:29838,varname:node_3936,prsc:2|A-6167-OUT,B-1928-OUT;n:type:ShaderForge.SFN_Power,id:1928,x:28868,y:29982,varname:node_1928,prsc:2|VAL-6616-OUT,EXP-3203-OUT;n:type:ShaderForge.SFN_Vector1,id:3483,x:28352,y:29951,varname:node_3483,prsc:2,v1:1;n:type:ShaderForge.SFN_Subtract,id:6167,x:29049,y:29787,varname:node_6167,prsc:2|A-3483-OUT,B-2378-OUT;n:type:ShaderForge.SFN_Sqrt,id:2801,x:29434,y:29838,varname:node_2801,prsc:2|IN-3936-OUT;n:type:ShaderForge.SFN_Append,id:5209,x:29642,y:29838,varname:node_5209,prsc:2|A-1959-OUT,B-6616-OUT,C-2801-OUT;n:type:ShaderForge.SFN_Power,id:4678,x:28865,y:30306,varname:node_4678,prsc:2|VAL-2920-OUT,EXP-9270-OUT;n:type:ShaderForge.SFN_Vector1,id:9270,x:28195,y:30392,varname:node_9270,prsc:2,v1:2;n:type:ShaderForge.SFN_Subtract,id:6217,x:29252,y:30306,varname:node_6217,prsc:2|A-3009-OUT,B-2800-OUT;n:type:ShaderForge.SFN_Power,id:2800,x:28865,y:30450,varname:node_2800,prsc:2|VAL-960-OUT,EXP-9270-OUT;n:type:ShaderForge.SFN_Vector1,id:8640,x:28357,y:30392,varname:node_8640,prsc:2,v1:1;n:type:ShaderForge.SFN_Subtract,id:3009,x:29045,y:30254,varname:node_3009,prsc:2|A-8640-OUT,B-4678-OUT;n:type:ShaderForge.SFN_Sqrt,id:229,x:29431,y:30306,varname:node_229,prsc:2|IN-6217-OUT;n:type:ShaderForge.SFN_Append,id:8506,x:29639,y:30306,varname:node_8506,prsc:2|A-2920-OUT,B-960-OUT,C-229-OUT;n:type:ShaderForge.SFN_Power,id:9752,x:28881,y:30835,varname:node_9752,prsc:2|VAL-8507-OUT,EXP-7103-OUT;n:type:ShaderForge.SFN_Vector1,id:7103,x:28216,y:30895,varname:node_7103,prsc:2,v1:2;n:type:ShaderForge.SFN_Subtract,id:6357,x:29268,y:30835,varname:node_6357,prsc:2|A-3124-OUT,B-9326-OUT;n:type:ShaderForge.SFN_Power,id:9326,x:28881,y:30979,varname:node_9326,prsc:2|VAL-1337-OUT,EXP-7103-OUT;n:type:ShaderForge.SFN_Vector1,id:4311,x:28348,y:30895,varname:node_4311,prsc:2,v1:1;n:type:ShaderForge.SFN_Subtract,id:3124,x:29061,y:30784,varname:node_3124,prsc:2|A-4311-OUT,B-9752-OUT;n:type:ShaderForge.SFN_Sqrt,id:5734,x:29447,y:30835,varname:node_5734,prsc:2|IN-6357-OUT;n:type:ShaderForge.SFN_Append,id:6412,x:29647,y:30835,varname:node_6412,prsc:2|A-8507-OUT,B-1337-OUT,C-5734-OUT;n:type:ShaderForge.SFN_Vector1,id:5545,x:32791,y:32631,varname:node_5545,prsc:2,v1:0;n:type:ShaderForge.SFN_Lerp,id:8423,x:30060,y:33192,varname:node_8423,prsc:2|A-1377-B,B-154-B,T-3903-OUT;n:type:ShaderForge.SFN_Lerp,id:3315,x:30957,y:33242,varname:node_3315,prsc:2|A-8423-OUT,B-6406-B,T-4337-A;n:type:ShaderForge.SFN_Lerp,id:8846,x:30482,y:33445,varname:node_8846,prsc:2|A-1377-B,B-1296-B,T-306-R;n:type:ShaderForge.SFN_Lerp,id:9598,x:31239,y:33382,varname:node_9598,prsc:2|A-3315-OUT,B-8846-OUT,T-306-B;n:type:ShaderForge.SFN_Lerp,id:922,x:30412,y:31860,varname:node_922,prsc:2|A-6380-OUT,B-6412-OUT,T-306-R;n:type:ShaderForge.SFN_Multiply,id:9665,x:28463,y:29341,varname:node_9665,prsc:2|A-1377-R,B-5486-OUT;n:type:ShaderForge.SFN_Multiply,id:1623,x:28463,y:29583,varname:node_1623,prsc:2|A-1377-G,B-5486-OUT;n:type:ShaderForge.SFN_Subtract,id:8730,x:28635,y:29341,varname:node_8730,prsc:2|A-9665-OUT,B-2931-OUT;n:type:ShaderForge.SFN_Subtract,id:679,x:28635,y:29583,varname:node_679,prsc:2|A-1623-OUT,B-2931-OUT;n:type:ShaderForge.SFN_Multiply,id:652,x:28465,y:29778,varname:node_652,prsc:2|A-154-R,B-3203-OUT;n:type:ShaderForge.SFN_Multiply,id:4442,x:28465,y:30020,varname:node_4442,prsc:2|A-154-G,B-3203-OUT;n:type:ShaderForge.SFN_Subtract,id:1959,x:28637,y:29778,varname:node_1959,prsc:2|A-652-OUT,B-3483-OUT;n:type:ShaderForge.SFN_Subtract,id:6616,x:28637,y:30020,varname:node_6616,prsc:2|A-4442-OUT,B-3483-OUT;n:type:ShaderForge.SFN_Multiply,id:3441,x:28463,y:30253,varname:node_3441,prsc:2|A-6406-R,B-9270-OUT;n:type:ShaderForge.SFN_Multiply,id:7215,x:28463,y:30495,varname:node_7215,prsc:2|A-6406-G,B-9270-OUT;n:type:ShaderForge.SFN_Subtract,id:2920,x:28635,y:30253,varname:node_2920,prsc:2|A-3441-OUT,B-8640-OUT;n:type:ShaderForge.SFN_Subtract,id:960,x:28635,y:30495,varname:node_960,prsc:2|A-7215-OUT,B-8640-OUT;n:type:ShaderForge.SFN_Multiply,id:1853,x:28457,y:30755,varname:node_1853,prsc:2|A-1296-R,B-7103-OUT;n:type:ShaderForge.SFN_Multiply,id:6509,x:28457,y:30997,varname:node_6509,prsc:2|A-1296-G,B-7103-OUT;n:type:ShaderForge.SFN_Subtract,id:8507,x:28629,y:30755,varname:node_8507,prsc:2|A-1853-OUT,B-4311-OUT;n:type:ShaderForge.SFN_Subtract,id:1337,x:28629,y:30997,varname:node_1337,prsc:2|A-6509-OUT,B-4311-OUT;n:type:ShaderForge.SFN_Lerp,id:3187,x:30145,y:31582,varname:node_3187,prsc:2|A-6380-OUT,B-5209-OUT,T-3903-OUT;proporder:586-7188-154-7195-6406-8932-1377-6873-1296-4337-306-6992;pass:END;sub:END;*/

Shader "Custom/TSW/Terrain3" {
    Properties {
        _Tiling ("Tiling", Float ) = 1
        _Grass ("Grass", 2D) = "white" {}
        _Grass_n ("Grass_n", 2D) = "white" {}
        _Grass2 ("Grass2", 2D) = "white" {}
        _Grass2_n ("Grass2_n", 2D) = "white" {}
        _Rock ("Rock", 2D) = "white" {}
        _Rock_n ("Rock_n", 2D) = "white" {}
        _sha ("sha", 2D) = "white" {}
        _Sha_n ("Sha_n", 2D) = "white" {}
        _Alpha1 ("Alpha1", 2D) = "white" {}
        _Alpha2 ("Alpha2", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 100
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "Assets/Resources/Shaders/Library/Atmosphere.cginc"
            #pragma multi_compile __ _SKYENABLE
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Alpha1; uniform float4 _Alpha1_ST;
            uniform sampler2D _Rock; uniform float4 _Rock_ST;
            uniform sampler2D _Grass; uniform float4 _Grass_ST;
            uniform float _Tiling;
            uniform sampler2D _Alpha2; uniform float4 _Alpha2_ST;
            uniform sampler2D _sha; uniform float4 _sha_ST;
            uniform sampler2D _Grass2; uniform float4 _Grass2_ST;
            uniform sampler2D _Grass2_n; uniform float4 _Grass2_n_ST;
            uniform sampler2D _Grass_n; uniform float4 _Grass_n_ST;
            uniform sampler2D _Rock_n; uniform float4 _Rock_n_ST;
            uniform sampler2D _Sha_n; uniform float4 _Sha_n_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;

                #ifdef _SKYENABLE
                    half3 inscatter : TEXCOORD9;
                    half3 extinction : TEXCOORD10;
                #endif

                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)

                // atmosphere
                #ifdef _SKYENABLE
                    half3 extinction = 0;
                    // half3 MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);
                    o.inscatter = InScattering(_WorldSpaceCameraPos, o.posWorld, extinction);
                    o.extinction = extinction;
                #endif
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID(i);

                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float2 node_8491 = (i.uv0*_Tiling);
                float4 _Rock_n_var = tex2D(_Rock_n,TRANSFORM_TEX(node_8491, _Rock_n));
                float node_5486 = 2.0;
                float node_2931 = 1.0;
                float node_8730 = ((_Rock_n_var.r*node_5486)-node_2931);
                float node_679 = ((_Rock_n_var.g*node_5486)-node_2931);
                float3 node_6380 = float3(node_8730,node_679,sqrt(((node_2931-pow(node_8730,node_5486))-pow(node_679,node_5486))));
                float4 _Grass_n_var = tex2D(_Grass_n,TRANSFORM_TEX(node_8491, _Grass_n));
                float node_3203 = 2.0;
                float node_3483 = 1.0;
                float node_1959 = ((_Grass_n_var.r*node_3203)-node_3483);
                float node_6616 = ((_Grass_n_var.g*node_3203)-node_3483);
                float4 _Rock_var = tex2D(_Rock,TRANSFORM_TEX(node_8491, _Rock));
                float4 _Alpha1_var = tex2D(_Alpha1,TRANSFORM_TEX(i.uv0, _Alpha1));
                float4 _Grass_var = tex2D(_Grass,TRANSFORM_TEX(node_8491, _Grass));
                float node_3903 = step((_Rock_var.a*(1.0 - _Alpha1_var.g)),(_Grass_var.a*_Alpha1_var.g));
                float4 _Grass2_n_var = tex2D(_Grass2_n,TRANSFORM_TEX(node_8491, _Grass2_n));
                float node_9270 = 2.0;
                float node_8640 = 1.0;
                float node_2920 = ((_Grass2_n_var.r*node_9270)-node_8640);
                float node_960 = ((_Grass2_n_var.g*node_9270)-node_8640);
                float4 _Sha_n_var = tex2D(_Sha_n,TRANSFORM_TEX(node_8491, _Sha_n));
                float node_7103 = 2.0;
                float node_4311 = 1.0;
                float node_8507 = ((_Sha_n_var.r*node_7103)-node_4311);
                float node_1337 = ((_Sha_n_var.g*node_7103)-node_4311);
                float4 _Alpha2_var = tex2D(_Alpha2,TRANSFORM_TEX(i.uv0, _Alpha2));
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 node_7066_nrm_base = lerp(lerp(lerp(node_6380,float3(node_1959,node_6616,sqrt(((node_3483-pow(node_1959,node_3203))-pow(node_6616,node_3203)))),node_3903),float3(node_2920,node_960,sqrt(((node_8640-pow(node_2920,node_9270))-pow(node_960,node_9270)))),_Alpha1_var.a),lerp(node_6380,float3(node_8507,node_1337,sqrt(((node_4311-pow(node_8507,node_7103))-pow(node_1337,node_7103)))),_Alpha2_var.r),_Alpha2_var.b) + float3(0,0,1);
                float3 node_7066_nrm_detail = _Normal_var.rgb * float3(-1,-1,1);
                float3 node_7066_nrm_combined = node_7066_nrm_base*dot(node_7066_nrm_base, node_7066_nrm_detail)/node_7066_nrm_base.z - node_7066_nrm_detail;
                float3 node_7066 = node_7066_nrm_combined;
                float3 normalLocal = node_7066;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float node_9598 = lerp(lerp(lerp(_Rock_n_var.b,_Grass_n_var.b,node_3903),_Grass2_n_var.b,_Alpha1_var.a),lerp(_Rock_n_var.b,_Sha_n_var.b,_Alpha2_var.r),_Alpha2_var.b);
                float gloss = 1.0 - node_9598; // Convert roughness to gloss
                float perceptualRoughness = node_9598;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = 0.0;
                float specularMonochrome;
                float4 _Grass2_var = tex2D(_Grass2,TRANSFORM_TEX(node_8491, _Grass2));
                float4 _sha_var = tex2D(_sha,TRANSFORM_TEX(node_8491, _sha));
                float3 node_3271 = lerp(_Rock_var.rgb,_sha_var.rgb,_Alpha2_var.r);
                float3 diffuseColor = lerp(lerp(lerp(_Rock_var.rgb,lerp(_Grass_var.rgb,(_Grass_var.rgb*float3(0.1729022,0.2867647,0.2093382)),_Alpha1_var.r),node_3903),_Grass2_var.rgb,_Alpha1_var.a),lerp(node_3271,(node_3271*float3(0.6470588,0.6470588,0.6470588)),_Alpha2_var.g),_Alpha2_var.b); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);

                #ifdef _SKYENABLE
                    finalRGBA.rgb = finalRGBA.rgb * i.extinction + i.inscatter;
                #endif

                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Alpha1; uniform float4 _Alpha1_ST;
            uniform sampler2D _Rock; uniform float4 _Rock_ST;
            uniform sampler2D _Grass; uniform float4 _Grass_ST;
            uniform float _Tiling;
            uniform sampler2D _Alpha2; uniform float4 _Alpha2_ST;
            uniform sampler2D _sha; uniform float4 _sha_ST;
            uniform sampler2D _Grass2; uniform float4 _Grass2_ST;
            uniform sampler2D _Grass2_n; uniform float4 _Grass2_n_ST;
            uniform sampler2D _Grass_n; uniform float4 _Grass_n_ST;
            uniform sampler2D _Rock_n; uniform float4 _Rock_n_ST;
            uniform sampler2D _Sha_n; uniform float4 _Sha_n_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                UNITY_SETUP_INSTANCE_ID(i);
                
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float2 node_8491 = (i.uv0*_Tiling);
                float4 _Rock_n_var = tex2D(_Rock_n,TRANSFORM_TEX(node_8491, _Rock_n));
                float node_5486 = 2.0;
                float node_2931 = 1.0;
                float node_8730 = ((_Rock_n_var.r*node_5486)-node_2931);
                float node_679 = ((_Rock_n_var.g*node_5486)-node_2931);
                float3 node_6380 = float3(node_8730,node_679,sqrt(((node_2931-pow(node_8730,node_5486))-pow(node_679,node_5486))));
                float4 _Grass_n_var = tex2D(_Grass_n,TRANSFORM_TEX(node_8491, _Grass_n));
                float node_3203 = 2.0;
                float node_3483 = 1.0;
                float node_1959 = ((_Grass_n_var.r*node_3203)-node_3483);
                float node_6616 = ((_Grass_n_var.g*node_3203)-node_3483);
                float4 _Rock_var = tex2D(_Rock,TRANSFORM_TEX(node_8491, _Rock));
                float4 _Alpha1_var = tex2D(_Alpha1,TRANSFORM_TEX(i.uv0, _Alpha1));
                float4 _Grass_var = tex2D(_Grass,TRANSFORM_TEX(node_8491, _Grass));
                float node_3903 = step((_Rock_var.a*(1.0 - _Alpha1_var.g)),(_Grass_var.a*_Alpha1_var.g));
                float4 _Grass2_n_var = tex2D(_Grass2_n,TRANSFORM_TEX(node_8491, _Grass2_n));
                float node_9270 = 2.0;
                float node_8640 = 1.0;
                float node_2920 = ((_Grass2_n_var.r*node_9270)-node_8640);
                float node_960 = ((_Grass2_n_var.g*node_9270)-node_8640);
                float4 _Sha_n_var = tex2D(_Sha_n,TRANSFORM_TEX(node_8491, _Sha_n));
                float node_7103 = 2.0;
                float node_4311 = 1.0;
                float node_8507 = ((_Sha_n_var.r*node_7103)-node_4311);
                float node_1337 = ((_Sha_n_var.g*node_7103)-node_4311);
                float4 _Alpha2_var = tex2D(_Alpha2,TRANSFORM_TEX(i.uv0, _Alpha2));
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 node_7066_nrm_base = lerp(lerp(lerp(node_6380,float3(node_1959,node_6616,sqrt(((node_3483-pow(node_1959,node_3203))-pow(node_6616,node_3203)))),node_3903),float3(node_2920,node_960,sqrt(((node_8640-pow(node_2920,node_9270))-pow(node_960,node_9270)))),_Alpha1_var.a),lerp(node_6380,float3(node_8507,node_1337,sqrt(((node_4311-pow(node_8507,node_7103))-pow(node_1337,node_7103)))),_Alpha2_var.r),_Alpha2_var.b) + float3(0,0,1);
                float3 node_7066_nrm_detail = _Normal_var.rgb * float3(-1,-1,1);
                float3 node_7066_nrm_combined = node_7066_nrm_base*dot(node_7066_nrm_base, node_7066_nrm_detail)/node_7066_nrm_base.z - node_7066_nrm_detail;
                float3 node_7066 = node_7066_nrm_combined;
                float3 normalLocal = node_7066;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float node_9598 = lerp(lerp(lerp(_Rock_n_var.b,_Grass_n_var.b,node_3903),_Grass2_n_var.b,_Alpha1_var.a),lerp(_Rock_n_var.b,_Sha_n_var.b,_Alpha2_var.r),_Alpha2_var.b);
                float gloss = 1.0 - node_9598; // Convert roughness to gloss
                float perceptualRoughness = node_9598;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = 0.0;
                float specularMonochrome;
                float4 _Grass2_var = tex2D(_Grass2,TRANSFORM_TEX(node_8491, _Grass2));
                float4 _sha_var = tex2D(_sha,TRANSFORM_TEX(node_8491, _sha));
                float3 node_3271 = lerp(_Rock_var.rgb,_sha_var.rgb,_Alpha2_var.r);
                float3 diffuseColor = lerp(lerp(lerp(_Rock_var.rgb,lerp(_Grass_var.rgb,(_Grass_var.rgb*float3(0.1729022,0.2867647,0.2093382)),_Alpha1_var.r),node_3903),_Grass2_var.rgb,_Alpha1_var.a),lerp(node_3271,(node_3271*float3(0.6470588,0.6470588,0.6470588)),_Alpha2_var.g),_Alpha2_var.b); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
