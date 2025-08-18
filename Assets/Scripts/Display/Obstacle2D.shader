Shader "Custom/Obstacle2D" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Scale ("Scale", Float) = 20.0
		_ScaleY ("Scale Y", Float) = 1.0
		_OffsetX ("Offset X", Float) = 0.0
		_OffsetY ("Offset Y", Float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

			#include "UnityCG.cginc"
			
			float4 _Color;
			float2 _ObstacleSize;
			float2 _ObstacleCentre;
			float _Scale;
			float _ScaleY;
			float _OffsetX;
			float _OffsetY;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag (v2f i) : SV_Target {
				float2 uv = i.uv;
				
				// Convert UV to world position
				float2 worldPos = (uv - 0.5) * _Scale; // Use adjustable scale
				
				// Apply offset to obstacle center
				float2 adjustedCentre = _ObstacleCentre + float2(_OffsetX, _OffsetY);
				
				// Apply Y scaling to obstacle size
				float2 scaledSize = _ObstacleSize;
				scaledSize.y *= _ScaleY;
				
				// Check if we're inside the obstacle area
				float2 halfSize = scaledSize * 0.5;
				float2 edgeDist = halfSize - abs(worldPos - adjustedCentre);
				
				if (edgeDist.x >= 0 && edgeDist.y >= 0) {
					// Inside obstacle - draw white
					return float4(1, 1, 1, 1); // White
				} else {
					// Outside obstacle - transparent
					return float4(0, 0, 0, 0);
				}
			}
			ENDCG
		}
	}
}
