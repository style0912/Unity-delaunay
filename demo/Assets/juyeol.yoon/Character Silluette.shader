Shader "Game/Character/Silluette"
{
	Properties {
		_Color("Silluette Color", Color) = (0,0,0,1)
		_Thickness("Silluette Thickness", Range(0.0, 1)) = .1
		_MinAlpha("Silluette Min Alpha", Range(0.0, 1)) = .1
	}

	SubShader {
		Tags { 
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Stencil {
			Ref 2
			Comp notequal
			Pass keep
		}

		Blend SrcAlpha OneMinusSrcAlpha
		ZTest Always
		ZWrite Off
		Cull Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				fixed4 col : COLOR0; // diffuse lighting color
				float4 pos : POSITION;
			};

			uniform half _Thickness;
			uniform half _MinAlpha;
			uniform half4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				float3 offset = normalize(v.vertex.xyz) * _Thickness;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex + float4(offset.xyz, 0));

				half3 viewNorm = mul(UNITY_MATRIX_MV, float4(v.normal, 0));
				half alpha = 1 - abs(dot(viewNorm, half3(0, 0, 1)));
				o.col.rgb = 1;
				o.col.a = clamp(alpha + _MinAlpha, 0, 1);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = _Color;
				col.a = i.col.a;
				return col;
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}
