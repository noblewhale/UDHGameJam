namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

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

        public float heightToWarp = 26;
        public float widthToWarp => Map.instance.TotalWidth;
        public Vector2 areaToWarp => new Vector2(widthToWarp, heightToWarp);

        override public void PlaceQuad()
        {
            Camera mainCamera = Camera.main;

            warpMaterial.SetFloat("_SeaLevel", seaLevel);
            warpMaterial.SetFloat("_InnerRadius", innerRadius);
            warpMaterial.SetFloat("_InnerFadeSize", fadeSize);
            warpMaterial.SetFloat("_InnerFadeExp", fadeExponent);


            float _SeaLevel = warpMaterial.GetFloat("_SeaLevel");
            float _InnerRadius = warpMaterial.GetFloat("_InnerRadius");
            float mapCameraHeight = PlayerCamera.instance.camera.orthographicSize * 2;

            float p = 1 - mapCameraHeight / heightToWarp;

            float pos1 = p + (mapCameraHeight / heightToWarp) / 2 - .5f / heightToWarp + PlayerCamera.instance.cameraOffset / heightToWarp;
            float pos2 = p + (mapCameraHeight / heightToWarp) / 2 + .5f / heightToWarp + PlayerCamera.instance.cameraOffset / heightToWarp;

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

            float pos3 = p + (mapCameraHeight / heightToWarp) / 2 + PlayerCamera.instance.cameraOffset / heightToWarp;

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

            renderedWidth = Mathf.Min(renderedWidth, Map.instance.TotalWidth);
            renderedHeight = Mathf.Min(renderedHeight, Map.instance.TotalHeight);

            // If the width is not exactly this value then we will not get pixel perfect rendering to the render texture
            int renderTextureWidth = (int)(renderedWidth * 64);
            int renderTextureHeight = (int)(renderedHeight * 64);

            if (renderTextureWidth != oldRenderTextureWidth || renderTextureHeight != oldRenderTextureHeight)
            {
                oldRenderTextureWidth = renderTextureWidth;
                oldRenderTextureHeight = renderTextureHeight;

                // Create the render texture. May want to switch to HDR format here if we ever need it for effects. It does double an already very large texture size though.
                var renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0);
                renderTexture.filterMode = FilterMode.Point;
                renderTexture.useMipMap = false;
                renderTexture.depth = 0;

                // The depth texture is needed for the outline effect in the warp shader. It does not need to be very accurate though.
                var depthTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.Depth);

                // Tell the camera to render to the render and depth texture
                camera.SetTargetBuffers(renderTexture.colorBuffer, depthTexture.depthBuffer);
                //camera.GetComponent<UniversalAdditionalCameraData>().requiresDepthTexture = true;
                //var renderHandleColor = RTHandles.Alloc(renderTexture.colorBuffer);
                //var renderHandleDepth = RTHandles.Alloc(renderTexture.depthBuffer);
                //camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.ConfigureCameraTarget(renderHandleColor, renderHandleDepth);
                camera.forceIntoRenderTexture = true;
                //camera.targetTexture = renderTexture;

                // Tell the render quad material to use the render and depth texture
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
                renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0);
                renderTexture.filterMode = FilterMode.Point;
                renderTexture.useMipMap = false;
                renderTexture.depth = 0;

                // The depth texture is needed for the outline effect in the warp shader. It does not need to be very accurate though.
                depthTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.Depth);

                // Tell the camera to render to the render and depth texture
                wrapCamera.SetTargetBuffers(renderTexture.colorBuffer, depthTexture.depthBuffer);
                //wrapCamera.GetComponent<UniversalAdditionalCameraData>().requiresDepthTexture = true;
                //renderHandleColor = RTHandles.Alloc(renderTexture.colorBuffer);
                //renderHandleDepth = RTHandles.Alloc(renderTexture.depthBuffer);
                //wrapCamera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.ConfigureCameraTarget(renderHandleColor, renderHandleDepth);
                wrapCamera.forceIntoRenderTexture = true;
                //wrapCamera.targetTexture = renderTexture;

                // Tell the render quad material to use the render and depth texture
                warpMaterial.SetTexture("_WrapTexture", renderTexture);
                warpMaterial.SetTexture("_WrapDepth", depthTexture);
            }
        }

        override public void Update()
        {
            base.Update();
            UpdateCameraShaderProperties();
        }

        // Update _CameraPos and _CameraDim for the CircleWarp shader
        // The positions and dimensions are relative to the map and normalized to the warpedArea
        public void UpdateCameraShaderProperties()
        {
            // _CameraPos needs to be the bottom left corner of area viewed by the camera
            // First get the center relative to the map 
            Vector2 cameraCenterRelativeToMap = PlayerCamera.instance.transform.position - Map.instance.transform.position;
            // Find the corner, still relative to the map
            Vector2 cameraCornerRelativeToMap = cameraCenterRelativeToMap - PlayerCamera.instance.camera.GetSize() / 2;
            // Now normalized to the warped area
            Vector2 cameraCornerInNormalizedMapCoords = cameraCornerRelativeToMap / areaToWarp;
            // And send it off to the shader
            warpMaterial.SetVector("_CameraPos", cameraCornerInNormalizedMapCoords);

            // _CameraDim is the camera dimensions normalized to the warped area
            Vector2 cameraDimensionsInNormalizedMapCoords = PlayerCamera.instance.camera.GetSize() / areaToWarp;
            // And send it off to the shader
            warpMaterial.SetVector("_CameraDim", cameraDimensionsInNormalizedMapCoords);
        }
    }
}