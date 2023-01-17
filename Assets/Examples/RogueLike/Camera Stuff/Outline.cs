using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>Adds an outline effect via the Custom/Outline shader with the help of the _CameraDepthTexture</summary>
[Serializable]
[PostProcess(typeof(OutlineRenderer), PostProcessEvent.BeforeStack, "Custom/Outline")]
public sealed class Outline : PostProcessEffectSettings
{
    [Range(0f, .1f), Tooltip("Outline thickness.")]
    public FloatParameter thickness = new FloatParameter { value = 0.5f };

    [Tooltip("The outline color.")]
    public ColorParameter color = new ColorParameter { value = Color.red };
}

public sealed class OutlineRenderer : PostProcessEffectRenderer<Outline>
{
    const string outlineShaderName = "Custom/Outline";
    Shader outlineShader;
    int thicknessPropertyID;
    int colorPropertyID;

    /// <summary>Cache all the things because render loop must be tight. Tight tight. Tight tight tight.</summary>
    public override void Init()
    {
        base.Init();

        outlineShader = Shader.Find(outlineShaderName);
        thicknessPropertyID = Shader.PropertyToID("_Thickness");
        colorPropertyID = Shader.PropertyToID("_Color");
    }

    /// <summary>Not much really happens here, just set the properties and blit to target</summary>
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(outlineShader);
        sheet.properties.SetFloat(thicknessPropertyID, settings.thickness);
        sheet.properties.SetColor(colorPropertyID, settings.color);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}