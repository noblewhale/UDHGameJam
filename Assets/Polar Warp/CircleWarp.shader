Shader "Unlit/CircleWarp"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SeaLevel ("Sea Level", Float) = .2
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{ 
			Cull Off ZWrite Off ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float _SeaLevel;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float3 fromCenter = float3(i.uv.x - .5, i.uv.y - .5, 0);

				float3 normalized = normalize(fromCenter);
				float angle = acos(dot(normalized.xy, float2(0, 1)));
				float3 check = cross(normalized, float3(0, 1, 0));
				angle = angle * (check.z >= 0) + (2 * 3.14159 - angle) * (check.z < 0);
				float d = pow(fromCenter.x*fromCenter.x + fromCenter.y*fromCenter.y, .5) / pow(.5, .5);
				d = _SeaLevel * log(d / _SeaLevel) / (_SeaLevel * log(1 / _SeaLevel));
				
				float2 idk = float2(1 - (angle / (2 * 3.14159)), 1 - d);

				fixed4 col = tex2D(_MainTex, idk);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
