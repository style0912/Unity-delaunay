Shader "Game/Character/Unlit-Color"
{
	Properties{
		_Color("Color", Color) = (0,0,0,1)
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

		half4 _Color;

		half4 LightingSimpleUnlit(SurfaceOutput s, half3 lightDir, half atten) {
			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = _Color.rgb;
		}
		ENDCG
	}

	FallBack "Diffuse"
}
