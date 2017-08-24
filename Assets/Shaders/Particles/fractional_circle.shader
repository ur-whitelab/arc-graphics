// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Custom/WedgeCircle" {
     Properties {
         _Color ("Color", Color) = (1,0,0,0)
		 //_NumWedges ("Number of Sections", Int) = 3
		 _Angle1 ("First Angle in tau", Range (0.0, 1.0)) = 0.0
		 _Angle2 ("Second Angle in tau", Range (0.0, 1.0)) = 0.0
		 _Angle3 ("Third Angle in tau", Range (0.0, 1.0)) = 0.0
		 //_Angle2 ("Second Angle in tau", Float) = (1/3)
		 //_Angle3 ("Third Angle in tau", Float) = 2/3
     }
     SubShader {
		 Blend SrcAlpha OneMinusSrcAlpha

         Pass {
             CGPROGRAM
 
             #pragma vertex vert
             #pragma fragment frag
             #include "UnityCG.cginc"
 
             fixed4 _Color; // low precision type is usually enough for colors
			 float _Angle1;
			 float _Angle2;
			 float _Angle3;	

             struct fragmentInput {
                 float4 pos : SV_POSITION;
                 float2 uv : TEXTCOORD0;
             };
 
             fragmentInput vert (appdata_base v)
             {
                 fragmentInput o;
 
                 o.pos = UnityObjectToClipPos (v.vertex);
                 o.uv = v.texcoord.xy - fixed2(0.5,0.5);
 
                 return o;
             }
 
             fixed4 frag(fragmentInput i) : SV_Target {
                 float distance = sqrt(pow(i.uv.x, 2) + pow(i.uv.y,2));
				 float pi = 3.141593;//close enough
				 float angle = (atan2(i.uv.y, i.uv.x) + pi);//this atan returns from -pi to pi
				 float tau = 2*pi;
                 //float distancez = sqrt(distance * distance + i.l.z * i.l.z);
                 if(distance > 0.5f || distance < 0.25f){
                     return fixed4(0,0,0,0);
                 }
                 else{
					 /*if(i.uv.x > 0.0f && i.uv.y > 0.0f){
						return fixed4(0,0,1,1);
					 }
					 else if(i.uv.y > 0.0f){
					 	 return fixed4(1,0,0,1);
					 }
					 else if(i.uv.x > 0.0f){
					 	 return fixed4(.5,.5,.5,1);
					 }
					 else{
						return fixed4(0,1,0,1);
					 }*/
					 if(angle < _Angle1*tau){
					 	 return fixed4(1,0,1,1);
					 }
					 else if(angle < _Angle2*tau){
					 	 return fixed4(1,1,0,1);
					 }
					 else{
					 	 return fixed4(0,1,1,1);
					 }
                     
                 }
             }
             ENDCG
         }
     }
 }