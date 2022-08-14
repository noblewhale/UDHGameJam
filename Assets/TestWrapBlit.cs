using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWrapBlit : MonoBehaviour
{
    public Material wrapMaterial;
    RenderTexture renderTexture;

    private void Awake()
    {
        renderTexture = new RenderTexture(Screen.width * 3, Screen.height, 16); 
        Camera.main.targetTexture = renderTexture;
        Camera.main.cullingMatrix = Matrix4x4.Ortho(-Screen.width*3/2, Screen.width*3/2, -Screen.height/2, Screen.height/2, Camera.main.nearClipPlane, Camera.main.farClipPlane) *
                             Matrix4x4.Translate(Camera.main.transform.position) *
                             Camera.main.worldToCameraMatrix;
    }

    void OnPreRender()
    {
        Camera.main.targetTexture = renderTexture;
    }

    void OnPostRender()
    {
        Camera.main.targetTexture = null;
        Graphics.Blit(renderTexture, null, wrapMaterial);
    }
}
