using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarWarpEffect : MonoBehaviour
{

    public Material warpMaterial;
    public RenderTexture overlay;
    public Material overlayMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, warpMaterial);

        if (overlay && overlayMaterial)
        {
            Graphics.Blit(overlay, destination, overlayMaterial);
        }
    }
}
