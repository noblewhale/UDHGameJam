using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(WrapRenderer), PostProcessEvent.BeforeTransparent, "Custom/Wrap")]
public sealed class WrapPostProcessingEffect : PostProcessEffectSettings
{
    [Tooltip("Level width.")]
    public FloatParameter levelWidth = new FloatParameter { value = 10f };
}
public sealed class WrapRenderer : PostProcessEffectRenderer<WrapPostProcessingEffect>
{
    public override void Render(PostProcessRenderContext context)
    {
        //Rect viewport = new Rect();
        //viewport.center = (Vector2)context.camera.transform.position + Vector2.right * settings.levelWidth;
        //viewport.height = context.camera.orthographicSize * 2;
        //viewport.width = viewport.height * context.camera.aspect;
        //context.command.BlitFullscreenTriangle(context.source, context.destination, false, null, true);
    }
}