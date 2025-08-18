Shader "Custom/Obstacle2D" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ScaleY ("Scale Y", Float) = 1.0
		_QuadSize ("Quad Size", Vector) = (1,1,0,0)
		_ScaleFactor ("Scale Factor", Vector) = (1,1,0,0)
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
			float _ScaleY;
			float2 _QuadSize;
			float2 _ScaleFactor;
			
			// New obstacle system
			StructuredBuffer<float4> _Obstacles;
			int _NumObstacles;
			int _MaxObstacles;

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

			bool IsInsideObstacle(float2 worldPos, float2 obstaclePos, float2 obstacleSize)
			{
				float2 halfSize = obstacleSize * 0.5;
				float2 edgeDist = halfSize - abs(worldPos - obstaclePos);
				return edgeDist.x >= 0 && edgeDist.y >= 0;
			}
			


			float4 frag (v2f i) : SV_Target {
				float2 uv = i.uv;
				
				// Convert UV to world position using actual quad size
				float2 worldPos = (uv - 0.5) * _QuadSize;
				
				// Check if we're inside any obstacle
				bool insideObstacle = false;
				
				// Check new obstacle system first
				if (_NumObstacles > 0)
				{
					for (int j = 0; j < _NumObstacles; j++)
					{
						float4 obstacle = _Obstacles[j];
						float2 obstaclePos = obstacle.xy;
						float2 obstacleSize = obstacle.zw;
						
						// Apply scale factor to obstacle position and size
						obstaclePos *= _ScaleFactor;
						obstacleSize *= _ScaleFactor;
						
						// Apply Y scaling to obstacle size
						obstacleSize.y *= _ScaleY;
						
						if (IsInsideObstacle(worldPos, obstaclePos, obstacleSize))
						{
							insideObstacle = true;
							break;
						}
					}
				}
				else
				{
					// Fallback to legacy obstacle system
					float2 scaledSize = _ObstacleSize * _ScaleFactor;
					scaledSize.y *= _ScaleY;
					
					float2 scaledCentre = _ObstacleCentre * _ScaleFactor;
					insideObstacle = IsInsideObstacle(worldPos, scaledCentre, scaledSize);
				}
				
				if (insideObstacle) {
					return _Color; // Inside obstacle - draw with configured color
				} else {
					return float4(0, 0, 0, 0); // Outside obstacle - transparent
				}
			}
			ENDCG
		}
	}
}
