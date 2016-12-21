Shader "Game/Character/Ghost"
{
	Properties {
		_Color ("Color", Color) = ( 1, 1, 1, 1 )
		_Thickness("Silluette Thickness", Range(0.0, 1)) = .1
		_ShiftAlpha("Silluette Shift Alpha", Range(0.0, 1)) = .1
	}

	SubShader {

		Pass {
			Tags {
				"LightMode" = "ForwardBase"
				"RenderType" = "Opaque"
			}
			
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				fixed4 diff : COLOR0; // diffuse lighting color
				float4 vertex : SV_POSITION;
			};

			uniform half4 _Color;
			uniform half _Thickness;
			uniform half _ShiftAlpha;
			
			v2f vert (appdata_base v) {
				v2f o;
				float3 posNorm = normalize(float3(v.vertex.x, v.vertex.y, v.vertex.z));
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex + float4(posNorm.x * _Thickness, posNorm.y * _Thickness, posNorm.z * _Thickness, 0));

				half3 viewNormal = mul(UNITY_MATRIX_MV, float4(v.normal, 0));
				half alpha = 1- abs(dot(viewNormal, half3(0, 0, 1)));
				o.diff.r = o.diff.g = o.diff.b = 1;
				o.diff.a = clamp(alpha * alpha + _ShiftAlpha, 0, 1);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = _Color;
				col.a = i.diff.a;
				return col;
			}
			ENDCG
		}
	}
}
