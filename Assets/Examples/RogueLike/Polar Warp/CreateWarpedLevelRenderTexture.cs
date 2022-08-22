namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class CreateWarpedLevelRenderTexture : MonoBehaviour
    {
        public Map map;
        public MapRenderer render;
        void Awake()
        {
            var camera = GetComponent<Camera>();
            camera.depthTextureMode = DepthTextureMode.Depth;

            // Times 2 for the double wide rendering that makes wrapping work
            float renderedWidth = map.TotalWidth * 2;

            // If the width is not exactly this value then we will not get pixel perfect rendering to the render texture
            int renderTextureWidth = (int)(renderedWidth * 128);
            // The height was chosen via experimentation such that the tiles appear squarish after circle warping
            int renderTextureHeight = (int)(renderTextureWidth / 4.5f);

            // Create the render texture. May want to switch to HDR format here if we ever need it for effects. It does double an already very large texture size though.
            var renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0, RenderTextureFormat.ARGB32);
            //renderTexture.filterMode = FilterMode.Point;

            // The depth texture is needed for the outline effect in the warp shader. It does not need to be very accurate though.
            var depthTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.Depth);

            // Tell the camera to render to the render and depth texture
            camera.SetTargetBuffers(renderTexture.colorBuffer, depthTexture.depthBuffer);

            // Tell the render quad material to use the render and depth texture
            render.warpMaterial.mainTexture = renderTexture;
            render.warpMaterial.SetTexture("_Depth", depthTexture);

            // Set the camera size so that the width is 2 times the map width so wrapping magic works.
            camera.orthographicSize = (renderedWidth / camera.aspect) / 2.0f;

            // Now that the camera is set up we can place the render quad
            MapRenderer.instance.PlaceQuad();
        }
    }
}