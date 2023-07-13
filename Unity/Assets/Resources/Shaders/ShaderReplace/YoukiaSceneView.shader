Shader "YoukiaEngine/ShaderReplace/YoukiaSceneView" 
{
	Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    CGINCLUDE
        #include "UnityCG.cginc"

        #pragma fragmentoption ARB_precision_hint_fastest 

        sampler2D _MainTex, _gUVCheckTex;
        half4 _MainTex_ST;

        struct v2f 
        {
            half4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
        };

        v2f vert (appdata_full v) 
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
            return o; 
        }		

        fixed4 frag (v2f i) : SV_Target 
        {
            fixed4 tex = tex2D(_gUVCheckTex, i.uv);
            fixed4 main = tex2D(_MainTex, i.uv);
            //tex.rgb *= main.rgb;
            return tex;
        }

    ENDCG

    SubShader 
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
            ENDCG
        }
    }

    SubShader 
    {
        Tags { "RenderType" = "TransparentCutout" }

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
            ENDCG
        }
    }

    SubShader 
    {
        Tags { "RenderType" = "Transparent" }

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
            ENDCG
        }
    }

    SubShader 
    {
        Tags { "RenderType" = "Background" }

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
            ENDCG
        }
    }
}
