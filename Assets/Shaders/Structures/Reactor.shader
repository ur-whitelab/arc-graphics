﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Custom/WedgeCircle" {
	 Properties{
		 _NumWedges("Number of Sections", Int) = 5
		 //have to do these out manually due to shader restrictions
		 _Fraction1("Mole fraction species 1", Float) = 0.0
		 _Fraction2("Mole fraction species 2", Float) = 0.0
		 _Fraction3("Mole fraction species 3", Float) = 0.0
		 _Fraction4("Mole fraction species 4", Float) = 0.0
		 _Fraction5("Mole fraction species 5", Float) = 1.0
		 _Fraction6("Mole fraction species 6", Float) = 0.0
     }
     SubShader {
		 Tags{ "RenderType" = "Opaque" }
		 LOD 100
		 Blend SrcAlpha OneMinusSrcAlpha

         Pass {
             CGPROGRAM

             #pragma vertex vert
             #pragma fragment frag
             #include "UnityCG.cginc"

			 int _NumWedges;//number of wedges we're shading
			 float _Fraction1;
			 float _Fraction2;
			 float _Fraction3;
			 float _Fraction4;
			 float _Fraction5;
			 float _Fraction6;


             struct fragmentInput {
                 float4 pos : SV_POSITION;
                 float2 uv : TEXCOORD0;
             };

             fragmentInput vert (appdata_base v)
             {
                 fragmentInput o;

                 o.pos = UnityObjectToClipPos (v.vertex);
                 o.uv = v.texcoord.xy - fixed2(0.5,0.5);//centering origin

                 return o;
             }

             fixed4 frag(fragmentInput i) : SV_Target {
                 float distance = sqrt(pow(i.uv.x, 2) + pow(i.uv.y,2));
				 float pi = 3.1415927;//close enough
				 float angle = (atan2(i.uv.y, i.uv.x) + pi);//this atan returns from -pi to pi
				 float tau = 2*pi;//easier to do mole fractions in tau
				 float4 colors[7];
				 float fractions[6];//the mole fractions
				 float angles[6];//the actual angle cutoffs
				 int num_wedges = _NumWedges;

				 //color map pulled from colorbrewer2.org

				 colors[0] = float4(152.0f / 255.0f, 78.0f / 255.0f, 163.0f / 255.0f, 0.6); //purple
				 colors[1] = float4(228.0f / 255.0f, 26.0f / 255.0f, 28.0f / 255.0f, 0.6); //red
				 colors[2] = float4(77.0f / 255.0f, 175.0f / 255.0f, 74.0f / 255.0f, 0.6); //green
				 colors[3] = float4(55.0f / 255.0f, 126.0f / 255.0f, 184.0f / 255.0f, 0.6); //blue
				 colors[4] = float4(80.0f / 255.0f, 80.0f / 255.0f, 80.0f / 255.0f, 0.6); //gray for when we have no fractions
				 colors[5] = float4(255.0f / 255.0f, 255.0f / 255.0f, 51.0f / 255.0f, 0.6);
				 colors[6] = float4(166.0f / 255.0f, 86.0f / 255.0f, 40.0f / 255.0f, 0.6);

				 fractions[0] = _Fraction1;
				 fractions[1] = _Fraction2;
				 fractions[2] = _Fraction3;
				 fractions[3] = _Fraction4;
				 fractions[4] = _Fraction5;
				 fractions[5] = _Fraction6;
				 int j = 0;
				 float tot_frac = 0.0f;
				 //Need to track the CUMULATIVE starting angle for each fraction.
				 for (j = 0; j < num_wedges; j++) {
					 tot_frac += fractions[j];
					 fractions[j] = tot_frac;
				 }

				 for(j = 0; j < num_wedges; j++){
					 angles[j] = tau*fractions[j]/tot_frac;
				 }


				 if (distance < 0.2f) {
					 return fixed4(0, 0, 0, 0);
				 }

                 else if (distance > 0.35f && distance < 0.5f){

				 for (j = 0; j < num_wedges; j++) {
					 if (angle < angles[j]) {
						 return colors[j];
					 }
				 }

					 return fixed4(0, 0, 0, 0);
                 }
				 else {
					 return fixed4(0, 0, 0, 0);
				 }
             }
             ENDCG
         }
     }
 }