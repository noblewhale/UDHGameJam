﻿namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class CreateWarpedLevelRenderTexture : MonoBehaviour
    {
        public MeshRenderer renderQuad;
        void Awake()
        {
            var camera = GetComponent<Camera>();

            // Attempt to calculate the optimum size for the render texture based on screen resolution
            // and the properties of the warp effect that is used to render the map

            // We only need to render at half height because the warp squashes twice the height into half the space
            int height = Screen.height;
            // Extra size comes from the render quad being scaled up a bit from full screen size. 
            // This is to zoom in on the character more while cutting off the top that doesn't need to be seen.
            float extraSize = renderQuad.transform.localScale.y / (camera.orthographicSize * 2);
            height = (int)(height * extraSize);
            height = (int)(Mathf.Pow(2, Mathf.Ceil(Mathf.Log(height) / Mathf.Log(2))));
            // Width is double the height because it seems right..
            int width = (int)(height * 2.25f);
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            renderTexture.filterMode = FilterMode.Point;
            camera.targetTexture = renderTexture;
            renderQuad.sharedMaterial.mainTexture = renderTexture;
        }
    }
}