using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarWarpEffect : MonoBehaviour
{

    public Material warpMaterial;
    public Camera foregroundCamera;
    public Material overlayMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //if (foregroundCamera && foregroundCamera.targetTexture && overlayMaterial)
        //{
        //    Graphics.Blit(foregroundCamera.targetTexture, destination, overlayMaterial);
        //}
        Graphics.Blit(source, destination, warpMaterial);

        if (foregroundCamera && foregroundCamera.targetTexture && overlayMaterial)
        {
            Graphics.Blit(foregroundCamera.targetTexture, destination, overlayMaterial);
        }
    }
}
