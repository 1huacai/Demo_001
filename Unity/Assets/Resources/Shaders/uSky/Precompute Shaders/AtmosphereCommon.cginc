/*=============================================================================
	AtmosphereCommon.cginc : Functions and variables shared by both rendering and precomputation

	This code contains embedded portions of free sample source code from 
	http://www-evasion.imag.fr/Membres/Eric.Bruneton/PrecomputedAtmosphericScattering2.zip, Author: Eric Bruneton, 
	08/16/2011, Copyright (c) 2008 INRIA, All Rights Reserved, which have been altered from their original version.

	Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

    1. Redistributions of source code must retain the above copyright notice, 
	   this list of conditions and the following disclaimer.
    2. Redistributions in binary form must reproduce the above copyright notice, 
	   this list of conditions and the following disclaimer in the
       documentation and/or other materials provided with the distribution.
    3. Neither the name of the copyright holders nor the names of its
       contributors may be used to endorse or promote products derived from
       this software without specific prior written permission.
       
	Author: Eric Bruneton
	Modified and ported to Unity by Justin Hawkins 2014 
	Modified by Michael Lam 2015
=============================================================================*/
#ifndef USKY_ATMOSPHERE_COMMON
#define USKY_ATMOSPHERE_COMMON
// ---------------------------------------------------------------------------- 
// PHYSICAL MODEL PARAMETERS 
// ----------------------------------------------------------------------------
 
//The radius of the planet (Rg), radius of the atmosphere (Rt),  atmosphere limit (RL)
const static float Rg = 6360.0;
const static float Rt = 6420.0;
const static float RL = 6421.0;

//Half heights for the atmosphere air density (HR) and particle density (HM)
//This is the height in km that half the particles are found below
const static float HR = 8;
const static float HM = 10;

//const static float3 betaR = float3(5.8e-3, 1.35e-2, 3.31e-2);
uniform float4 betaR;

const static float3 betaMSca = float3(4e-3, 4e-3, 4e-3);
const static float3 betaMEx = betaMSca / 0.9;

// ---------------------------------------------------------------------------- 
// CONSTANT PARAMETERS 
// ---------------------------------------------------------------------------- 

const static int TRANSMITTANCE_H = 256;
const static int TRANSMITTANCE_W = 64;
const static int SKY_W = 64;
const static int SKY_H = 16;
uniform int RES_R;
//const static int RES_R = 32;
const static int RES_MU = 128;
const static int RES_MU_S = 32;
const static int RES_NU = 8;

// ---------------------------------------------------------------------------- 
// NUMERICAL INTEGRATION PARAMETERS 
// ---------------------------------------------------------------------------- 
// default Transmittance sample is 500, less then 250 sample will fit in SM 3.0 for dx9,
#define TRANSMITTANCE_INTEGRAL_SAMPLES 250
//default Inscatter sample is 50
#define INSCATTER_INTEGRAL_SAMPLES 25
//#define IRRADIANCE_INTEGRAL_SAMPLES 32 

// ---------------------------------------------------------------------------- 
// PARAMETERIZATION OPTIONS 
// ---------------------------------------------------------------------------- 
 
#define TRANSMITTANCE_NON_LINEAR	
#define INSCATTER_NON_LINEAR		
 
// ---------------------------------------------------------------------------- 
// PARAMETERIZATION FUNCTIONS 
// ---------------------------------------------------------------------------- 
//UNITY_DECLARE_TEX2D ( _MainTex );
sampler2D_float _MainTex;
float4 _MainTex_TexelSize;

// x: mie scale, y: r exp, z: r linear, w: m exp
float4 _uAtmospherePreParam0;
// x: m linear, y: RGBAFloatPoint, z: RES_R
float4 _uAtmospherePreParam1;

#define _MieScale _uAtmospherePreParam0.x
#define _Rexp _uAtmospherePreParam0.y
#define _Rlinear _uAtmospherePreParam0.z
#define _Mexp _uAtmospherePreParam0.w
#define _Mlinear _uAtmospherePreParam1.x
#define _RGBAFloatPoint _uAtmospherePreParam1.y
#define _Mlinear _uAtmospherePreParam1.x

float _fogIndex;
float _fogInit;
float4 _Rcolor;
float4 _Mcolor;

float4 SampleFloatTex(sampler2D tex, float2 uv, float4 texelSize)
{
	// if (_RGBAFloatPoint < 0.5)
	// {
		return tex2Dlod(tex, float4(uv, 0, 0));
	// }
	// else
	// {
	// 	float2 centerPos = floor((uv + texelSize.xy*0.5)*texelSize.zw);
	// 	float2 center = centerPos / texelSize.zw;
	// 	float2 xyLerp = saturate((uv * texelSize.zw - (centerPos - 0.5)));//0.5 - (center.x - uv.x) / texelSize.x;
	// 	float4 c0 = tex2Dlod(tex, float4(center - texelSize.xy*0.5, 0, 0));
	// 	float4 c1 = tex2Dlod(tex, float4(center + texelSize.xy*0.5*float2(1, -1), 0, 0));
	// 	float4 c2 = tex2Dlod(tex, float4(center + texelSize.xy*0.5, 0, 0));
	// 	float4 c3 = tex2Dlod(tex, float4(center + texelSize.xy*0.5*float2(-1, 1), 0, 0));
	// 	return lerp(lerp(c0, c1, xyLerp.x), lerp(c3, c2, xyLerp.x), xyLerp.y);
	// }
}

