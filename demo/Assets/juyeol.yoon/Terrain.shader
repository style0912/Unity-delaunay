Shader "Game/Terrain" {

	Properties {
		[NoScaleOffset] _MainTex("Tile Texture", 2D) = "white" {}
		_MulItensity("Multipier Light Intensity", float) = 1
	}

	SubShader {
		Tags{ "RenderType" = "Opaque" }

		Stencil {
			Ref 1
			Comp always
			Pass replace
		}

		CGPROGRAM
		#pragma surface surf SimpleLambert

		half _MulItensity;

		half4 LightingSimpleLambert(SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot(s.Normal, lightDir);
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * NdotL * atten * _MulItensity;
			c.a = s.Alpha;
			return c;
		}

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
		}
		ENDCG
	}

	FallBack "Diffuse"
}
