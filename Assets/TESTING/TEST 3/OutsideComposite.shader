// Create a material from this shader and assign it to a FullScreen Custom Pass (Custom Pass Volume).


Shader "Hidden/HDRP_OutsideComposite"
{
	Properties
	{
		_WaterRT("Water RT", 2D) = "white" {}
		[HideInInspector]_StencilReadMask("_StencilReadMask", Float) = 0
		[HideInInspector]_StencilRef("_StencilRef", Float) = 0
		}
		SubShader
		{
			Tags { "RenderType" = "Opaque" "Queue" = "Overlay" "RenderPipeline" = "HDRenderPipeline" }
			Cull Off
			ZWrite Off
			ZTest Always


			Pass
			{
			Name "COMPOSITE"


			// only pass where (stencil & ReadMask) == (Ref & ReadMask)
			Stencil
			{
			Ref [_StencilRef]
			ReadMask [_StencilReadMask]
			Comp Equal
			Pass Keep
			}


			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"


			sampler2D _WaterRT;


			half4 frag(v2f_img i) : SV_Target
			{
			#ifdef UNITY_UV_STARTS_AT_TOP
			float2 uv = float2(i.uv.x, 1.0 - i.uv.y);
			#else
			float2 uv = i.uv;
			#endif


			fixed4 col = tex2D(_WaterRT, uv);
			return col;
			} 
		ENDHLSL
		}
	}
} 