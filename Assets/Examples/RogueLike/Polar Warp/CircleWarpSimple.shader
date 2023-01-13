﻿Shader "Unlit/CircleWarpSimple"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white"
		_WrapTexture("Wrap Texture", 2D) = "white"
		_Depth("Depth", 2D) = "white" {}
		_WrapDepth("Wrap Depth", 2D) = "white" {}
		_InnerColor("Inner Color", Color) = (0, 0, 0, 0)
		_SeaLevel("Sea Level", Float) = 5
		_Rotation("Rotation", Float) = 0
		_InnerRadius("Inner Radius", Float) = .1
		_InnerFadeSize("Inner Fade Size", Float) = .25
		_InnerFadeExp("Inner Fade Exponent", Float) = 4.0
		_CameraPos("Camera Position", Vector) = (.1, .1, 0, 0)
		_CameraDim("Camera Dimensions", Vector) = (.8, .8, 0, 0)
		_OutlineThickness("Outline Thickness", Vector) = (.0005, .004, 0, 0)
		[HDR]_OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
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
				float4 vertex : SV_POSITION;
			};

			// The texture to warp
			sampler2D _MainTex;
			sampler2D _WrapTexture;

			float4 _MainTex_TexelSize; 
			// The depth texture to use for outlines
			sampler2D _Depth;
			// The depth texture to use for outlines
			sampler2D _WrapDepth;
			// The rotation to apply to the circular warp
			float _Rotation;
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
			float2 _CameraPos;
			float2 _CameraDim;
			// The thickness to use for the outline
			float4 _OutlineThickness;
			// The color of the outline
			float4 _OutlineColor;

			// Angles are calculated from this vector
			// At large scales there are rendering errors near this vector so we don't use RIGHT where the player is rendered.
			static const float3 RIGHT = float3(1, 0, 0);
			// It's a PI
			static const float PI = 3.1415926535897932384626433832795028841971693993751058209749445923078164062;
			
			// Just your standard vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
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

				return 0; 
			}

			// Use distance and angle to calculate the unwarped positon
			float2 UnWarp(float distance, float angle)
			{
				return float2(UnWarpX(angle), UnWarpY(distance));
			}

			// For grabbing adjacent pixels for outline stuff
			// Handles wrapping the x value within the middle third of the texture
			float2 GetAdjacent(float2 pos, int xDir, int yDir)
			{
				// Calculate adjacent location
				float xDif = _OutlineThickness.x * xDir;
				float yDif = _OutlineThickness.y * yDir;
				float2 adjacent = float2(pos.x + xDif, pos.y + yDif);

				return adjacent;
			}

			bool IsOutline(float2 uv, sampler2D depth_texture)
			{
				// Get some adjacent locations, accounting for x wrapping
				float2 adjacentUV1 = GetAdjacent(uv, 1, 1);
				float2 adjacentUV2 = GetAdjacent(uv, -1, -1); 
				float2 adjacentUV3 = GetAdjacent(uv, -1, 1);
				float2 adjacentUV4 = GetAdjacent(uv, 1, -1); 

				// Sample the depth at the current pixel location as well as the adjacent locations
				float depthMe = SAMPLE_DEPTH_TEXTURE(depth_texture, uv);
				float depth0 = SAMPLE_DEPTH_TEXTURE(depth_texture, adjacentUV1);
				float depth1 = SAMPLE_DEPTH_TEXTURE(depth_texture, adjacentUV2);
				float depth2 = SAMPLE_DEPTH_TEXTURE(depth_texture, adjacentUV3);
				float depth3 = SAMPLE_DEPTH_TEXTURE(depth_texture, adjacentUV4);

				// If the difference between the depths of opposite pixels is large then this is an outline pixel (maybe)
				bool isOutline = abs(depth0 - depth1) > .8f || abs(depth2 - depth3) > .8f;
				// If current pixel depth is the max among neighbors then don't treat it as outline.
				// This keeps the outline strictly outside of the thing being outlined
				// Otherwise the outline would be centered on the edge and half of it would overlap the thing being outlined.
				isOutline = isOutline && (depthMe != max(max(max(depth0, depth1), depth2), depth3));

				return isOutline;
			}

			fixed4 frag(v2f i) : SV_Target
			{ 				
				// Transform texture coordinates to be relative to the center. The values go from -1 to 1
				i.uv = i.uv * 2 - 1;
				
				// The distance from center to the texture coord
				float distance = sqrt(i.uv.x * i.uv.x + i.uv.y * i.uv.y);

				// Discard corner pixels, we are rendering a circle, circles don't have corners!
				if (distance > 1) discard;

				// Get the angle between the right vector and the texture coordinate
				float angle = CalculateVectorAngle(i.uv, distance);

				angle += 2 * PI * (_CameraPos.x + _CameraDim.x / 2) + PI/2;

				// Use angle and distance to get unwarped position
				float2 unwarpedUV = UnWarp(distance, angle);

				unwarpedUV.y += _CameraPos.y; 

				// Ok finally sample the texture
				half4 totalColor;
				bool isOutline = false;
				float depthMe = 0;
				if (unwarpedUV.y < _CameraPos.y || unwarpedUV.y > _CameraPos.y + _CameraDim.y)
				{
					totalColor = float4(1, 0, 0, 1);
				}
				else if (unwarpedUV.x >= _CameraPos.x && unwarpedUV.x <= _CameraPos.x + _CameraDim.x)
				{
					unwarpedUV.x = (unwarpedUV.x - _CameraPos.x) / (_CameraDim.x);
					unwarpedUV.y = (unwarpedUV.y - _CameraPos.y) / _CameraDim.y;
					totalColor = tex2D(_MainTex, unwarpedUV);
					isOutline = IsOutline(unwarpedUV, _Depth);
					depthMe = SAMPLE_DEPTH_TEXTURE(_Depth, unwarpedUV);
				}
				else if (unwarpedUV.x >= _CameraPos.x - 1 && unwarpedUV.x <= _CameraPos.x - 1 + _CameraDim.x)
				{
					unwarpedUV.x = (unwarpedUV.x - (_CameraPos.x - 1)) / (_CameraDim.x);
					unwarpedUV.y = (unwarpedUV.y - _CameraPos.y) / _CameraDim.y;
					totalColor = tex2D(_WrapTexture, unwarpedUV);
					isOutline = IsOutline(unwarpedUV, _WrapDepth);
					depthMe = SAMPLE_DEPTH_TEXTURE(_WrapDepth, unwarpedUV);
				}
				else if (unwarpedUV.x >= _CameraPos.x + 1 && unwarpedUV.x <= _CameraPos.x + 1 + _CameraDim.x)
				{
					unwarpedUV.x = (unwarpedUV.x - (_CameraPos.x + 1)) / (_CameraDim.x);
					unwarpedUV.y = (unwarpedUV.y - _CameraPos.y) / _CameraDim.y;
					totalColor = tex2D(_WrapTexture, unwarpedUV);
					isOutline = IsOutline(unwarpedUV, _WrapDepth);
					depthMe = SAMPLE_DEPTH_TEXTURE(_WrapDepth, unwarpedUV);
				}
				else
				{
					totalColor = float4(0, 1, 0, 1);
				}

				// Apply a fade around the _InnerRadius
				totalColor = lerp(_InnerColor, totalColor, min(1, pow((distance - _InnerRadius)/_InnerFadeSize, _InnerFadeExp)));
				
				float4 outlineColor = isOutline * _OutlineColor;
				totalColor.rgb = totalColor.rgb * (1 - outlineColor.a) + outlineColor.rgb * outlineColor.a;
				totalColor.a += outlineColor.a;

				// Check if distance is within inner circle, if so use solid color instead of texture samples
				bool isInsideInnerRadius = distance < _InnerRadius;

				return isInsideInnerRadius * _InnerColor + !isInsideInnerRadius * totalColor;

				return totalColor;
			}
			ENDCG
		}
	}
}
