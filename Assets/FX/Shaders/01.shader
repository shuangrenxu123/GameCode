// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/NewUnlitShader"
{
	Properties{
		_Color("color",Color) = (1.0,1.0,1.0,1.0)
	}
		SubShader{
			Pass{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				fixed4 _Color;
			struct atov {
				float4 vert :POSITION;
				float3 normal:NORMAL;
				float4 texcoord:TEXCOORD0;
			};
			struct vtof {
				float4 pos :SV_POSITION;
				fixed3 color : COLOR0;

			};
			vtof vert(atov v){
				vtof o;
				o.pos = UnityObjectToClipPos(v.vert);
				o.color = v.normal * 0.5 + fixed3(0.5, 0.5, 0.5);
				return o;
			}
			fixed4 frag(vtof i) : SV_Target{
				fixed3 c = i.color;
			c = c * _Color;
				return fixed4(c,1.0);
			}
				ENDCG
			} 
	}
}