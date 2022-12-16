﻿Shader "Unlit/CircleWarpSimple"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white"
		_WrapTexture("Wrap Texture", 2D) = "white"
		_Depth("Depth", 2D) = "white" {}
		_InnerColor("Inner Color", Color) = (0, 0, 0, 0)
		_SeaLevel("Sea Level", Float) = 5
		_Rotation("Rotation", Float) = 0
		_InnerRadius("Inner Radius", Float) = .1
		_InnerFadeSize("Inner Fade Size", Float) = .25
		_InnerFadeExp("Inner Fade Exponent", Float) = 4.0
		_CameraPos("Camera Position", Vector) = (.1, .1, 0, 0)
		_CameraDim("Camera Dimensions", Vector) = (.8, .8, 0, 0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline"}
		LOD 100

		Pass
		{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull back

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// It's a PI
			#define PI 3.141592653589793238462643;

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
			
			// This macro declares _BaseMap as a Texture2D object.
			TEXTURE2D(_MainTex);
			// This macro declares the sampler for the _BaseMap texture.
			SAMPLER(sampler_MainTex);
			
			CBUFFER_START(UnityPerMaterial)
				// The following line declares the _BaseMap_ST variable, so that you
				// can use the _BaseMap variable in the fragment shader. The _ST
				// suffix is necessary for the tiling and offset function to work.
				float4 _MainTex_ST;
			CBUFFER_END
			
				// This macro declares _BaseMap as a Texture2D object.
			TEXTURE2D(_WrapTexture);
			// This macro declares the sampler for the _BaseMap texture.
			SAMPLER(sampler_WrapTexture);

			CBUFFER_START(UnityPerMaterial)
				// The following line declares the _BaseMap_ST variable, so that you
				// can use the _BaseMap variable in the fragment shader. The _ST
				// suffix is necessary for the tiling and offset function to work.
				float4 _WrapTexture_ST;
			CBUFFER_END

			float4 _MainTex_TexelSize;
			// The depth texture to use for outlines
			sampler2D _Depth;
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

			// Angles are calculated from this vector
			// At large scales there are rendering errors near this vector so we don't use RIGHT where the player is rendered.
			static const float3 RIGHT = float3(1, 0, 0);
			
			// Just your standard vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
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
			}

			// Use distance and angle to calculate the unwarped positon
			float2 UnWarp(float distance, float angle)
			{
				return float2(UnWarpX(angle), UnWarpY(distance));
			}

			half4 frag(v2f i) : SV_Target
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
				if (unwarpedUV.y < _CameraPos.y || unwarpedUV.y > _CameraPos.y + _CameraDim.y)
				{
					totalColor = float4(1, 0, 0, 1);
				}
				else if (unwarpedUV.x >= _CameraPos.x && unwarpedUV.x <= _CameraPos.x + _CameraDim.x)
				{
					unwarpedUV.x = (unwarpedUV.x - _CameraPos.x) / (_CameraDim.x);
					unwarpedUV.y = (unwarpedUV.y - _CameraPos.y) / _CameraDim.y;
					/*if (_CameraPos.x + _CameraDim.x / 2 > .5)
					{
						unwarpedUV.x = floor(unwarpedUV.x / _MainTex_TexelSize.x) * _MainTex_TexelSize.x;
					}
					else*/
					//{
						//unwarpedUV.x = ceil(unwarpedUV.x / _MainTex_TexelSize.x) * _MainTex_TexelSize.x;
					//}
					totalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, unwarpedUV);
					//totalColor = tex2D(_MainTex, unwarpedUV);
				}
				else if (unwarpedUV.x >= _CameraPos.x - 1 && unwarpedUV.x <= _CameraPos.x - 1 + _CameraDim.x)
				{
					unwarpedUV.x = (unwarpedUV.x - (_CameraPos.x - 1)) / (_CameraDim.x);
					unwarpedUV.y = (unwarpedUV.y - _CameraPos.y) / _CameraDim.y;
					// You're just going to have to trust me on this one -------V
					//unwarpedUV.x = (ceil(unwarpedUV.x / _MainTex_TexelSize.x) + 1) * _MainTex_TexelSize.x;
					totalColor = SAMPLE_TEXTURE2D(_WrapTexture, sampler_WrapTexture, unwarpedUV);
					//totalColor = tex2D(_WrapTexture, unwarpedUV);
				}
				else if (unwarpedUV.x >= _CameraPos.x + 1 && unwarpedUV.x <= _CameraPos.x + 1 + _CameraDim.x)
				{
					unwarpedUV.x = (unwarpedUV.x - (_CameraPos.x + 1)) / (_CameraDim.x);
					unwarpedUV.y = (unwarpedUV.y - _CameraPos.y) / _CameraDim.y;
					//unwarpedUV.x = floor(unwarpedUV.x / _MainTex_TexelSize.x) * _MainTex_TexelSize.x;
					totalColor = SAMPLE_TEXTURE2D(_WrapTexture, sampler_WrapTexture, unwarpedUV);
					//totalColor = tex2D(_WrapTexture, unwarpedUV);
				}
				else
				{
					totalColor = float4(0, 1, 0, 1);
				}

				// Apply a fade around the _InnerRadius
				totalColor = lerp(_InnerColor, totalColor, min(1, pow((distance - _InnerRadius)/_InnerFadeSize, _InnerFadeExp)));
				
				// Check if distance is within inner circle, if so use solid color instead of texture samples
				bool isInsideInnerRadius = distance < _InnerRadius;

				return isInsideInnerRadius * _InnerColor + !isInsideInnerRadius * totalColor;

				//return totalColor;
			}
			ENDHLSL
		}
	}
}
