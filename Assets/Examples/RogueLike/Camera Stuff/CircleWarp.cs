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
    }
    public sealed class CircleWarpRenderer : PostProcessEffectRenderer<CircleWarp>
    {
        const string warpShaderName = "Custom/CircleWarp";
        Shader warpShader;

        /// <summary>Cache all the things because render loop must be tight. Tight tight. Tight tight tight.</summary>
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

            Vector2 cameraPosition = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
            Vector2 cameraDimensions = new Vector2(Camera.main.orthographicSize * 2 * Camera.main.aspect, Camera.main.orthographicSize * 2);
            Vector2 warpedAreaDimensions = Map.instance.totalArea.size / settings.WarpAmount;
            // This magical bit adjusts the map width/height ratio so that the center of the camera target is completely unscaled
            warpedAreaDimensions.y = (warpedAreaDimensions.x / (2 * Mathf.PI)) - PlayerCamera.instance.cameraOffset + cameraDimensions.y / 2;
            // Position the map so that the bottom of the area aligns with the bottom of the camera
            Vector2 warpedAreaPosition = new Vector2(Map.instance.totalArea.center.x, cameraPosition.y + (warpedAreaDimensions.y / 2 - Camera.main.orthographicSize));

            float cameraOffset = PlayerCamera.instance.cameraOffset;

            // Normalize to map dimensions
            cameraDimensions /= warpedAreaDimensions;
            cameraPosition -= warpedAreaPosition;
            cameraPosition /= warpedAreaDimensions;
            cameraOffset /= warpedAreaDimensions.y;

            // Camera position needs to be bottom left corner, not center
            cameraPosition = cameraPosition - cameraDimensions / 2 + Vector2.one * .5f;

            sheet.properties.SetVector("_CameraDimensions", cameraDimensions);
            sheet.properties.SetVector("_CameraPosition", cameraPosition);
            sheet.properties.SetFloat("_CameraOffset", cameraOffset);
            sheet.properties.SetFloat("_CameraAspect", Camera.main.aspect);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}