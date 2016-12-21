Shader "Game/Character/Lit"
{
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_AddItensity("Add Light Intensity", float) = 0.2
		_MinIntensity("Min Light Intensity", float) = 0.5
	}

	SubShader {
		Tags { "RenderType" = "Opaque" }

		Stencil {
			Ref 2
			Comp always
			Pass replace
		}

		CGPROGRAM
		#pragma surface surf SimpleLambert

		half _AddItensity;
		half _MinIntensity;

		half4 LightingSimpleLambert(SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot(s.Normal, lightDir);
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * clamp((NdotL * atten) + _AddItensity, _MinIntensity, 1);
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
