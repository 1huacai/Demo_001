Shader "Hidden/uSkyPro/Precompute" { 
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
			Tags { "PreviewType"="Plane" }
	// Transmittance
	Pass { 
			CGPROGRAM
			#include "AtmospherePrecompute.cginc"
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment TransmittancePS
			ENDCG
		}
		
	// Inscatter1T
	Pass { 
			CGPROGRAM
			#include "AtmospherePrecompute.cginc"
			#pragma target 3.0
			#pragma vertex vert_img
			#pragma fragment Inscatter1PS
			ENDCG
		}
	}
	// TODO:
	// Fallback to Shader Model 2.0
	// Too many instruction for d3d9 or d3d11_9x  to fit in SM2, so excluded those in compiler
/*
SubShader { 
			Tags { "PreviewType"="Plane" }
	// Transmittance
	Pass { 
			CGPROGRAM
			#include "AtmospherePrecompute.cginc"
			#pragma vertex vert_img
			#pragma fragment TransmittancePS
			
			#pragma exclude_renderers d3d9 d3d11_9x
//			#pragma only_renderers glcore opengl gles metal
			 
			ENDCG
		}
		
	// Inscatter1T
	Pass { 
			CGPROGRAM
			#include "AtmospherePrecompute.cginc"
			#pragma vertex vert_img
			#pragma fragment Inscatter1PS
			
			#pragma exclude_renderers d3d9 d3d11_9x
//			#pragma only_renderers glcore opengl gles metal
			
			ENDCG
		}
	}
*/

}