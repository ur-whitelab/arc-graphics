// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//TODO: Use a geomtry shader instead of passing in the quad/billboard information.
//that should save some calls./

Shader "Custom/ParticleBillboard"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
		_Color("Color", Color) = (1, 0.5, 0.5, 1)
		_Size("Size", float) = 1
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Transparent" }
		ZTest Off
		ZWrite Off
		Cull Off
		Blend SrcAlpha One

		Pass
		{
		CGPROGRAM

			#pragma vertex vertexShader
			#pragma fragment fragmentShader
			#pragma geometry geometryShader
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 5.0

			#include "UnityCG.cginc"
			#include "../Compute/DataTypes.cginc"

			StructuredBuffer<float2> positions;
			StructuredBuffer<ParticleProperties> properties;


			struct GS_INPUT
			{
				float4 pos : SV_POSITION;
				float explode : BLENDWEIGHT; //goes from 0 to 1 indicating explosion progress
				float fid : PS; //some float that does't change between frames for rendering
				float4 color : COLOR0;
			};

			struct FS_INPUT
			{
				float4 pos	: SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _Size;
			float4 _Color;

			float explodeLife;
			float explodeSize;
			float explodeRadius;
			int particleNumber;

			float badNoise(float n, float min, float max)
			{
				//returns noise between 0.5 and 2.5 (supposed to be psuedo-log)
				return sin(n * 4324.3435) * (max - min) + max;
			}

			// Vertex Shader ------------------------------------------------
			GS_INPUT vertexShader(uint id : SV_VertexID)
			{
				GS_INPUT vOut;

				// set output values
				vOut.pos = float4( positions[id], 0, properties[id].state != PARTICLE_STATE_DEAD);
				vOut.explode = (properties[id].state == PARTICLE_STATE_EXPLODING) * properties[id].life / explodeLife;
				vOut.color = properties[id].color;
				vOut.fid = ((float) id) / particleNumber;

				return vOut;
			}


			// Geometry Shader -----------------------------------------------------
			//the p[1] means 1 element
			[maxvertexcount(GS_VERT_NUMBER)]
			void geometryShader(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
			{
				//check for clip
				if (p[0].pos.w == 0)
					return;
				// create billboard
				float4 v[4];
				float2 uv[4];
				float3 up = float3(0, 1, 0);
				float3 right = float3(1, 0, 0);
				uint i;
				FS_INPUT fIn;
				float halfSize = _Size / 2;

				if (p[0].explode == 0) {

					v[0] = float4(p[0].pos + halfSize * right - halfSize * up, 1.0f);
					uv[0] = float2(1, 0);

					v[1] = float4(p[0].pos + halfSize * right + halfSize * up, 1.0f);
					uv[1] = float2(1, 1);

					v[2] = float4(p[0].pos - halfSize * right - halfSize * up, 1.0f);
					uv[2] = float2(0, 0);

					v[3] = float4(p[0].pos - halfSize * right + halfSize * up, 1.0f);
					uv[3] = float2(0, 1);

					//create vertices


					[unroll]
					for (i = 0; i < 4; i++) {
						fIn.pos = UnityObjectToClipPos(v[i]);
						fIn.uv = uv[i];
						fIn.color = p[0].color;
						triStream.Append(fIn);
					}
				}
				else {
					//process explosion
					uint N = EXPLODE_NUMBER;
					float iN = 1.0f / N;

					float theta = p[0].explode / 2;//give it a small rotation

					float offset_x, offset_y, offset_t;
					float3 noise;
					float PI = 3.141259265;
					float3 pos;

					[unroll]
					for (i = 0; i < N; i++) {
						triStream.RestartStrip();

						//rotate around a circle
						offset_t = theta + i * iN * 2.0 * PI;

						//get shift for radius due to nosie
						noise = float3(badNoise(p[0].fid * i),
							badNoise(p[0].fid * i + 1),
							badNoise(p[0].fid * i + 2));

						//we add hs/8 so that the starting radius is small but non-zero
						offset_x = (halfSize/4 + noise[0] * explodeRadius * p[0].explode) * cos(offset_t);
						offset_y = (halfSize/4 + noise[1] * explodeRadius * p[0].explode) * sin(offset_t);

						//change the position to be on the new circle
						pos = p[0].pos + float3(offset_x, offset_y, 0);

						//make the meshes follow circle, with a 5pi/4 offset so they point inwarsd
						up = float3(cos(offset_t + PI / 2 + 5 * PI / 4), sin(offset_t + PI / 2 + 5 * PI / 4), 0);
						right = float3(cos(offset_t + 5 * PI / 4), sin(offset_t + 5 * PI / 4), 0);

						//draw the mesh
						v[0] = float4(pos, 1.0f);
						v[1] = float4(pos + noise[2] * explodeSize * right, 1.0f);
						v[2] = float4(pos + noise[2] * explodeSize * up +  explodeSize * right, 1.0f);

						//load it
						[unroll]
						for (uint j = 0; j < 3; j++) {
							fIn.pos = UnityObjectToClipPos(v[j]);
							fIn.uv = float2(0.4, 0.4); //Take a point onthe texture as the explosion point.
							fIn.color = p[0].color;
							triStream.Append(fIn);
						}
					}
				}

			}


			fixed4 fragmentShader(FS_INPUT fIn) : SV_TARGET
			{
				// sample the texture color
				float4 texCol = tex2D(_MainTex, fIn.uv);

				//get our color choice
				float4 partCol = fIn.color;

				//blend them with alpha-blending
				return float4 (1.0f - (1.0f - texCol.rgb) * (1.0f - partCol.rgb), texCol.a * fIn.color.a);
			}
		ENDCG
	}
}
}
