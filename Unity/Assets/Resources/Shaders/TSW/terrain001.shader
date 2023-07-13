// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.6397059,fgcg:0.7912779,fgcb:1,fgca:1,fgde:0.01,fgrn:0,fgrf:100,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9803,x:34065,y:33423,varname:node_9803,prsc:2|diff-5251-OUT,normal-2245-OUT;n:type:ShaderForge.SFN_Tex2d,id:2004,x:33340,y:34497,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:node_2004,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:3772c40c10349fd408987f722de152ec,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Tex2d,id:1446,x:31811,y:32652,ptovrint:False,ptlb:Blend01,ptin:_Blend01,varname:node_1446,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:3cdeb0b6550d19349b4731e44d23adc0,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:2370,x:31811,y:32846,ptovrint:False,ptlb:Blend02,ptin:_Blend02,varname:node_2370,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:eadc404dce62408439dfa960981050d2,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:4505,x:32453,y:32456,ptovrint:False,ptlb:node_4505,ptin:_node_4505,varname:node_4505,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Tex2d,id:3870,x:31801,y:32451,ptovrint:False,ptlb:Grass,ptin:_Grass,varname:node_3870,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ff96279ef85769347b7f854bffe9fea4,ntxv:0,isnm:False|UVIN-2199-OUT;n:type:ShaderForge.SFN_TexCoord,id:1727,x:31022,y:33610,varname:node_1727,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:2199,x:31240,y:33676,varname:node_2199,prsc:2|A-1727-UVOUT,B-2429-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2429,x:31022,y:33781,ptovrint:False,ptlb:Tiling,ptin:_Tiling,varname:node_2429,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Lerp,id:7650,x:32453,y:32618,varname:node_7650,prsc:2|A-4262-OUT,B-3870-RGB,T-1446-R;n:type:ShaderForge.SFN_Tex2d,id:9978,x:32012,y:33664,ptovrint:False,ptlb:Rock,ptin:_Rock,varname:node_9978,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:adc029c20ad7b9949b10fd11ed487901,ntxv:0,isnm:False|UVIN-2199-OUT;n:type:ShaderForge.SFN_Lerp,id:5835,x:32766,y:32618,varname:node_5835,prsc:2|A-7650-OUT,B-4505-RGB,T-2370-R;n:type:ShaderForge.SFN_RemapRange,id:2999,x:32474,y:33872,varname:node_2999,prsc:2,frmn:0,frmx:1,tomn:-2,tomx:1|IN-1446-B;n:type:ShaderForge.SFN_Clamp01,id:5815,x:32660,y:33872,varname:node_5815,prsc:2|IN-2999-OUT;n:type:ShaderForge.SFN_Lerp,id:5251,x:33675,y:33419,varname:node_5251,prsc:2|A-3345-OUT,B-9536-OUT,T-5815-OUT;n:type:ShaderForge.SFN_Multiply,id:4262,x:32209,y:32432,varname:node_4262,prsc:2|A-4469-RGB,B-3870-RGB;n:type:ShaderForge.SFN_Color,id:4469,x:31801,y:32283,ptovrint:False,ptlb:node_4469,ptin:_node_4469,varname:node_4469,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:7280,x:33016,y:32940,varname:node_7280,prsc:2|A-5835-OUT,B-4134-RGB,T-2370-G;n:type:ShaderForge.SFN_Color,id:4134,x:32654,y:32788,ptovrint:False,ptlb:node_4134,ptin:_node_4134,varname:node_4134,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:3345,x:33265,y:33107,varname:node_3345,prsc:2|A-7280-OUT,B-3732-RGB,T-2370-B;n:type:ShaderForge.SFN_Tex2d,id:3732,x:32786,y:33029,ptovrint:False,ptlb:Grass2,ptin:_Grass2,varname:node_3732,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:7426106b0c3c195478d54ccf990f725f,ntxv:0,isnm:False|UVIN-2199-OUT;n:type:ShaderForge.SFN_Lerp,id:5940,x:32474,y:33695,varname:node_5940,prsc:2|A-3417-OUT,B-9978-RGB,T-1446-R;n:type:ShaderForge.SFN_Color,id:7469,x:32012,y:33428,ptovrint:False,ptlb:node_7469,ptin:_node_7469,varname:node_7469,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:3417,x:32275,y:33478,varname:node_3417,prsc:2|A-7469-RGB,B-9978-RGB;n:type:ShaderForge.SFN_Lerp,id:9359,x:32836,y:33694,varname:node_9359,prsc:2|A-5940-OUT,B-9848-RGB,T-2370-R;n:type:ShaderForge.SFN_Color,id:9848,x:32610,y:33455,ptovrint:False,ptlb:node_9848,ptin:_node_9848,varname:node_9848,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:5726,x:33112,y:33674,varname:node_5726,prsc:2|A-9359-OUT,B-6407-RGB,T-2370-G;n:type:ShaderForge.SFN_Color,id:6407,x:32877,y:33448,ptovrint:False,ptlb:node_6407,ptin:_node_6407,varname:node_6407,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:9536,x:33399,y:33657,varname:node_9536,prsc:2|A-5726-OUT,B-6601-RGB,T-2370-B;n:type:ShaderForge.SFN_Color,id:6601,x:33153,y:33442,ptovrint:False,ptlb:node_6601,ptin:_node_6601,varname:node_6601,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:2403,x:32963,y:34268,varname:node_2403,prsc:2|A-996-RGB,B-268-RGB,T-2370-B;n:type:ShaderForge.SFN_Lerp,id:6378,x:33340,y:34305,varname:node_6378,prsc:2|A-2403-OUT,B-9012-RGB,T-5815-OUT;n:type:ShaderForge.SFN_Tex2d,id:996,x:32099,y:34170,ptovrint:False,ptlb:Grass_n,ptin:_Grass_n,varname:node_996,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:21afa1773181fb947a122a3edc97a27e,ntxv:3,isnm:True|UVIN-2199-OUT;n:type:ShaderForge.SFN_Tex2d,id:268,x:32099,y:34336,ptovrint:False,ptlb:Grass2_n,ptin:_Grass2_n,varname:node_268,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:4d765e036a2d58e4589fb095dd64f38a,ntxv:3,isnm:True|UVIN-2199-OUT;n:type:ShaderForge.SFN_Tex2d,id:9012,x:32099,y:34531,ptovrint:False,ptlb:Rock_n,ptin:_Rock_n,varname:node_9012,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:810741128d08e2a45b15724100ffb8c4,ntxv:3,isnm:True|UVIN-2199-OUT;n:type:ShaderForge.SFN_NormalBlend,id:2245,x:33541,y:34345,varname:node_2245,prsc:2|BSE-6378-OUT,DTL-2004-RGB;proporder:2429-2004-1446-2370-4505-4469-4134-3870-3732-9978-7469-9848-6407-6601-996-268-9012;pass:END;sub:END;*/

