// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TriplanarShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bump Map", 2D) = "bump" {}
		_Scale("Scale", Float) = 1
		_YScale("Y Scale", Float) = 1
		_YOffset("Y offest", Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		// And generate the shadow pass with instancing support
		#pragma surface surf Standard fullforwardshadows addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		// Enable instancing for this shader
		#pragma multi_compile_instancing

		// Config maxcount. See manual page.
		// #pragma instancing_options

		sampler2D _MainTex;
		sampler2D _BumpMap;
		fixed4 _Color;
		float _Scale;
		float _YScale;
		float _YOffset;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		float3 triplanar(float3 worldPos, float3 blendAxes, sampler2D map) {

			float3 scaledWorldPos = worldPos * _Scale;

			float3 xProjection = tex2D(map, float2(scaledWorldPos.z, scaledWorldPos.y / _YScale + _YOffset)) * blendAxes.x;
			float3 yProjection = tex2D(map, float2(scaledWorldPos.x, scaledWorldPos.z)) * blendAxes.y;
			float3 zProjection = tex2D(map, float2(scaledWorldPos.x, scaledWorldPos.y / _YScale + _YOffset)) * blendAxes.z;

			return xProjection + yProjection + zProjection;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 blendAxes = abs(IN.worldNormal);
			blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;

			// Albedo comes from a texture tinted by color
			float3 c = triplanar(IN.worldPos, blendAxes, _MainTex) * _Color;
			float3 normal = normalize(triplanar(IN.worldPos, blendAxes, _BumpMap));
			//o.Normal = UnpackNormal(fixed4(normal.xyz, 1)); //Lo intenté pero no parece que se pueda :/
			o.Albedo = c.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
