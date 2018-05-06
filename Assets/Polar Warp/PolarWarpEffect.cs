using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarWarpEffect : MonoBehaviour
{

    public Material warpMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, warpMaterial);
    }
}
