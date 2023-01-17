namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
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
        public FloatParameter FakeMapHeight = new FloatParameter() { value = 20 };
    }
    public sealed class CircleWarpRenderer : PostProcessEffectRenderer<CircleWarp>
    {
        const string warpShaderName = "Custom/CircleWarpNew";
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
            float fakeMapHeight = Map.instance.width / Mathf.PI;
            sheet.properties.SetVector("_MapDimensions", new Vector2(Map.instance.width, fakeMapHeight));
            sheet.properties.SetVector("_MapPosition", new Vector2(Map.instance.totalArea.center.x, Camera.main.transform.position.y + (fakeMapHeight / 2 - Camera.main.orthographicSize)));
            sheet.properties.SetVector("_CameraDimensions", new Vector2(Camera.main.orthographicSize * 2 * Camera.main.aspect, Camera.main.orthographicSize * 2));
            sheet.properties.SetVector("_CameraPosition", new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y));
            sheet.properties.SetFloat("_CameraOffset", PlayerCamera.instance.cameraOffset);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}