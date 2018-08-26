Shader "GrassNStuff/GNS_Shader" {
	Properties {
		_MainColor ("Main color",color) = (0,0,0,0)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Normal ("Normal Map", 2D) = "bump" {}
		_SpecGloss ("Spec Gloss Map", 2D) = "white" {}
		_Cutoff("Cut Alpha",Range(0,.9)) = .5
		_GlossStrength("Gloss Strength",Range(0,10)) = 1
		_SpecularStrength("Specular Strength",Range(0,10)) = 1
		_OcclusionHeight ("Occlusion Height",Range(0,10)) = 1
		_OcclusionStrength ("Occlusion Strength",Range(0,10)) = 1
		_OcclusionToColor ("Occlusion To Color",Range(0,1)) = 0.5
		_HeightMapResolution("HeightMap Resolution",int) = 0

		// Buffers
		_PositionBuffer ("Position Buffer",2D)= "white" {}
		_RotationBuffer ("Rotation Buffer",2D)= "white" {}
		_ScaleBuffer ("Scale Buffer",2D)= "white" {}
		_NoiseBuffer ("Noise Buffer",2D)= "white" {}
		[PerRendererData]_PaintBuffer ("Paint Buffer",2D)= "white" {}
		_WindBuffer ("Wind Buffer",2D) = "white" {}
		_HeightBuffer("Height Map", 2D) = "white" {}

		// Data
		_FadeDistance ("Fade Distance",Float) = 50
		_FadeLength("Fade Length",Float) = 400
		_WindSpeed("Wind Speed",Range(0,20)) = 1
		_WindStrength("Wind Strength",Range(0,20)) = 1
		_BoundSize("Bound Size",Vector) = (0,0,0,0)
		_BoundPosition("Bound Position",Vector) = (0,0,0,0)
		_PatchSize("Patch Position",Vector) = (0,0,0,0)
		[PerRendererData]_CrossFade("Cross Fade",float) = 0
	}
	SubShader {
		Tags {"Queue"="Geometry+200" "RenderType"="Grass" "IgnoreProjector"="True"}
		LOD 200
		Cull Off
		
		CGPROGRAM
		#pragma surface surf Custom vertex:vert addshadow alphatest:_Cutoff
		#pragma target 3.0
		#pragma multi_compile_instancing
		#include "UnityPBSLighting.cginc"


		// base values
		fixed4 _MainColor;
		sampler2D _MainTex;
		sampler2D _Normal;
		sampler2D _SpecGloss;
		float _GlossStrength;
		float _SpecularStrength;
		float _OcclusionHeight;
		float _OcclusionStrength;
		float _OcclusionToColor;
		float _CrossFade;
		int _HeightMapResolution;

		// Buffers
		sampler2D _HeightBuffer;
		sampler2D _PositionBuffer;
		sampler2D _RotationBuffer;
		sampler2D _ScaleBuffer;
		sampler2D _NoiseBuffer;
		sampler2D _WindBuffer;
		sampler2D _PaintBuffer;

		// Data
		fixed4 _BoundSize;
		fixed4 _PatchSize;
		fixed4 _BoundPosition;
		float _WindSpeed;
		float _WindStrength;
		float4 _WindBuffer_ST;
		float _FadeLength;
		float _FadeDistance;

		struct Input {
			float2 uv_MainTex;
			float ambientOcclusion;
			float fadeOut;
			fixed shouldRender;
		};

		float _Glossiness;
		float _Metallic;

		// voids
		fixed4 CalculateRelativePosition(fixed4 WorldPosition){
			fixed4 RelativePosition = WorldPosition - _BoundPosition; // calculate relative position
			return fixed4((RelativePosition.x / _BoundSize.x),(RelativePosition.z / _BoundSize.z),0,0);
		}
		fixed4 CalculateRelativePatchPosition(fixed4 ObjectPosition){
			return fixed4((ObjectPosition.x / _PatchSize.x),(ObjectPosition.z / _PatchSize.y),0,0);
		}
		float4 qmul(float4 q1, float4 q2)
        {
            return float4(
                q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
                q1.w * q2.w - dot(q1.xyz, q2.xyz)
            );
        }
        float3 RotateVector(float3 v, float4 r)
        {
            float4 r_c = r * float4(-1, -1, -1, 1);
            return qmul(r, qmul(float4(v, 0), r_c)).xyz;
        }
        fixed denormalize(fixed number){
			return (number / 0.5) - 1;
		}
        fixed4 VertexWind(fixed4 vert){
       	 	vert.x = denormalize(vert.x);
       		vert.z = denormalize(vert.z);
        	return fixed4(
	        	sin(_Time.x * _WindSpeed + vert.x) + sin(_Time.x * _WindSpeed + vert.z * 2) + sin(_Time.x * _WindSpeed * 0.1), // x
	        	0,
				cos(_Time.x * _WindSpeed + vert.x * 2) + cos(_Time.x * _WindSpeed + vert.z), // z
				0
			) * _WindStrength;
        }
        fixed ToPixelThing(float Value, int width){
        	return (Value / width) + ((1 / width) * 0.5);
        }

        fixed PositionHeight(fixed4 Position){
        	fixed4 RelativePosition = CalculateRelativePosition(Position);
        	fixed4 TexturePosition = fixed4((RelativePosition.x * _HeightMapResolution),0,(RelativePosition.y * _HeightMapResolution),0);

        	// floor floor
        	fixed hx0z0 = tex2Dlod(_HeightBuffer,fixed4(ToPixelThing(floor(TexturePosition.x),_HeightMapResolution),ToPixelThing(floor(TexturePosition.z),_HeightMapResolution),0,0));

        	fixed hx1z0 = tex2Dlod(_HeightBuffer,fixed4(ToPixelThing(ceil(TexturePosition.x),_HeightMapResolution),ToPixelThing(floor(TexturePosition.z),_HeightMapResolution),0,0));

        	fixed hx0z1 = tex2Dlod(_HeightBuffer,fixed4(ToPixelThing(floor(TexturePosition.x),_HeightMapResolution),ToPixelThing(ceil(TexturePosition.z),_HeightMapResolution),0,0));
   
        	fixed hx1z1 = tex2Dlod(_HeightBuffer,fixed4(ToPixelThing(ceil(TexturePosition.x),_HeightMapResolution),ToPixelThing(ceil(TexturePosition.z),_HeightMapResolution),0,0));

        	// Interpolation
        	return hx0z0 + (hx1z0 - hx0z0) * (TexturePosition.x - (int)TexturePosition.x) + (hx0z1 - hx0z0) * (TexturePosition.z - (int)TexturePosition.z) + (hx0z0 - hx1z0 - hx0z1 + hx1z1) * (TexturePosition.x - (int)TexturePosition.x) * (TexturePosition.z - (int)TexturePosition.z);
        }

       inline half4 LightingCustom_Deferred (SurfaceOutputStandard s, half3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
		{
			half oneMinusReflectivity;
			half3 specColor;
			s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

			half4 c = UNITY_BRDF_PBS (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
			c.rgb += UNITY_BRDF_GI (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);

			outDiffuseOcclusion = half4(s.Albedo, s.Occlusion) * s.Occlusion;
			outSpecSmoothness = half4(specColor, s.Smoothness) * s.Occlusion;
			outNormal = half4(s.Normal * 0.5 + 0.5, 1);
			half4 emission = half4(s.Emission + c.rgb, 1);
			return emission * s.Occlusion;
		}

		inline void LightingCustom_GI (
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			gi = UnityGlobalIllumination (data, s.Occlusion, s.Smoothness, s.Normal);
		}
        // vertex function
		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input,o); // initialize input

			fixed4 uv2 = v.texcoord1;
			fixed4 VertexBeforePosition = v.vertex; // initial vertex position
			fixed4 WorldPosition = mul(unity_ObjectToWorld,v.vertex) - VertexBeforePosition; // calc worldpos
			fixed4 RelativePosition = CalculateRelativePosition(WorldPosition);

			// Get Buffers
			fixed4 RelativeUV2 = uv2 + RelativePosition;
			fixed4 BufferOffset = tex2Dlod(_PositionBuffer,RelativeUV2);
			fixed4 RotationOffset = tex2Dlod(_RotationBuffer,RelativeUV2);
			fixed4 ScaleOffset = tex2Dlod(_ScaleBuffer,RelativeUV2);
			
			// Final
			fixed4 RotatedVertex = fixed4(RotateVector(v.vertex.xyz, RotationOffset).xyz * ScaleOffset.xyz,0);
			fixed4 PositionOffset = fixed4(
				BufferOffset.r * _PatchSize.r,
				0,
				BufferOffset.g * _PatchSize.g,
				0
			);

			// set values
			fixed4 WorldPositionOffsetted = WorldPosition + PositionOffset; // calc worldpos
			fixed4 RelativeOffsettedPosition = CalculateRelativePosition(WorldPositionOffsetted);

			fixed Height = PositionHeight(WorldPositionOffsetted) * _BoundSize.y;

			fixed NoiceScaleOffset = tex2Dlod(_NoiseBuffer,RelativeOffsettedPosition).r;

			fixed viewLength = max(0,length(_WorldSpaceCameraPos - WorldPositionOffsetted) - _FadeLength) / _FadeDistance;
			o.fadeOut = viewLength;

			// set final
			v.normal = lerp(RotateVector(v.normal, RotationOffset),fixed4(0,1,0,0),v.texcoord2.g);
			v.vertex = RotatedVertex * NoiceScaleOffset;
			o.shouldRender = tex2Dlod(_PaintBuffer,fixed4(BufferOffset.x - 0.5,BufferOffset.y - 0.5,0,0)).r;

			fixed occlusion = saturate(v.vertex.y / _OcclusionHeight);
			o.ambientOcclusion = pow(occlusion, _OcclusionStrength);

			v.vertex += PositionOffset; // offset

			fixed4 World = mul(unity_ObjectToWorld,v.vertex);

			World.y = ((VertexBeforePosition.y  * NoiceScaleOffset * ScaleOffset.y) - WorldPosition.y) + (Height + _BoundPosition.y);
			v.vertex = mul(unity_WorldToObject,World);

			fixed4 uv = fixed4(
				v.texcoord.x,
				0,
				v.texcoord.y,
				0
			);

			fixed WindPerlin = tex2Dlod(_WindBuffer,RelativeOffsettedPosition).r;
			v.vertex.x += sin(RelativeOffsettedPosition.x + WindPerlin + (_Time.y * _WindSpeed)) * VertexBeforePosition.y * ScaleOffset.x * NoiceScaleOffset * _WindStrength * v.texcoord2.x;
			v.vertex.z += sin(RelativeOffsettedPosition.z + WindPerlin + (_Time.y * _WindSpeed)) * VertexBeforePosition.y * ScaleOffset.z * NoiceScaleOffset * _WindStrength * v.texcoord2.x;

        	#if _Normal
            v.tangent.xyz = RotateVector(v.tangent.xyz,RotationOffset);
           	#endif
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 SpecCloss = tex2D(_SpecGloss,IN.uv_MainTex);

			o.Occlusion = lerp(IN.ambientOcclusion,1,1-_OcclusionToColor);
			o.Albedo = c.rgb * _MainColor;
			o.Alpha = ((c.a - IN.fadeOut) * IN.shouldRender) * _CrossFade;
			o.Smoothness = SpecCloss.g * _GlossStrength;
			o.Normal = UnpackNormal (tex2D(_Normal,IN.uv_MainTex));
			o.Metallic = SpecCloss.r * _SpecularStrength;
		}
		ENDCG
	}
	SubShader {
		Tags {
			"Queue" = "Geometry"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
		}
		Cull Off
		LOD 200
		ColorMask RGB
		
		Pass {
			Material {
				Diffuse (1,1,1,1)
				Ambient (1,1,1,1)
			}
			Lighting On
			ColorMaterial AmbientAndDiffuse
			AlphaTest Greater [_Cutoff]
			SetTexture [_MainTex] { combine texture * primary DOUBLE, texture }
		}
	}
	
	Fallback Off
}
