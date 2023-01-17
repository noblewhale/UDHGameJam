Shader "Custom/CircleWarpNew"
{
  SubShader
  {
    Cull Off ZWrite Off ZTest Always
    Pass
    {
      CGPROGRAM
      #pragma vertex VertMe
      #pragma fragment FragMe
      #include "UnityCG.cginc"

      sampler2D _MainTex;
	float4 _MainTex_TexelSize;
			// A magic number for adjusting the appearance of the warp
			float _SeaLevel;
			// The radius of the inner circle
			float _InnerRadius;
			// The size of the fade effect on the inner circle
			float _InnerFadeSize;
			// How sharply the fade falls off
			float _InnerFadeExp;
			// The color to use for the inner circle
			float4 _InnerColor;
			// The thickness to use for the outline
			float4 _OutlineThickness;
			// The color of the outline
			float4 _OutlineColor;

			float2 _CameraPosition;
			float2 _CameraDimensions;
			float _CameraOffset;
			float2 _MapPosition;
			float2 _MapDimensions;

			// Angles are calculated from this vector
			// At large scales there are rendering errors near this vector so we don't use DOWN where the player is rendered.
			static const float3 RIGHT = float3(1, 0, 0);
			// It's a PI
			static const float PI = 3.1415926535897932384626433832795028841971693993751058209749445923078164062;

      struct v2f
      {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      // Just your standard vertex shader
      v2f VertMe(appdata_base v)
      {
        v2f o;
        o.vertex = v.vertex;
        o.uv = (v.vertex.xy + 1.0) * 0.5;
        #if UNITY_UV_STARTS_AT_TOP
          o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
        #endif
        return o;
      }

			// Get the angle between the right vector and the input
			float CalculateVectorAngle(float2 input, float distance)
			{
				// Normalize so we can get the angle
				input /= distance;

				float dotProduct = dot(RIGHT.xy, input);
				// The dot product can sometimes become larger than 1 when the input vector is very close to the right vector.
				// This would cause the angle to be NAN, leading to some rendering artifacts, so let's not let that happen.
				// Also apparently there's some problem at -1 that creates a seam but adding this small arbitrary amount fixes it...
				dotProduct = clamp(dotProduct, -1 + .0000001, 1);

				// Yay the angle (almost)
				float angle = acos(dotProduct);

				// Depending on the z component of the cross product we may need to invert the angle
				float3 check = cross(RIGHT, float3(input.x, input.y, 0));
				// Invert if necessary. Otherwise angle would cycle twice between 0 and PI and only half the texture would get sampled and mirrored
				angle = angle * (check.z >= 0) + (2 * PI - angle) * (check.z < 0);

				return angle;
			}

			// Convert distance to unwarped y value.
			float UnWarpY(float distance)
			{
				// Renormalize d taking into account the inner radius. Now d goes from 0 at the edge of the inner radius to 1 at edge of the texture coordinates
				float y = (distance - _InnerRadius) / (1 - _InnerRadius);

				// Y is inverted because low distance from center should correspond to high y values
				// The play area is in the bottom half of the circle and up should be up.
				return 1 - y;
			}

			// Convert angle to unwarped x value
			float UnWarpX(float angle)
			{
				// How far around the circle
				float x = angle / (2 * PI);
				// Wrap between 0 and 1 because rotation may have made angle negative or greater than 2*PI
				x -= floor(x);
				return x;
			}

			// Use distance and angle to calculate the unwarped positon
			float2 UnWarp(float distance, float angle)
			{
				return float2(UnWarpX(angle), UnWarpY(distance));
			}

			fixed4 FragMe(v2f i) : SV_Target
			{
				_MapDimensions *= 4;
				float2 cameraBottomLeft = _CameraPosition - _CameraDimensions / 2;
				float2 mapBottomLeft = _MapPosition - _MapDimensions / 2;
				float2 normalizedCameraBottomLeft = (cameraBottomLeft - mapBottomLeft) / _MapDimensions;
				float2 normalizedCameraDimensions = _CameraDimensions / _MapDimensions;

				// Transform texture coordinates to be relative to the center. The values go from -1 to 1
				i.uv = i.uv * 2 - 1;

				i.uv *= (normalizedCameraDimensions.y - (-_CameraOffset * 2) / _MapDimensions.y) / 2;
				i.uv.y -= 1;
				i.uv.y += normalizedCameraBottomLeft.y + normalizedCameraDimensions.y / 2;

				i.uv.x *= (_CameraDimensions.x / _CameraDimensions.y);

				i.uv.y -= _CameraOffset / _MapDimensions.y;

				// The distance from center to the texture coord
				float distance = sqrt(i.uv.x * i.uv.x + i.uv.y * i.uv.y);

				// Discard corner pixels, we are rendering a circle, circles don't have corners!
				if (distance > 1) return float4(0, 0, 0, 0);

				// Get the angle between the right vector and the texture coordinate
				float angle = CalculateVectorAngle(i.uv, distance);

				angle += 2 * PI * (normalizedCameraBottomLeft.x + normalizedCameraDimensions.x / 2) + PI / 2;

				// Use angle and distance to get unwarped position
				float2 unwarpedUV = UnWarp(distance, angle);

				// Ok finally sample the texture
				half4 totalColor;
				if (
					unwarpedUV.x > normalizedCameraBottomLeft.x && 
					unwarpedUV.x < normalizedCameraBottomLeft.x + normalizedCameraDimensions.x && 
					unwarpedUV.y > normalizedCameraBottomLeft.y &&
					unwarpedUV.y < normalizedCameraBottomLeft.y + normalizedCameraDimensions.y)
				{
					unwarpedUV -= normalizedCameraBottomLeft;
					unwarpedUV /= normalizedCameraDimensions;

					float2 clampedUV = floor(unwarpedUV / _MainTex_TexelSize) + float2(.5, .5);
					clampedUV *= _MainTex_TexelSize;

					totalColor = tex2D(_MainTex, clampedUV);
				}
				else
				{
					totalColor = float4(1, 0, 0, 1);
				}

				// Apply a fade around the _InnerRadius
				totalColor = lerp(_InnerColor, totalColor, min(1, pow((distance - _InnerRadius) / _InnerFadeSize, _InnerFadeExp)));

				// Check if distance is within inner circle, if so use solid color instead of texture samples
				bool isInsideInnerRadius = distance < _InnerRadius;

				return isInsideInnerRadius * _InnerColor + !isInsideInnerRadius * totalColor;
      }
      ENDCG
    }
  }
}