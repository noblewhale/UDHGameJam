// This monster does many thing
// Primarily it applies a circular warp to a render texture and handles rotating it
// It also outlines things using the depth texture
// And does pixel level wrapping (camera should be twice as wide as visible area)
// Also some fading for the center of the circle
Shader "Unlit/CircleWarp"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Depth("Depth", 2D) = "white" {}
		_InnerColor("Inner Color", Color) = (0, 0, 0, 0)
		_SeaLevel("Sea Level", Float) = 5
		_Rotation("Rotation", Float) = 0
		_OutlineThickness("Outline Thickness", Vector) = (.0005, .004, 0, 0)
		_OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
		_InnerRadius("Inner Radius", Float) = .1
		_InnerFadeSize("Inner Fade Size", Float) = .25
		_InnerFadeExp("Inner Fade Exponent", Float) = 4.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull back

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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

			// The texture to warp
			sampler2D _MainTex;
			// The depth texture to use for outlines
			sampler2D _Depth;
			// The rotation to apply to the circular warp
			float _Rotation;
			// A magic number for adjusting the appearance of the warp
			float _SeaLevel;
			// The thickness to use for the outline
			float4 _OutlineThickness;
			// The color of the outline
			float4 _OutlineColor;
			// The radius of the inner circle
			float _InnerRadius;
			// The size of the fade effect on the inner circle
			float _InnerFadeSize;
			// How sharply the fade falls off
			float _InnerFadeExp;
			// The color to use for the inner circle
			float4 _InnerColor;

			// Everything left of MIN_X is overlayed on the right side for screen-wrapping
			static const float MIN_X = .25f;
			// Everything right of MAX_X is overlayed on the left side for screen-wrapping
			static const float MAX_X = .75f;
			// Angles are calculated from this vector
			static const float3 DOWN = float3(0, -1, 0);
			// It's a PI
			static const float PI = 3.14159f;
			
			// Just your standard vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			// For grabbing adjacent pixels for outline stuff
			// Handles wrapping the x value within the middle third of the texture
			float2 GetAdjacent(float2 pos, int xDir, int yDir)
			{
				// Calculate adjacent location
				float xDif = _OutlineThickness.x * xDir;
				float yDif = _OutlineThickness.y * yDir;
				float2 adjacent = float2(pos.x + xDif, pos.y + yDif);

				// Wrap X
				bool lessThan = adjacent.x < MIN_X;
				bool greaterThan = adjacent.x > MAX_X;
				float wrappedX = 0;
				wrappedX += lessThan * (.5f + adjacent.x);
				wrappedX += greaterThan * (adjacent.x - .5f);
				wrappedX += (!lessThan && !greaterThan) * adjacent.x;
				adjacent.x = wrappedX;

				return adjacent;
			}

			bool IsOutline(float2 uv)
			{
				// Get some adjacent locations, accounting for x wrapping
				float2 adjacentUV1 = GetAdjacent(uv, 1, 1);
				float2 adjacentUV2 = GetAdjacent(uv, -1, -1);
				float2 adjacentUV3 = GetAdjacent(uv, -1, 1);
				float2 adjacentUV4 = GetAdjacent(uv, 1, -1);

				// Sample the depth at the current pixel location as well as the adjacent locations
				float depthMe = SAMPLE_DEPTH_TEXTURE(_Depth, uv);
				float depth0 = SAMPLE_DEPTH_TEXTURE(_Depth, adjacentUV1);
				float depth1 = SAMPLE_DEPTH_TEXTURE(_Depth, adjacentUV2);
				float depth2 = SAMPLE_DEPTH_TEXTURE(_Depth, adjacentUV3);
				float depth3 = SAMPLE_DEPTH_TEXTURE(_Depth, adjacentUV4);

				// If the difference between the depths of opposite pixels is large then this is an outline pixel (maybe)
				bool isOutline = abs(depth0 - depth1) > .5f || abs(depth2 - depth3) > .5f;
				// If current pixel depth is the max among neighbors then don't treat it as outline.
				// This keeps the outline strictly outside of the thing being outlined
				// Otherwise the outline would be centered on the edge and half of it would overlap the thing being outlined.
				isOutline = isOutline && (depthMe != max(max(max(depth0, depth1), depth2), depth3));

				return isOutline;
			}

			// Get the angle between the up vector and the input
			float CalculateVectorAngle(float2 input, float distance)
			{
				// Normalize so we can get the angle
				input /= distance;
				// The angle between the up vector to the normalized direction vector
				float angle = acos(dot(DOWN.xy, input));
				// Depending on the z component of the cross product we may need to invert the angle
				float3 check = cross(DOWN, float3(input.x, input.y, 0));
				// Invert if necessary. Otherwise angle cycles twice between 0 and PI and only half the texture gets sampled and mirrored
				angle = angle * (check.z >= 0) + (2 * PI - angle) * (check.z < 0);

				// Apply rotation
				angle -= _Rotation;

				return angle;
			}

			// Convert distance to unwarped y value.
			float UnWarpY(float distance)
			{
				// Renormalize d taking into account the inner radius. Now d goes from 0 at the edge of the inner radius to 1 at edge of the texture coordinates
				float y = (distance - _InnerRadius) / (1 - _InnerRadius);

				// This is literally just a random equation that gave a visual effect that I liked. I guess it's some sort of log with an adjustable base.
				// Without this the warp near the center is too extreme and nauseating
				y = log(1 + y * _SeaLevel) / log(1 + _SeaLevel);

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

			// Overlay either the left quarter or right quarter of the texture so achieve wrapping
			float4 Wrap(float2 uv, fixed4 totalColor)
			{
				// Sample the left quarter
				// This allows foreground objects to move off the left of the map but appear as if they are re-entering from the right
				float2 wrappedUV = uv;
				wrappedUV.x = (uv.x - .5f) / 2.0f;
				float4 leftColor = tex2D(_MainTex, wrappedUV);

				// Sample the right quarter for similar reasons
				wrappedUV.x = uv.x / 2.0f + MAX_X;
				float4 rightColor = tex2D(_MainTex, wrappedUV);

				// If left of center, overlay the right quarter. If right of center, overlay the left quarter.
				bool isRightOfCenter = uv.x > .5f;
				float4 color = isRightOfCenter * leftColor + !isRightOfCenter * rightColor;

				// Yay blending
				totalColor.rgb = totalColor.rgb * (1 - color.a) + color.rgb * color.a;
				totalColor.a += color.a;

				return totalColor;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				// Transform texture coordinates to be relative to the center. The values go from -1 to 1
				i.uv = i.uv * 2 - 1;

				// The distance from center to the texture coord
				float distance = sqrt(i.uv.x * i.uv.x + i.uv.y * i.uv.y);

				// Discard corner pixels, we are rendering a circle, circles don't have corners!
				if (distance > 1) discard;

				// Get the angle between the down vector and the texture coordinate
				float angle = CalculateVectorAngle(i.uv, distance);

				// Use angle and distance to get unwarped position
				float2 unwarpedUV = UnWarp(distance, angle);

				// Transform the unwarped position to account for the extra wide camera
				float2 squishedUV = unwarpedUV;
				// The entire map exists between .25 and .75, the extra space will be used for wrapping
				squishedUV.x = squishedUV.x / 2.0f + MIN_X;
				
				// Ok finally sample the texture
				fixed4 totalColor = tex2D(_MainTex, squishedUV);

				// Apply the wrapping (more texture samples)
				totalColor = Wrap(unwarpedUV, totalColor);

				// Apply a fade around the _InnerRadius
				totalColor = lerp(_InnerColor, totalColor, min(1, pow((distance - _InnerRadius)/_InnerFadeSize, _InnerFadeExp)));
				
				// Use the depth buffer to determine if this is an outline pixel or not
				bool isOutline = IsOutline(squishedUV);

				// Check if distance is within inner circle, if so use solid color instead of texture samples
				bool isInsideInnerRadius = distance < _InnerRadius;

				return isInsideInnerRadius * _InnerColor + !isInsideInnerRadius * (isOutline * _OutlineColor + !isOutline * totalColor);
			}
			ENDCG
		}
	}
}
