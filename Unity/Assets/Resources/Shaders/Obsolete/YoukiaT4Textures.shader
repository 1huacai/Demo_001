Shader "YoukiaEngine/Obsolete/YoukiaT4Textures" 
{
	Properties 
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Splat0 ("Layer 1", 2D) = "white" {}
		// _Splat0Tilling ("Layer 1 tilling", Range(0, 1)) = 0.5
		_Splat1 ("Layer 2", 2D) = "white" {}
		// _Splat1Tilling ("Layer 2 tilling", Range(0, 1)) = 0.5
		_Splat2 ("Layer 3", 2D) = "white" {}
		// _Splat2Tilling ("Layer 3 tilling", Range(0, 1)) = 0.5
		_Splat3 ("Layer 4", 2D) = "white" {}
		// _Splat3Tilling ("Layer 4 tilling", Range(0, 1)) = 0.5
		_Control ("Control (RGBA)", 2D) = "white" {}

		// _RefNumber("Reference number", Int) = 1
	}

	SubShader 
	{
		Tags { "Queue"="AlphaTest+5" "RenderType"="Opaque" "ShadowType" = "ST_Terrain" }

		Pass
		{
			// Stencil 

			// {
			// 	Ref [_RefNumber]
			// 	Comp notequal
			// }
			
			Tags{ "LightMode" = "ForwardBase" }

			ZWrite On
			ZTest LEqual
			Cull Back

			CGPROGRAM
				#pragma multi_compile_fwdbase
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_instancing

                #pragma multi_compile __ _HEIGHTFOG
				#pragma multi_compile __ _UNITY_RENDER
				#pragma multi_compile __ _SKYENABLE
                #pragma multi_compile __ _SUB_LIGHT_S
				// #pragma multi_compile __ _SurfaceDecal

				#pragma vertex vert
				#pragma fragment frag

				#include "../Library/YoukiaLight.cginc"
                #include "../Library/YoukiaEnvironment.cginc"
				#include "../Library/Atmosphere.cginc"
				#include "YoukiaObsolete.cginc"

				#pragma skip_variants POINT_COOKIE DIRECTIONAL_COOKIE SHADOWS_CUBE SHADOWS_DEPTH FOG_EXP FOG_EXP2 FOG_LINEAR
            	#pragma skip_variants VERTEXLIGHT_ON DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK
				
				struct appdata_t
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					half2 texcoord : TEXCOORD0;
					half4 tangent : TANGENT;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					half2 uv_Control : TEXCOORD0;
					half4 uv_Splat0 : TEXCOORD1;
					half4 uv_Splat1 : TEXCOORD2;
                    half4 TtoW0 : TEXCOORD3;  
                    half4 TtoW1 : TEXCOORD4;  
                    half4 TtoW2 : TEXCOORD5;

					float4 worldPos : TEXCOORD6;
					half3 sh : TEXCOORD7;
					fixed3 viewDir : TEXCOORD8;
                    // fixed3 lightDir : TEXCOORD9; //移到 TtoW 中


					#ifdef _SKYENABLE
                        half3 inscatter : TEXCOORD9;
                        half3 extinction : TEXCOORD10;
                    #endif

					#ifdef _UNITY_RENDER
                        UNITY_LIGHTING_COORDS(11, 12)
                    #else
                        fixed4 screenPos : TEXCOORD11; 
                    #endif

					#ifdef _HEIGHTFOG
                        half4 heightfog : TEXCOORD13;
                    #endif

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				sampler2D _Control;
				sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
				half4 _Splat0_ST,_Splat1_ST,_Splat2_ST,_Splat3_ST;
				// half _Splat0Tilling, _Splat1Tilling, _Splat2Tilling, _Splat3Tilling;

				fixed4 hash4(fixed2 p)
				{ 
					return frac(sin(fixed4(1.0 + dot(p,fixed2(37.0, 17.0)),
                                        	2.0 + dot(p,fixed2(11.0, 47.0)),
                                        	3.0 + dot(p,fixed2(41.0, 29.0)),
                                        	4.0 + dot(p,fixed2(23.0, 31.0)))) * 103.0); 
				}


				half3 tex2DNoTile(sampler2D samp, in fixed2 uv)
				{
					return tex2D(samp, uv).rgb;
					half2 iuv = half2(floor(uv));
					half2 fuv = frac(uv);

					// generate per-tile transform
					half4 ofa = hash4(iuv + half2(0, 0));
					half4 ofb = hash4(iuv + half2(1, 0));
					half4 ofc = hash4(iuv + half2(0, 1));
					half4 ofd = hash4(iuv + half2(1, 1));
					
					half2 _ddx = ddx(uv);
					half2 _ddy = ddy(uv);

					// transform per-tile uvs
					ofa.zw = sign(ofa.zw - 0.5);
					ofb.zw = sign(ofb.zw - 0.5);
					ofc.zw = sign(ofc.zw - 0.5);
					ofd.zw = sign(ofd.zw - 0.5);

					// fetch and blend
					half2 b = smoothstep(0.25, 0.75, fuv);
					b = saturate(sign(b - 0.5));

					half4 ofDither = lerp(lerp(ofa, ofb, b.x), lerp(ofc, ofd, b.x), b.y);
					half2 ddxyScale = ofDither.zw;
					// return _ddy.yyyy;
					// ddxyScale *= 10;

					half2 finalUV = half2(uv.xy * ofDither.zw + ofDither.xy);
					// _ddx = ddx(finalUV);
					// _ddy = ddy(finalUV);

					return tex2Dgrad(samp, finalUV, _ddx * ddxyScale, _ddy * ddxyScale).rgb;
				}

				// fixed4 tex2DNoTile( sampler2D samp, in fixed2 uv )
				// {
				// 	fixed2 iuv = floor( uv );
				// 	fixed2 fuv = frac( uv );
				
				
				// // #ifdef USEHASH  
				// // 	// generate per-tile transform (needs GL_NEAREST_MIPMAP_LINEARto work right)
				// // 	fixed4 ofa = tex2D( _SecondTex, (iuv + fixed2(0.5,0.5))/256.0 );
				// // 	fixed4 ofb = tex2D( _SecondTex, (iuv + fixed2(1.5,0.5))/256.0 );
				// // 	fixed4 ofc = tex2D( _SecondTex, (iuv + fixed2(0.5,1.5))/256.0 );
				// // 	fixed4 ofd = tex2D( _SecondTex, (iuv + fixed2(1.5,1.5))/256.0 );
				// // #else
				// 	// generate per-tile transform
				// 	fixed4 ofa = hash4( iuv + fixed2(0.0,0.0) );
				// 	fixed4 ofb = hash4( iuv + fixed2(1.0,0.0) );
				// 	fixed4 ofc = hash4( iuv + fixed2(0.0,1.0) );
				// 	fixed4 ofd = hash4( iuv + fixed2(1.0,1.0) );
				// // #endif
				
				// 	fixed2 _ddx = ddx( uv );
				// 	fixed2 _ddy = ddy( uv );
				
				// 	// transform per-tile uvs
				// 	ofa.zw = sign(ofa.zw-0.5);
				// 	ofb.zw = sign(ofb.zw-0.5);
				// 	ofc.zw = sign(ofc.zw-0.5);
				// 	ofd.zw = sign(ofd.zw-0.5);
				
				// 	// uv's, and derivarives (for correct mipmapping)
				// 	fixed2 uva = uv*ofa.zw + ofa.xy; fixed2 ddxa = _ddx*ofa.zw; fixed2 ddya = _ddy*ofa.zw;
				// 	fixed2 uvb = uv*ofb.zw + ofb.xy; fixed2 ddxb = _ddx*ofb.zw; fixed2 ddyb = _ddy*ofb.zw;
				// 	fixed2 uvc = uv*ofc.zw + ofc.xy; fixed2 ddxc = _ddx*ofc.zw; fixed2 ddyc = _ddy*ofc.zw;
				// 	fixed2 uvd = uv*ofd.zw + ofd.xy; fixed2 ddxd = _ddx*ofd.zw; fixed2 ddyd = _ddy*ofd.zw;
					
				// 	// fetch and blend
				// 	fixed2 b = smoothstep(0.25,0.75,fuv);
				
				// 	return lerp( lerp( tex2Dgrad ( samp, uva, ddxa, ddya ),
				// 					tex2Dgrad( samp, uvb, ddxb, ddyb ), b.x ),
				// 					lerp( tex2Dgrad ( samp, uvc, ddxc, ddyc ),
				// 					tex2Dgrad ( samp, uvd, ddxd, ddyd ), b.x), b.y );
				// }

				v2f vert(appdata_t v)
				{
					v2f o;
					UNITY_INITIALIZE_OUTPUT(v2f,o);
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv_Control = v.texcoord;
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);

					o.uv_Splat0.xy = TRANSFORM_TEX(v.texcoord, _Splat0);
					o.uv_Splat0.zw = TRANSFORM_TEX(v.texcoord, _Splat1);
					o.uv_Splat1.xy = TRANSFORM_TEX(v.texcoord, _Splat2);
					o.uv_Splat1.zw = TRANSFORM_TEX(v.texcoord, _Splat3);


                    half3 worldNormal = UnityObjectToWorldNormal(v.normal);  
                    half3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
                    half3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
					half3 lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));

					o.TtoW0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, lightDir.x);
                    o.TtoW1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, lightDir.y);
                    o.TtoW2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, lightDir.z);

					// o.normal = worldNormal;
					o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));

					


					#ifdef _UNITY_RENDER
                        UNITY_TRANSFER_LIGHTING(o, v.texcoord);
                    #else
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif

					#if defined (UNITY_UV_STARTS_AT_TOP)
                        o.sh = ShadeSHPerVertex(worldNormal, o.sh);
                    #endif
					o.sh += YoukiaGI_IndirectDiffuse(worldNormal);

					// height fog
                    #ifdef _HEIGHTFOG
                        o.heightfog.rgb = YoukiaHeightFog(o.worldPos, 0, o.heightfog.a);
                    #endif

                    // atmosphere
                    #ifdef _SKYENABLE
                        o.inscatter = InScattering(_WorldSpaceCameraPos, o.worldPos, o.extinction);
                    #endif

					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
					UNITY_SETUP_INSTANCE_ID(i);

					return OBSOLETECOLOR;
					
					half3 viewDir = i.viewDir;
                    half3 lightDir = half3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);

					fixed4 splat_control = tex2D(_Control, i.uv_Control).rgba;

					fixed3 lay1 = tex2DNoTile(_Splat0, i.uv_Splat0.xy);
					fixed3 lay2 = tex2DNoTile(_Splat1, i.uv_Splat0.zw);
					fixed3 lay3 = tex2DNoTile(_Splat2, i.uv_Splat1.xy);
					fixed3 lay4 = tex2DNoTile(_Splat3, i.uv_Splat1.zw);

				
					fixed4 col;
					col.rgb = (lay1 * splat_control.r + lay2 * splat_control.g + lay3 * splat_control.b + lay4 * splat_control.a) * _Color;
					col.a = 1;
					fixed4 albedo = col;


					// half3 normal = i.normal;
					half3 normal = half3(i.TtoW0.z, i.TtoW1.z, i.TtoW2.z);
					// #ifdef _SurfaceDecal
					// 	YoukiaSurfaceDecal(i.worldPos, normal);
					// 	normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
					// #endif


                    UNITY_BRANCH
                    if(_gSurfaceDecalParam.x > 0)
                    {
                        YoukiaSurfaceDecal(0, i.worldPos, normal);
                        normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
                    }

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
					YoukiaLightingData data = YoukiaData(col.rgb, normal, viewDir, colShadow, gi.indirect, gi.light);
					
					col.rgb = YoukiaLight(data);


					// sub light
                    // #ifdef _SUB_LIGHT_S
                    //     half intensity = _gVSLIntensity_S;
                    //     UNITY_BRANCH
                    //     if (intensity > 0)
                    //     {
                    //         half3 subLightColor = _gVSLColor_S.rgb * intensity;
					// 		half3 subLightDir = normalize(_gVSLFwdVec_S.xyz);

					// 		UnityGI gi = GetUnityGI_simplify(normal, i.worldPos, subLightDir, viewDir, i.sh);
                    //         YoukiaLightingData data = YoukiaData(col.rgb, normal, viewDir, colShadow, gi.indirect, gi.light);
					// 		col.rgb += YoukiaLight(data);
                    //     }
                    // #endif
					// col.a = 1;


					// height fog
                    #ifdef _HEIGHTFOG
                        col.rgb = lerp(col.rgb, i.heightfog.rgb, i.heightfog.a);
                    #endif

                    #ifdef _SKYENABLE
                        col.rgb = col.rgb * i.extinction + i.inscatter;
                    #endif


					// #ifdef _SurfaceDecal
					// 	col.rgb = topNormal.rgb;
					// #endif

					
					return col;
				}

			ENDCG
		}

	}
	FallBack "VertexLit"
}