float mod(float x, float y) { return x - y * floor(x/y); }
  
float sqrt_new(float n) {
	//float err = 0.00001;
	//float root = n*0.001;
	//float count = 0;
	//while ((abs(n - root * root) > err) && count < 1000)
	//{
	//	root = (n / root + root) * 0.5;
	//	count += 1;
	//}
	//return root;

	//////////////////////////////


	return sqrt(n);

	///////////////////////////////////
	//float err = 0.01;
	//float guess = n * 0.5;
	//float ns = 0;
	//for (int i = 0; i < 500; i++)
	//{
	//	ns = guess * guess;
	//	if (abs(ns - n) < err)return guess;
	//	if (guess*guess > n)
	//	{
	//		guess *= 0.5;
	//	}
	//	else
	//	{
	//		guess *= 1.5;
	//	}
	//}
	//return guess;

}

float2 GetTransmittanceUV(float r, float mu) 
{ 
    float uR, uMu; 
#ifdef TRANSMITTANCE_NON_LINEAR 
	uR = sqrt_new((r - Rg) / (Rt - Rg)); 
	uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5; 
#else 
	uR = (r - Rg) / (Rt - Rg); 
	uMu = (mu + 0.15) / (1.0 + 0.15); 
#endif 
    return float2(uMu, uR); 
} 
 
void GetTransmittanceRMu(float2 coord, out float r, out float muS) 
{ 
    r = coord.y; 
    muS = coord.x;
#ifdef TRANSMITTANCE_NON_LINEAR 
    r = Rg + (r * r) * (Rt - Rg);
    muS = -0.15 + tan(1.5 * muS) / tan(1.5) * (1.0 + 0.15); 
#else 
    r = Rg + r * (Rt - Rg); 
    muS = -0.15 + muS * (1.0 + 0.15); 
#endif 
}
 
float2 GetIrradianceUV(float r, float muS) 
{ 
    float uR = (r - Rg) / (Rt - Rg); 
    float uMuS = (muS + 0.2) / (1.0 + 0.2); 
    return float2(uMuS, uR); 
}  

void GetIrradianceRMuS(float2 coord, out float r, out float muS) 
{ 
    r = Rg + (coord.y * float(SKY_H) - 0.5) / (float(SKY_H) - 1.0) * (Rt - Rg); 
    muS = -0.2 + (coord.x * float(SKY_W) - 0.5) / (float(SKY_W) - 1.0) * (1.0 + 0.2);    
}  

//----------------------------------------------------------------------------------------------------

void GetMuMuSNu(float2 coord, float r, float4 dhdH, out float mu, out float muS, out float nu) 
{ 
    float x = coord.x * float(RES_MU_S * RES_NU) - 0.5;
    float y = coord.y * float(RES_MU) - 0.5;
#ifdef INSCATTER_NON_LINEAR 
    if (y < float(RES_MU) / 2.0) 
    { 
        float d = 1.0 - y / (float(RES_MU) / 2.0 - 1.0); 
        d = min(max(dhdH.z, d * dhdH.w), dhdH.w * 0.999); 
        mu = (Rg * Rg - r * r - d * d) / (2.0 * r * d); 
        mu = min(mu, -sqrt_new(1.0 - (Rg / r) * (Rg / r)) - 0.001); 
    } 
    else 
    { 
        float d = (y - float(RES_MU) / 2.0) / (float(RES_MU) / 2.0 - 1.0); 
        d = min(max(dhdH.x, d * dhdH.y), dhdH.y * 0.999); 
        mu = (Rt * Rt - r * r - d * d) / (2.0 * r * d); 
    } 
    muS = mod(x, float(RES_MU_S)) / (float(RES_MU_S) - 1.0); 
    // paper formula 
    //muS = -(0.6 + log(1.0 - muS * (1.0 -  exp(-3.6)))) / 3.0; 
    // better formula 
    muS = tan((2.0 * muS - 1.0 + 0.26) * 1.1) / tan(1.26 * 1.1); 
    nu = -1.0 + floor(x / float(RES_MU_S)) / (float(RES_NU) - 1.0) * 2.0; 
#else 
    mu = -1.0 + 2.0 * y / (float(RES_MU) - 1.0); 
    muS = mod(x, float(RES_MU_S)) / (float(RES_MU_S) - 1.0); 
    muS = -0.2 + muS * 1.2; 
    nu = -1.0 + floor(x / float(RES_MU_S)) / (float(RES_NU) - 1.0) * 2.0; 
#endif 
} 

