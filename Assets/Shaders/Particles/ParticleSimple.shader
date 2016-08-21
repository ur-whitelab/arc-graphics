//TODO: Use a geomtry shader instead of passing in the quad/billboard information.
//that should save some calls./

Shader "Particles/ParticleSimple"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 0.5, 0.5, 1)
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" }
		LOD 100
		ZTest Off
		ZWrite Off
		Cull Off
		Blend SrcAlpha One

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			
			#include "UnityCG.cginc"
			#include "../Compute/DataTypes.cginc"

			StructuredBuffer<float2> positions;
			StructuredBuffer<ParticleProperties> properties;
			StructuredBuffer<float3> quadPoints;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color: COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			
			// vertex shader with no inputs
			// uses the system values SV_VertexID and SV_InstanceID to read from compute buffers
			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f o;
					

				float3 worldPosition = float3(positions[inst], 0);
				float3 quadPoint = quadPoints[id];

				o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_V, float4(worldPosition, 1.0f)) + float4(quadPoint, 0.0f));


				if (properties[inst].state == PARTICLE_STATE_DEAD) {
					o.pos.w = 0; //bit of a hack. Causes the pixel value to be out of clip
				}

				o.uv = quadPoints[id] + 0.5f;

				//for now we don't do any fancy color stuff
				o.color = properties[inst].color;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture color
				float4 texCol = tex2D(_MainTex, i.uv);

				//get our color choice
				float4 partCol = i.color;

				//blend them with alpha-blending
				return float4 (1.0f - (1.0f - texCol.rgb) * (1.0f - partCol.rgb), texCol.a * i.color.a);
			}
			ENDCG
		}
	}
}
