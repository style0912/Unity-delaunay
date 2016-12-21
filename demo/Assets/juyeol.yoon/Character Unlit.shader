Shader "Game/Character/Unlit"
{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader{
		Tags{ "RenderType" = "Opaque" }
		
		Stencil {
			Ref 2
			Comp always
			Pass replace
		}

		CGPROGRAM
		#pragma surface surf SimpleUnlit

		half4 LightingSimpleUnlit(SurfaceOutput s, half3 lightDir, half atten) {
			half4 c;
			c.rgb = s.Albedo;
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
