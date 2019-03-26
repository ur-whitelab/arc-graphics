// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ColorMatrix" {
	Properties{
		_NumQuadsX("Number of Sections Across", Int) = 2
		_NumQuadsY("Number of Sections Vertical", Int) = 2
		_ColorMap("The texture showing where to color what. DO NOT EDIT", 2D) = "white" {}
	}
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 100
		Pass{
		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		#include "../Compute/DataTypes.cginc"

		int _NumQuadsX;
		int _NumQuadsY;		
		sampler2D _ColorMap;
		float4 _ColorMap_ST;

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		v2f vert(appdata_base v, uint inst : SV_InstanceID)
		{
			v2f o;

			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = (v.texcoord.xy );
			return o;
		}


		fixed4 frag(v2f i) : SV_Target{
			float4 texcol = tex2D(_ColorMap, i.uv.xy);
			return  texcol;
		}
		ENDCG
		}
	}
}