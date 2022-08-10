Shader "Unlit/CircleWarp"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SeaLevel("Sea Level", Float) = .2
		_Rotation("Rotation", Float) = 0
		_DistanceOffset("DistanceOffset", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{ 
			//Cull Off ZWrite Off ZTest Always

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
			float _Rotation;
			float _SeaLevel;
			float4 _MainTex_ST;
			float _DistanceOffset;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				float aspect = unity_OrthoParams.x / unity_OrthoParams.y;
				// sample the texture
				float3 fromCenter = float3(i.uv.x - .5, i.uv.y - .5, 0);

				//fromCenter.x *= aspect;

				float newX = fromCenter.x * sin(_Rotation) - fromCenter.y * cos(_Rotation);
				float newY = fromCenter.x * cos(_Rotation) + fromCenter.y * sin(_Rotation);

				fromCenter.x = newX;
				fromCenter.y = newY;

				float3 normalized = normalize(fromCenter);
				float angle = acos(dot(normalized.xy, float2(0, 1)));
				float3 check = cross(normalized, float3(0, 1, 0));
				angle = angle * (check.z >= 0) + (2 * 3.14159 - angle) * (check.z < 0);
				float d = pow(fromCenter.x * fromCenter.x + fromCenter.y * fromCenter.y, .5) / .5f;// pow(.5, .5);

				if (d > 1) discard;
				if (d < .1)
				{
					return fixed4(32/255.0, 33/255.0, 38/255.0, 1);
				}
				else
				{
					d = (d - .1) / .9;
					//d = log(1 + (d / (_SeaLevel * (1 + d*d*1)))) / log(1 + 1 / (_SeaLevel * (1 + d*d*1)));
					d = log(1 + d / _SeaLevel) / log(1 + 1 / _SeaLevel);
				}

				float x = 1 - (angle / (2 * 3.14159));
				float y = 1 - d + _DistanceOffset;

				if (y < 0 || y > 1) return fixed4(0, 0, 0, 0);

				float2 idk = float2(x, y);

				fixed4 col = tex2D(_MainTex, idk);
				col = lerp(fixed4(32 / 255.0, 33 / 255.0, 38 / 255.0, 0), col, min(1, pow(d*9, 2)));
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