Shader "Custom/TSW/terrain001" {
    Properties {
        _Tiling ("Tiling", Float ) = 1
        _Normal ("Normal", 2D) = "bump" {}
        _Blend01 ("Blend01", 2D) = "white" {}
        _Blend02 ("Blend02", 2D) = "white" {}
        _node_4505 ("node_4505", Color) = (0.5,0.5,0.5,1)
        _node_4469 ("node_4469", Color) = (0.5,0.5,0.5,1)
        _node_4134 ("node_4134", Color) = (0.5,0.5,0.5,1)
        _Grass ("Grass", 2D) = "white" {}
        _Grass2 ("Grass2", 2D) = "white" {}
        _Rock ("Rock", 2D) = "white" {}
        _node_7469 ("node_7469", Color) = (0.5,0.5,0.5,1)
        _node_9848 ("node_9848", Color) = (0.5,0.5,0.5,1)
        _node_6407 ("node_6407", Color) = (0.5,0.5,0.5,1)
        _node_6601 ("node_6601", Color) = (0.5,0.5,0.5,1)
        _Grass_n ("Grass_n", 2D) = "bump" {}
        _Grass2_n ("Grass2_n", 2D) = "bump" {}
        _Rock_n ("Rock_n", 2D) = "bump" {}
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
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Blend01; uniform float4 _Blend01_ST;
            uniform sampler2D _Blend02; uniform float4 _Blend02_ST;
            uniform float4 _node_4505;
            uniform sampler2D _Grass; uniform float4 _Grass_ST;
            uniform float _Tiling;
            uniform sampler2D _Rock; uniform float4 _Rock_ST;
            uniform float4 _node_4469;
            uniform float4 _node_4134;
            uniform sampler2D _Grass2; uniform float4 _Grass2_ST;
            uniform float4 _node_7469;
            uniform float4 _node_9848;
            uniform float4 _node_6407;
            uniform float4 _node_6601;
            uniform sampler2D _Grass_n; uniform float4 _Grass_n_ST;
            uniform sampler2D _Grass2_n; uniform float4 _Grass2_n_ST;
            uniform sampler2D _Rock_n; uniform float4 _Rock_n_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
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
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
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
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float2 node_2199 = (i.uv0*_Tiling);
                float3 _Grass_n_var = UnpackNormal(tex2D(_Grass_n,TRANSFORM_TEX(node_2199, _Grass_n)));
                float3 _Grass2_n_var = UnpackNormal(tex2D(_Grass2_n,TRANSFORM_TEX(node_2199, _Grass2_n)));
                float4 _Blend02_var = tex2D(_Blend02,TRANSFORM_TEX(i.uv0, _Blend02));
                float3 _Rock_n_var = UnpackNormal(tex2D(_Rock_n,TRANSFORM_TEX(node_2199, _Rock_n)));
                float4 _Blend01_var = tex2D(_Blend01,TRANSFORM_TEX(i.uv0, _Blend01));
                float node_5815 = saturate((_Blend01_var.b*2.0+-1.0));
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 node_2245_nrm_base = lerp(lerp(_Grass_n_var.rgb,_Grass2_n_var.rgb,_Blend02_var.b),_Rock_n_var.rgb,node_5815) + float3(0,0,1);
                float3 node_2245_nrm_detail = _Normal_var.rgb * float3(-1,-1,1);
                float3 node_2245_nrm_combined = node_2245_nrm_base*dot(node_2245_nrm_base, node_2245_nrm_detail)/node_2245_nrm_base.z - node_2245_nrm_detail;
                float3 node_2245 = node_2245_nrm_combined;
                float3 normalLocal = node_2245;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float4 _Grass_var = tex2D(_Grass,TRANSFORM_TEX(node_2199, _Grass));
                float4 _Grass2_var = tex2D(_Grass2,TRANSFORM_TEX(node_2199, _Grass2));
                float4 _Rock_var = tex2D(_Rock,TRANSFORM_TEX(node_2199, _Rock));
                float3 node_5251 = lerp(lerp(lerp(lerp(lerp((_node_4469.rgb*_Grass_var.rgb),_Grass_var.rgb,_Blend01_var.r),_node_4505.rgb,_Blend02_var.r),_node_4134.rgb,_Blend02_var.g),_Grass2_var.rgb,_Blend02_var.b),lerp(lerp(lerp(lerp((_node_7469.rgb*_Rock_var.rgb),_Rock_var.rgb,_Blend01_var.r),_node_9848.rgb,_Blend02_var.r),_node_6407.rgb,_Blend02_var.g),_node_6601.rgb,_Blend02_var.b),node_5815);
                float3 diffuseColor = node_5251;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
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
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform sampler2D _Blend01; uniform float4 _Blend01_ST;
            uniform sampler2D _Blend02; uniform float4 _Blend02_ST;
            uniform float4 _node_4505;
            uniform sampler2D _Grass; uniform float4 _Grass_ST;
            uniform float _Tiling;
            uniform sampler2D _Rock; uniform float4 _Rock_ST;
            uniform float4 _node_4469;
            uniform float4 _node_4134;
            uniform sampler2D _Grass2; uniform float4 _Grass2_ST;
            uniform float4 _node_7469;
            uniform float4 _node_9848;
            uniform float4 _node_6407;
            uniform float4 _node_6601;
            uniform sampler2D _Grass_n; uniform float4 _Grass_n_ST;
            uniform sampler2D _Grass2_n; uniform float4 _Grass2_n_ST;
            uniform sampler2D _Rock_n; uniform float4 _Rock_n_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
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
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
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
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float2 node_2199 = (i.uv0*_Tiling);
                float3 _Grass_n_var = UnpackNormal(tex2D(_Grass_n,TRANSFORM_TEX(node_2199, _Grass_n)));
                float3 _Grass2_n_var = UnpackNormal(tex2D(_Grass2_n,TRANSFORM_TEX(node_2199, _Grass2_n)));
                float4 _Blend02_var = tex2D(_Blend02,TRANSFORM_TEX(i.uv0, _Blend02));
                float3 _Rock_n_var = UnpackNormal(tex2D(_Rock_n,TRANSFORM_TEX(node_2199, _Rock_n)));
                float4 _Blend01_var = tex2D(_Blend01,TRANSFORM_TEX(i.uv0, _Blend01));
                float node_5815 = saturate((_Blend01_var.b*2.0+-1.0));
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)));
                float3 node_2245_nrm_base = lerp(lerp(_Grass_n_var.rgb,_Grass2_n_var.rgb,_Blend02_var.b),_Rock_n_var.rgb,node_5815) + float3(0,0,1);
                float3 node_2245_nrm_detail = _Normal_var.rgb * float3(-1,-1,1);
                float3 node_2245_nrm_combined = node_2245_nrm_base*dot(node_2245_nrm_base, node_2245_nrm_detail)/node_2245_nrm_base.z - node_2245_nrm_detail;
                float3 node_2245 = node_2245_nrm_combined;
                float3 normalLocal = node_2245;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float4 _Grass_var = tex2D(_Grass,TRANSFORM_TEX(node_2199, _Grass));
                float4 _Grass2_var = tex2D(_Grass2,TRANSFORM_TEX(node_2199, _Grass2));
                float4 _Rock_var = tex2D(_Rock,TRANSFORM_TEX(node_2199, _Rock));
                float3 node_5251 = lerp(lerp(lerp(lerp(lerp((_node_4469.rgb*_Grass_var.rgb),_Grass_var.rgb,_Blend01_var.r),_node_4505.rgb,_Blend02_var.r),_node_4134.rgb,_Blend02_var.g),_Grass2_var.rgb,_Blend02_var.b),lerp(lerp(lerp(lerp((_node_7469.rgb*_Rock_var.rgb),_Rock_var.rgb,_Blend01_var.r),_node_9848.rgb,_Blend02_var.r),_node_6407.rgb,_Blend02_var.g),_node_6601.rgb,_Blend02_var.b),node_5815);
                float3 diffuseColor = node_5251;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
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
