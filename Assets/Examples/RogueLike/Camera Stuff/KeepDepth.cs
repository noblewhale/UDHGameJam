using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class KeepDepth : MonoBehaviour
{
    CommandBuffer keepDepthTexture;
    RenderTexture lastDepth;
    float oldScreenWidth, oldScreenHeight;
    public PostProcessVolume wrapEffectVolume;

    private void Awake()
    {
        lastDepth = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        oldScreenHeight = Screen.height;
        oldScreenWidth = Screen.width;
    }

    void OnScreenResize()
    {
        // Recreate the render texture at the proper 
        lastDepth.Release();
        lastDepth.width = Screen.width;
        lastDepth.height = Screen.height;
        lastDepth.Create();

        // Keep track of dimensions so we know when they change
        oldScreenHeight = Screen.height;
        oldScreenWidth = Screen.width;
    }

    void OnEnable()
    {
        if (keepDepthTexture == null)
        {
            Camera cam = GetComponent<Camera>();
            keepDepthTexture = new CommandBuffer();
            keepDepthTexture.name = "Keep MainCamera Depth Texture";
            keepDepthTexture.SetGlobalTexture("_MainCameraDepthTexture", lastDepth);
            keepDepthTexture.CopyTexture(BuiltinRenderTextureType.Depth, lastDepth);
            cam.AddCommandBuffer(CameraEvent.AfterDepthTexture, keepDepthTexture);
        }
    }

    void Update()
    {
        if (Screen.height != oldScreenHeight || Screen.width != oldScreenWidth)
        {
            OnScreenResize();
        }
    }

}
