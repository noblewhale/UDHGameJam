using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// <summary>Copies the previous depth buffer into the current depth buffer so that it is not lost</summary>
/// <remarks>
/// This helps the wrapped camera system behave more like a single camera, allowing post-processing effects
/// that depend on the depth buffer to work properly.
/// </remarks>
[Serializable]
[PostProcess(typeof(WrapEffectRenderer), PostProcessEvent.BeforeTransparent, "Custom/Wrap")]
public sealed class Wrap : PostProcessEffectSettings
{
}
public sealed class WrapEffectRenderer: PostProcessEffectRenderer<Wrap>
{
    Material additiveMaterial;
    public override void Render(PostProcessRenderContext context)
    {
        if (additiveMaterial == null) additiveMaterial = new Material(Shader.Find("Custom/Additive"));

        var sourceDepth = Shader.GetGlobalTexture("_MainCameraDepthTexture");
        var destDepth = Shader.GetGlobalTexture("_CameraDepthTexture");

        if (sourceDepth && destDepth)
        {
            context.command.SetGlobalTexture("_MainTex", destDepth);
            context.command.Blit(sourceDepth, destDepth, additiveMaterial);
        }
    }
}