void GetLayer(int layer, out float r, out float4 dhdH)
{
	// Assign the total depth constant for "RES_R" altitude layer setting in AtmospherePrecompute.cginc.
	const float RES_R_TOTAL = 32;
	
	r = float(layer) / (RES_R_TOTAL - 1.0);
	r = r * r;
	r = sqrt_new(Rg * Rg + r * (Rt * Rt - Rg * Rg)) + (layer == 0 ? 0.01 : (layer == RES_R_TOTAL - 1 ? -0.001 : 0.0));
	
	float dmin = Rt - r;
	float dmax = sqrt_new(r * r - Rg * Rg) + sqrt_new(Rt * Rt - Rg * Rg);
	float dminp = r - Rg;
	float dmaxp = sqrt_new(r * r - Rg * Rg);

	dhdH = float4(dmin, dmax, dminp, dmaxp);	
}
 
// ---------------------------------------------------------------------------- 
// UTILITY FUNCTIONS 
// ---------------------------------------------------------------------------- 

// nearest intersection of ray r,mu with ground or top atmosphere boundary 
// mu=cos(ray zenith angle at ray origin) 
float Limit(float r, float mu) 
{ 
	float a = r * 0.001;
	float b = RL * 0.001;
	float c = Rg * 0.001;
	float err = a * a * (mu * mu - 1.0) + b * b;
	err = sqrt_new((err)) * 1000;
	float dout = -r * mu + err;// sqrt_new(r * r * (mu * mu - 1.0) + RL * RL);
    float delta2 = a * a * (mu * mu - 1.0) + c * c; 

	//float temp = r * r * (mu * mu - 1.0) + RL * RL;
	//temp = sqrt_new((temp));
	//if (err > 0||err<0||err==0)return 5;
	//else return 0;
	//return  (r * r * (mu * mu - 1.0) + RL * RL) == 0 ? 5 : 0.0;//sqrt_new(r * r * (mu * mu - 1.0)*0.0001 + RL * RL*0.0001);// (r * r - RL * RL*0.01) > 40030000 ? 5 : 0.0;// sqrt_new(r*r - RL * RL*0.01) + 1000;
    if (delta2 >= 0.0) 
    { 
        float din = -r * mu - sqrt_new(delta2)*1000; 
        if (din >= 0.0) 
        { 
            dout = min(dout, din); 
        } 
    } 
	return (dout);
}

// transmittance(=transparency) of atmosphere for infinite ray (r,mu) 
// (mu=cos(view zenith angle)), intersections with ground ignored 
float3 Transmittance(float r, float mu) 
{ 
	float2 uv = GetTransmittanceUV(r, mu);
	//return tex2Dlod(_MainTex, float4(uv, 0, 0)).rgb;
	return SampleFloatTex(_MainTex, uv, _MainTex_TexelSize).rgb;


//// To avoid the warning of :
//// gradient-based operations must be moved out of flow control to prevent divergence. 
//// Performance may improve by using a non-gradient operation (on d3d11)
//	#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_PS4)
//    	return _MainTex.SampleLevel (sampler_MainTex, uv, 0).rgb;
////    #elif defined(SHADER_API_D3D9)
////    	return tex2Dlod(_MainTex, float4(uv,0,0)).rgb;
//    #else
//        if (_fogIndex < 15 &&  _fogInit <30)
//        {
//            return UNITY_SAMPLE_TEX2D(_MainTex, uv).rgb;
//        }
//        else
//    	{
//            return UNITY_SAMPLE_TEX2D(_MainTex, uv).rgb/2;
//        }
//        
//	#endif
} 

// transmittance(=transparency) of atmosphere between x and x0 
// assume segment x,x0 not intersecting ground 
// d = distance between x and x0, mu=cos(zenith angle of [x,x0) ray at x) 
float3 Transmittance(float r, float mu, float d)
{ 
    float3 result; 
    float r1 = sqrt_new(r * r + d * d + 2.0 * r * mu * d); 
    float mu1 = (r * mu + d) / r1; 

    // if (_fogIndex < 15 &&  _fogInit <30)
    // {
        if (mu > 0.0)
        {
        	result =Transmittance(r, mu) / (Transmittance(r1, mu1)); 
        } 
		else 
        { 
        	result = Transmittance(r1, -mu1) / (Transmittance(r, -mu));
        } 

		result = saturate(result);
    // }
    // else
    // {
    //     if (mu > 0.0)
    //     {
    //     result =Transmittance(r, mu) - (Transmittance(r1, mu1)); 
    //     } else 
    //     { 
    //     result = Transmittance(r1, -mu1) - (Transmittance(r, -mu));
    //     }
    //     result = result/2;
    // }
    
    return result; 
} 
/*
float3 Irradiance(sampler2D tex, float r, float muS) // not in use 
{ 
    float2 uv = GetIrradianceUV(r, muS); 
    return tex2D(tex, uv).rgb;
}  
*/
#endif // USKY_ATMOSPHERE_COMMON
