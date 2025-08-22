Shader "Instanced/Particle2D" {
	Properties {
		_BlurRadius ("Radio de Difusión", Range(0.0, 5.0)) = 0.5
		_Softness ("Softness", Range(0.0, 1.0)) = 0.3
		_GlowIntensity ("Glow Intensity", Range(0.0, 2.0)) = 0.5
	}
	SubShader {

		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass {

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

			#include "UnityCG.cginc"
			
			StructuredBuffer<float2> Positions2D;
			StructuredBuffer<float2> Velocities;
			StructuredBuffer<float2> DensityData;
			StructuredBuffer<int> ParticleTypes;
			float scale;
			float4 colA;
			Texture2D<float4> ColourMap;
			SamplerState linear_clamp_sampler;
			float velocityMax;
			float _BlurRadius;
			float _Softness;
			float _GlowIntensity;
			float4 fluidColor;
			float4 airColor;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 colour : TEXCOORD1;
			};

			v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
			{
				int particleType = ParticleTypes[instanceID];
				float speed = length(Velocities[instanceID]);
				float speedT = saturate(speed / velocityMax);
				float colT = speedT;
				
				float3 centreWorld = float3(Positions2D[instanceID], 0);
				float3 worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * scale);
				float3 objectVertPos = mul(unity_WorldToObject, float4(worldVertPos.xyz, 1));

				v2f o;
				o.uv = v.texcoord;
				o.pos = UnityObjectToClipPos(objectVertPos);
				
				// Use specific colors based on particle type
				if (particleType == 0) // Fluido
				{
					o.colour = ColourMap.SampleLevel(linear_clamp_sampler, float2(colT, 0.5), 0);
				}
				else // Aire
				{
					o.colour = airColor.rgb;
				}

				return o;
			}


			float4 frag (v2f i) : SV_Target
			{
				float2 centreOffset = (i.uv.xy - 0.5) * 2;
				float sqrDst = dot(centreOffset, centreOffset);
				
				// Enhanced blur and softness calculation
				float blurRadius = _BlurRadius;
				float softness = _Softness;
				float glowIntensity = _GlowIntensity;
				
				// Calculate distance from center with blur radius
				float dist = sqrt(sqrDst);
				float normalizedDist = dist / (1.0 + blurRadius * 0.5);
				
				// Create soft falloff with multiple smoothstep layers
				float alpha = 1.0;
				
				// Core particle (sharp center)
				float coreAlpha = 1.0 - smoothstep(0.0, 0.3, normalizedDist);
				
				// Soft edge
				float softAlpha = 1.0 - smoothstep(0.3, 0.7 + softness, normalizedDist);
				
				// Glow effect
				float glowAlpha = 1.0 - smoothstep(0.7, 1.5 + glowIntensity, normalizedDist);
				glowAlpha *= 0.3; // Reduce glow intensity
				
				// Combine all layers
				alpha = max(coreAlpha, softAlpha * 0.8);
				alpha = max(alpha, glowAlpha);
				
				// Apply additional blur for very soft particles
				if (blurRadius > 1.0) {
					float extraBlur = smoothstep(0.0, 1.0, 1.0 - normalizedDist);
					alpha *= extraBlur;
				}
				
				// Apply extreme blur for very high values
				if (blurRadius > 3.0) {
					float extremeBlur = smoothstep(0.0, 0.5, 1.0 - normalizedDist);
					alpha *= extremeBlur;
				}

				float3 colour = i.colour;
				return float4(colour, alpha);
			}

			ENDCG
		}
	}
}