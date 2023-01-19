namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.Rendering.PostProcessing;

    /// <summary>Copies the previous depth buffer into the current depth buffer so that it is not lost</summary>
    /// <remarks>
    /// This helps the wrapped camera system behave more like a single camera, allowing post-processing effects
    /// that depend on the depth buffer to work properly.
    /// </remarks>
    [Serializable]
    [PostProcess(typeof(CircleWarpRenderer), PostProcessEvent.BeforeStack, "Custom/CircleWarp", false)]
    public sealed class CircleWarp : PostProcessEffectSettings
    {
        public FloatParameter SeaLevel = new FloatParameter() { value = 5 };
        public ColorParameter InnerColor = new ColorParameter() { value = Color.clear };
        public FloatParameter InnerRadius = new FloatParameter() { value = .1f };
        public FloatParameter InnerFadeSize = new FloatParameter() { value = .25f };
        public FloatParameter InnerFadeExp = new FloatParameter() { value = 4.0f };

        [Tooltip("" +
            "A value of 1 will warp the map fully around the circle. " +
            "Lower values will warp only partially around the circle. " +
            "Values larger than 1 will 'over-warp' the map, so only part of the map will fit in the circle " +
            "and the player will have to go more than once around the circle to traverse the entire map width."
        )]
        public FloatParameter WarpAmount = new FloatParameter() { value = 1 };

        // Get the angle between the right vector and the input
        float CalculateVectorAngle(Vector2 input, float distance)
        {
            // Normalize so we can get the angle
            input /= distance;

            float dotProduct = Vector2.Dot(Vector2.right, input);
            // The dot product can sometimes become larger than 1 when the input vector is very close to the right vector.
            // This would cause the angle to be NAN, leading to some rendering artifacts, so let's not let that happen.
            // Also apparently there's some problem at -1 that creates a seam but adding this small arbitrary amount fixes it...
            dotProduct = Mathf.Clamp(dotProduct, -1 + .0000001f, 1);

            // Yay the angle (almost)
            float angle = Mathf.Acos(dotProduct);

            // Depending on the z component of the cross product we may need to invert the angle
            float3 check = Vector3.Cross(Vector2.right, new Vector3(input.x, input.y, 0));
            // Invert if necessary. Otherwise angle would cycle twice between 0 and PI and only half the texture would get sampled and mirrored
            if (check.z < 0)
            {
                angle = 2 * Mathf.PI - angle;
            }

            return angle;
        }

        // Convert distance to unwarped y value.
        float UnWarpY(float distance)
        {
            // Renormalize d taking into account the inner radius. Now d goes from 0 at the edge of the inner radius to 1 at edge of the texture coordinates
            float y = (distance - InnerRadius.value) / (1 - InnerRadius.value);

            // Y is inverted because low distance from center should correspond to high y values
            // The play area is in the bottom half of the circle and up should be up.
            return 1 - y;
        }

        // Convert angle to unwarped x value
        float UnWarpX(float angle)
        {
            // How far around the circle
            float x = angle / (2 * Mathf.PI);
            // Wrap between 0 and 1 because rotation may have made angle negative or greater than 2*PI
            x -= Mathf.Floor(x);
            return x;
        }

        // Use distance and angle to calculate the unwarped positon
        Vector2 UnWarp(float distance, float angle)
        {
            return new Vector2(UnWarpX(angle), UnWarpY(distance));
        }

        public bool UnwarpPosition(Vector2 warpedPos, out Vector2 unwarpedPos)
        {
            Vector2 warpedAreaDimensions = GetWarpedAreaDimensions();
            Vector2 warpedAreaPosition = GetWarpedAreaPosition(warpedAreaDimensions);
            float _CameraOffset = GetMapNormalizedCameraOffset(warpedAreaDimensions);
            Vector2 _CameraDimensions = GetMapNormalizedCameraDimensions(warpedAreaDimensions);
            Vector2 _CameraPosition = GetMapNormalizedCameraPosition(warpedAreaDimensions, warpedAreaPosition);
            float _CameraAspect = Camera.main.aspect;

            unwarpedPos = new Vector2();

            warpedPos.x /= Screen.width;
            warpedPos.y /= Screen.height;

            // Transform texture coordinates to be relative to the center. The values go from -1 to 1
            warpedPos = warpedPos * 2 - Vector2.one;

            // Scale so that camera area can fill the screen
            warpedPos *= _CameraDimensions.y / 2 + _CameraOffset;
            // Put center of circle at top of screen
            warpedPos.y -= 1;
            // Shift so camera target is center of screen
            warpedPos.y += _CameraPosition.y + _CameraDimensions.y / 2 - _CameraOffset;

            // Become a circle instead of a screen-shaped oval
            warpedPos.x *= _CameraAspect;

            // The distance from center to the texture coord
            float distance = Mathf.Sqrt(warpedPos.x * warpedPos.x + warpedPos.y * warpedPos.y);

            // Discard corner pixels, we are rendering a circle, circles don't have corners!
            if (distance > 1) return false;

            // Get the angle between the right vector and the texture coordinate
            float angle = CalculateVectorAngle(warpedPos, distance);

            // Adjust angle based on camera x position + a constant to get the player at the bottom
            angle += 2 * Mathf.PI * (_CameraPosition.x + _CameraDimensions.x / 2) + Mathf.PI / 2;

            // Use angle and distance to get unwarped position
            unwarpedPos = UnWarp(distance, angle);

            // Ok finally sample the texture, as long as we are within the camera view area
            if (
                unwarpedPos.x > _CameraPosition.x &&
                unwarpedPos.x < _CameraPosition.x + _CameraDimensions.x &&
                unwarpedPos.y > _CameraPosition.y &&
                unwarpedPos.y < _CameraPosition.y + _CameraDimensions.y)
            {
                // Re-normalize to camera dimensions
                unwarpedPos -= _CameraPosition;
                unwarpedPos /= _CameraDimensions;

                // And away we go
                return true;
            }
            else
            {
                return false;
            }
        }

        public Vector2 GetWarpedAreaDimensions()
        {
            Vector2 warpedAreaDimensions = Map.instance.totalArea.size / WarpAmount;
            // This magical bit adjusts the map width/height ratio so that the center of the camera target is completely unscaled
            warpedAreaDimensions.y = (warpedAreaDimensions.x / (2 * Mathf.PI)) - PlayerCamera.instance.cameraOffset + Camera.main.GetSize().y / 2;

            return warpedAreaDimensions;
        }

        public Vector2 GetWarpedAreaPosition(Vector2 warpedAreaDimensions)
        {
            // Position the map so that the bottom of the area aligns with the bottom of the camera
            float x = Map.instance.totalArea.center.x;
            float y = Camera.main.transform.position.y + (warpedAreaDimensions.y / 2 - Camera.main.orthographicSize);
            return new Vector2(x, y);
        }

        public Vector2 GetMapNormalizedCameraPosition(Vector2 warpedAreaDimensions, Vector2 warpedAreaPosition)
        {
            Vector2 cameraPosition = Camera.main.transform.position;

            // Relative to warped area center
            cameraPosition -= warpedAreaPosition;

            // Just kidding, relative to warped area bottom left corner
            cameraPosition += warpedAreaDimensions / 2;

            // But actually we want the bottom left of the camera view area
            cameraPosition -= Camera.main.GetSize() / 2;

            // And all in map normalized coords of course
            cameraPosition /= warpedAreaDimensions;

            return cameraPosition;
        }

        public Vector2 GetMapNormalizedCameraDimensions(Vector2 warpedAreaDimensions)
        {
            return Camera.main.GetSize() / warpedAreaDimensions;
        }

        public float GetMapNormalizedCameraOffset(Vector2 warpedAreaDimensions)
        {
            return PlayerCamera.instance.cameraOffset / warpedAreaDimensions.y;
        }
    }
    public sealed class CircleWarpRenderer : PostProcessEffectRenderer<CircleWarp>
    {
        const string warpShaderName = "Custom/CircleWarp";
        Shader warpShader;

        public override void Init()
        {
            base.Init();
            warpShader = Shader.Find(warpShaderName);
        }

        public override void Render(PostProcessRenderContext context)
        {
            if (!Map.instance) return;

            var sheet = context.propertySheets.Get(warpShader);

            sheet.properties.SetFloat("_SeaLevel", settings.SeaLevel);
            sheet.properties.SetColor("_InnerColor", settings.InnerColor);
            sheet.properties.SetFloat("_InnerRadius", settings.InnerRadius);
            sheet.properties.SetFloat("_InnerFadeSize", settings.InnerFadeSize);
            sheet.properties.SetFloat("_InnerFadeExp", settings.InnerFadeExp);

            Vector2 warpedAreaDimensions = settings.GetWarpedAreaDimensions();
            Vector2 warpedAreaPosition = settings.GetWarpedAreaPosition(warpedAreaDimensions);
            float cameraOffset = settings.GetMapNormalizedCameraOffset(warpedAreaDimensions);
            Vector2 cameraDimensions = settings.GetMapNormalizedCameraDimensions(warpedAreaDimensions);
            Vector2 cameraPosition = settings.GetMapNormalizedCameraPosition(warpedAreaDimensions, warpedAreaPosition);

            sheet.properties.SetVector("_CameraDimensions", cameraDimensions);
            sheet.properties.SetVector("_CameraPosition", cameraPosition);
            sheet.properties.SetFloat("_CameraOffset", cameraOffset);
            sheet.properties.SetFloat("_CameraAspect", Camera.main.aspect);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}