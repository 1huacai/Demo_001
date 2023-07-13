/*=============================================================================
	AtmospherePrecompute.cginc : Precompute data for Atmospheric Scattering

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
#ifndef USKY_ATMOSPHERE_PRECOMPUTED
#define USKY_ATMOSPHERE_PRECOMPUTED

#include "UnityCG.cginc"
#include "AtmosphereCommon.cginc"


float _uSkyAtmosphereThickness;

float DensityM(float r,float xj,float mu,float H)
{

    //return _fogIndex*exp(-(sqrt_new(r * r + xj * xj + 2.0 * xj * r * mu) - Rg) / H);
    // if(sqrt_new(r * r + xj * xj + 2.0 * xj * r * mu) - Rg < (0.02 * 0.02) * (Rt - Rg))
    // {
    //     return _fogIndex;
    // }
    // else if (sqrt_new(r * r + xj * xj + 2.0 * xj * r * mu) - Rg < (0.15*0.15)*(Rt-Rg))
    // {
    //     return (_fogInit+(_fogIndex - _fogInit)/1.326*(sqrt_new(r * r + xj * xj + 2.0 * xj * r * mu)-Rg));
    // }
    // else if(sqrt_new(r * r + xj * xj + 2.0 * xj * r * mu)  < Rg + (0.2 * 0.2) * (Rt-Rg))
    // {
    //     return _fogInit; 
    // }
    if (sqrt_new(r * r + xj * xj + 2.0 * xj * r * mu)  < Rg + (0.2 * 0.2) * (Rt-Rg))
    {
        return (_fogIndex -_fogInit)/2*(cos((sqrt_new(r * r + xj * xj + 2.0 * xj * r * mu)-Rg)*3.141592653/(0.2 * 0.2)/(Rt-Rg))+1)+_fogInit;
    }
    else
    {
        return _fogInit*exp(-(sqrt_new(r * r + xj * xj + 2.0 * xj * r * mu) - Rg - (0.2*0.2)*(Rt-Rg)) / H);
    }
    
}



float DensityR(float r,float xj,float mu,float H)
{
	float a = r * 0.001;
	float b = xj * 0.001;
    return exp(-(sqrt_new(a * a + b * b + 2.0 * b * a * mu)*1000 - Rg) / H);
      
}





// ---------------------------------------------------------------------------- 
// TRANSMITTANCE FUNCTIONS 
// ---------------------------------------------------------------------------- 
float OpticalDepthM(float H, float r, float mu) 
{ 
    float result = 0.0; 
    float dx = Limit(r, mu) / float(TRANSMITTANCE_INTEGRAL_SAMPLES); 
    float xi = 0.0; 
    float yi = exp(-(r - Rg) / H); 
    UNITY_LOOP
    for (int i = 1; i <= TRANSMITTANCE_INTEGRAL_SAMPLES; ++i) 
    { 
        float xj = float(i) * dx; 
        float yj = DensityM(r,xj,mu,H); 
        result += (yi + yj) / 2.0 * dx; 
        xi = xj; 
        yi = yj; 
    }
     
    return mu < -sqrt_new(1.0 - (Rg / r) * (Rg / r)) ? 1e9 : result; 
} 

float OpticalDepthR(float H, float r, float mu) 
{ 
    float result = 0.0; 
    float dx = Limit(r, mu) / float(TRANSMITTANCE_INTEGRAL_SAMPLES); 
    float xi = 0.0; 
    float yi = exp(-(r - Rg) / H); 
	UNITY_LOOP
    for (int i = 1; i <= TRANSMITTANCE_INTEGRAL_SAMPLES; ++i) 
    { 
        float xj = float(i) * dx; 
        float yj = DensityR(r,xj,mu,H); 
        result += (yi + yj) / 2.0 * dx; 
        xi = xj; 
        yi = yj; 
    }

	return mu < -sqrt_new(1.0 - (Rg / r) * (Rg / r)) ? 1e9 : result;
} 

float4 TransmittancePS(v2f_img IN) : SV_Target
{
	
    float r, muS; 
    GetTransmittanceRMu(IN.uv, r, muS);  

    //float betaIndex = max(1,(60-_uSkyAtmosphereThickness*59));

	float3 depth = max(_Rlinear * _Rcolor * betaR.xyz * OpticalDepthR(_Rexp, r, muS), 0) + max(_Mlinear * betaMEx * OpticalDepthM(_Mexp, r, muS), 0);
    //float3 depth = max(_Rlinear * _Rcolor * betaR.xyz * r, 0) + max(_Mlinear * _Mcolor * betaMEx * r, 0);
    // depth = saturate(depth);
    
	// if (_fogIndex < 15 &&  _fogInit <30)
    // {
	 	return float4(exp(-depth), 1.0); // Eq (5)
	// 	//return float4(OpticalDepthR(_Rexp, r, muS).xxx, 1.0); // Eq (5)
    // } 
    // else
    // {
    //     return float4(depth,1.0);
    // }
	
}

// ---------------------------------------------------------------------------- 
// INSCATTER FUNCTIONS 
// ---------------------------------------------------------------------------- 
void Integrand(float r, float mu, float muS, float nu, float t, out float3 ray, out float mie) 
{ 
    ray = float3(0,0,0); 
    mie = 0.0;
    float ri = sqrt_new(r * r + t * t + 2.0 * r * mu * t); 
    float muSi = (nu * t + muS * r) / (ri * lerp(1.0, betaR.w, max(0.0, muS))); // added betaR.w to fix the Rayleigh Offset artifacts issue
    ri = max(Rg, ri);
    float3 ti = 0;
    if (muSi >= -sqrt_new(1.0 - Rg * Rg / (ri * ri ))) 
    { 
        // if (_fogIndex < 15 &&  _fogInit <30)
        // {
            ti = Transmittance(r, mu, t) * Transmittance(ri, muSi);
        // }
        // else
        // {
        //     ti = exp(-Transmittance(r, mu, t) - Transmittance(ri, muSi));
        // }
		ray = exp(-(ri - Rg) / _Rexp) * ti; //OpticalDepthR(_Rexp, r, muS) * ti;//exp(-(ri - Rg) / HR) * ti; 
		mie = exp(-(ri - Rg) / _Mexp) * ti; // OpticalDepthR(_Mexp, r, muS) * ti;//DensityM(ri,0,mu,HM) * ti; // only calc the red channel
    } 
} 

void Inscatter(float r, float mu, float muS, float nu, out float3 ray, out float mie) 
{ 
	int Num = INSCATTER_INTEGRAL_SAMPLES + (int)((1.0 - abs(mu)) * 100.0);
    ray = float3(0,0,0); 
    mie = 0.0;
    float dx = Limit(r, mu) / float(Num);
    float xi = 0.0; 
    float3 rayi; 
    float miei; 
    Integrand(r, mu, muS, nu, 0.0, rayi, miei); 
	UNITY_LOOP
    for (int i = 1; i <= Num; ++i)
    { 
        float xj = float(i) * dx; 
        float3 rayj; 
        float miej; 
        Integrand(r, mu, muS, nu, xj, rayj, miej); 
        
        ray += (rayi + rayj) / 2.0 * dx;
        mie += (miei + miej) / 2.0 * dx;
        xi = xj; 
        rayi = rayj;
        miei = miej;
    } 
    //betaMSca = 1-(1-betaMSca)*(1-betaMSca);



    
    ray *= _Rlinear * ( _Rcolor) * betaR.xyz;
    mie *= _Mlinear * ( _Mcolor) * betaMSca.x;
} 

float4 Inscatter1PS(v2f_img IN) : SV_Target
{	
    float3 ray;
    float mie; // only calc the red channel
    float4 dhdH;
    float mu, muS, nu, r;
  float2 coords = IN.uv.xy; // range 0 ~ 1
//	float2 coords = float2 ( IN.uv.x * float(RES_MU_S * RES_NU), IN.uv.y * float(RES_MU));

// ----------------------------------------
//  uSkypro custom altitude layer override
// ----------------------------------------

// Total range of depth/layers is 0 ~ 31.
float3 uvLayer;

//uvLayer = float3(coords, RES_R-1);// one layer

//for (int i = 0; i < RES_R;i++){ // TODO: not working yet (WIP)
//	if(coords.y > (i/RES_R))
//		uvLayer = float3(coords.x, coords.y * RES_R - i, i);
//	else
//		uvLayer = float3(coords, i);
//}
 
if (RES_R >3){
	// hard coded to split the depth to 4 layer
    // Texture size = 256 x 512
    uvLayer =	coords.y > 0.75?	float3(coords.x, coords.y * RES_R -3, 16):	// atmosphere level layer
    			coords.y > 0.5 ?	float3(coords.x, coords.y * RES_R -2, 4): 
				coords.y > 0.25?	float3(coords.x, coords.y * RES_R -1, 2): 
									float3(coords.x, coords.y * RES_R	, 0);	// ground level layer
}else	
/*								
if (RES_R > 1){
	// hard coded to split the depth to 2 layer (upper half = clouds level layer, lower half = ground level layer) 
	// Texture size is 256 x 256
	uvLayer = coords.y > 0.5? float3(coords.x, coords.y * RES_R -1, 4):float3(coords.x, coords.y * RES_R, 0);

 }else
 */
 {
 	// One layer only, Texture size is 256 x 128
	uvLayer = float3(coords, 2);
}
 
// ------------------------------------


    GetLayer((int)uvLayer.z, r, dhdH); 
    GetMuMuSNu(uvLayer.xy, r, dhdH, mu, muS, nu); 
  
    Inscatter(r, mu, muS, nu, ray, mie); 
    
	// store only red component of single Mie scattering (cf. 'Angular precision')
	return float4(ray,mie);
}

#endif // USKY_ATMOSPHERE_PRECOMPUTED