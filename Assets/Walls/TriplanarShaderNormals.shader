// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TriplanarShaderNormals" {
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
		};

		float3 triplanar(float3 worldPos, sampler2D map) {

			/*float3 scaledWorldPos = worldPos*_Scale;
			scaledWorldPos.y /= _YOffset;
			scaledWorldPos.y += _YOffset;

			float3 xProjection = tex2D(map, float2(scaledWorldPos.z, scaledWorldPos.y)) * saturate(round(worldPos.z / worldPos.y));
			float3 yProjection = tex2D(map, float2(scaledWorldPos.x, scaledWorldPos.z)) * saturate(round(worldPos.x / worldPos.z));
			float3 zProjection = tex2D(map, float2(scaledWorldPos.x, scaledWorldPos.y)) * saturate(round(worldPos.y / worldPos.x));

			return xProjection + yProjection + zProjection;*/
			
			float3 scaledWorldPos = worldPos;
			scaledWorldPos.xz += float2(0.5, 0.5);
			scaledWorldPos *= _Scale;

			scaledWorldPos.y /= _YOffset;
			scaledWorldPos.y += _YOffset;

			float3 xProjection = tex2D(map, float2(scaledWorldPos.z, scaledWorldPos.y)) * saturate((scaledWorldPos.x * 2) - 1);
			//float3 yProjection = tex2D(map, float2(scaledWorldPos.x, scaledWorldPos.z)) * saturate((scaledWorldPos.y * 2) - 1);
			float3 zProjection = tex2D(map, float2(scaledWorldPos.x, scaledWorldPos.y)) * saturate((scaledWorldPos.z * 2) - 1);

			return xProjection + /*yProjection*/ + zProjection;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float3 c = triplanar(IN.worldPos, _MainTex) * _Color;
			float3 normal = normalize(triplanar(IN.worldPos, _BumpMap));
			o.Normal = UnpackNormal(fixed4(normal.xyz, 1));
			o.Albedo = c.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
