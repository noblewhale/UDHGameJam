using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(OutlineRenderer), PostProcessEvent.BeforeTransparent, "Custom/Outline")]
public sealed class Outline : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("Outline thickness.")]
    public FloatParameter thickness = new FloatParameter { value = 0.5f };
}
public sealed class OutlineRenderer : PostProcessEffectRenderer<Outline>
{
    Material material;
    public override void Render(PostProcessRenderContext context)
    {
        if (material == null) material = new Material(Shader.Find("Hidden/Custom/Outline"));
        material.SetFloat("_Thickness", settings.thickness);
        context.command.Blit(context.source, context.destination, material);
    }
}