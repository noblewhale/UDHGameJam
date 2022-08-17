namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class CreateWarpedLevelRenderTexture : MonoBehaviour
    {
        public MeshRenderer renderQuad;
        void Awake()
        {
            var camera = GetComponent<Camera>();
            camera.depthTextureMode = DepthTextureMode.Depth;

            // Attempt to calculate the optimum size for the render texture based on screen resolution
            // and the properties of the warp effect that is used to render the map

            // The height controls the level of detail. Using the screen's height seems to give good results.
            int height = Screen.height;

            // The 2.25 is a fudge to get the tiles looking squarish...the 3 is from the extra wide camera for wrapping magic
            int width = (int)(height * 2.25f) * 2;
            var renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            var depthTexture = new RenderTexture(width, height, 24, RenderTextureFormat.Depth);
            renderTexture.filterMode = FilterMode.Point;
            camera.targetTexture = renderTexture;
            camera.SetTargetBuffers(renderTexture.colorBuffer, depthTexture.depthBuffer);
            renderQuad.sharedMaterial.mainTexture = renderTexture;
            renderQuad.sharedMaterial.SetTexture("_Depth", depthTexture);
        }
    }
}