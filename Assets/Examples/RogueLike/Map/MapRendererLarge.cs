namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    public class MapRendererLarge : MapRenderer
    {
        public float vertical;
        public float horizontal;
        public Camera wrapCamera;
        public float scale = 1;

        public float seaLevel = 1;
        public float innerRadius = .1f;
        public float fadeSize = .02f;
        public float fadeExponent = 2f;

        override public void PlaceQuad()
        {
            Camera mainCamera = Camera.main;

            warpMaterial.SetFloat("_SeaLevel", seaLevel);
            warpMaterial.SetFloat("_InnerRadius", innerRadius);
            warpMaterial.SetFloat("_InnerFadeSize", fadeSize);
            warpMaterial.SetFloat("_InnerFadeExp", fadeExponent);


            float _SeaLevel = warpMaterial.GetFloat("_SeaLevel");
            float _InnerRadius = warpMaterial.GetFloat("_InnerRadius");
            float warpedMapSectionHeight = ((PlayerCameraCircleWarpLarge)PlayerCamera.instance).GetThatWierdThing();
            float mapCameraHeight = PlayerCamera.instance.camera.orthographicSize * 2;

            float p = 1 - mapCameraHeight / warpedMapSectionHeight;

            float pos1 = p + (mapCameraHeight / warpedMapSectionHeight) / 2 - .5f / warpedMapSectionHeight + PlayerCamera.instance.cameraOffset / warpedMapSectionHeight;
            float pos2 = p + (mapCameraHeight / warpedMapSectionHeight) / 2 + .5f / warpedMapSectionHeight + PlayerCamera.instance.cameraOffset / warpedMapSectionHeight;

            float d1 = pos1;
            d1 = (Mathf.Pow(1 + _SeaLevel, d1) - 1) / _SeaLevel;
            d1 = d1 * (1 - _InnerRadius) + _InnerRadius;
            d1 /= 2;

            float d2 = pos2;
            d2 = (Mathf.Pow(1 + _SeaLevel, d2) - 1) / _SeaLevel;
            d2 = d2 * (1 - _InnerRadius) + _InnerRadius;
            d2 /= 2;

            float scale = 1 / (d2 - d1);
            scale *= this.scale;

            float pos3 = p + (mapCameraHeight / warpedMapSectionHeight) / 2 + PlayerCamera.instance.cameraOffset / warpedMapSectionHeight;

            float d3 = pos3;
            d3 = (Mathf.Pow(1 + _SeaLevel, d3) - 1) / _SeaLevel;
            d3 = d3 * (1 - _InnerRadius) + _InnerRadius;
            d3 /= 2;

            if (float.IsNormal(scale))
            {
                transform.localScale = new Vector3(scale, scale, 1);
                transform.localPosition = new Vector3(0, d3 * scale, transform.localPosition.z);
            }
        }

        int oldRenderTextureWidth;
        int oldRenderTextureHeight;

        public override void CreateRenderTexture()
        {
            var camera = PlayerCamera.instance.camera;
            camera.depthTextureMode = DepthTextureMode.Depth;

            renderedWidth = Mathf.Min(66, Map.instance.TotalWidth);

            //renderedHeight = 17;

            // If the width is not exactly this value then we will not get pixel perfect rendering to the render texture
            int renderTextureWidth = (int)(renderedWidth * 64);
            int renderTextureHeight = (int)(renderedHeight * 64);

            if (renderTextureWidth != oldRenderTextureWidth || renderTextureHeight != oldRenderTextureHeight)
            {
                oldRenderTextureWidth = renderTextureWidth;
                oldRenderTextureHeight = renderTextureHeight;

                // Create the render texture. May want to switch to HDR format here if we ever need it for effects. It does double an already very large texture size though.
                var renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0, RenderTextureFormat.ARGB32);
                renderTexture.filterMode = FilterMode.Point;
                renderTexture.useMipMap = false;

                // The depth texture is needed for the outline effect in the warp shader. It does not need to be very accurate though.
                var depthTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.Depth);

                // Tell the camera to render to the render and depth texture
                camera.SetTargetBuffers(renderTexture.colorBuffer, depthTexture.depthBuffer);
                camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.ConfigureCameraTarget(renderTexture.colorBuffer, depthTexture.depthBuffer);
                camera.targetTexture = renderTexture;

                // Tell the render quad material to use the render and depth texture
                //warpMaterial.mainTexture = renderTexture;
                warpMaterial.SetTexture("_MainTex", renderTexture);
                warpMaterial.SetTexture("_Depth", depthTexture);

                // Set the camera size so that the width is the map width.
                camera.orthographicSize = renderedHeight / 2.0f;

                ////

                // Create second camera and render texture for wrapping
                if (wrapCamera == null)
                {
                    GameObject wrapCameraOb = Instantiate(PlayerCamera.instance.camera.gameObject);
                    Destroy(wrapCameraOb.GetComponent<PlayerCamera>());
                    wrapCamera = wrapCameraOb.GetComponent<Camera>();
                    wrapCameraOb.transform.parent = PlayerCamera.instance.transform;
                    wrapCameraOb.AddComponent<WrapCamera>();
                }

                // Create the render texture. May want to switch to HDR format here if we ever need it for effects. It does double an already very large texture size though.
                renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0, RenderTextureFormat.ARGB32);
                renderTexture.filterMode = FilterMode.Point;
                renderTexture.useMipMap = false;

                // The depth texture is needed for the outline effect in the warp shader. It does not need to be very accurate though.
                depthTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.Depth);

                // Tell the camera to render to the render and depth texture
                wrapCamera.SetTargetBuffers(renderTexture.colorBuffer, depthTexture.depthBuffer);
                wrapCamera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.ConfigureCameraTarget(renderTexture.colorBuffer, depthTexture.depthBuffer);
                wrapCamera.targetTexture = renderTexture;

                // Tell the render quad material to use the render and depth texture
                warpMaterial.SetTexture("_WrapTexture", renderTexture);
                warpMaterial.SetTexture("_WrapDepth", depthTexture);
            }
        }
    }
}