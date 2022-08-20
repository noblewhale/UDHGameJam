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
            Vector2 pixelSize = new Vector2(Camera.main.orthographicSize * 2 * Camera.main.aspect / Screen.width, Camera.main.orthographicSize * 2 / Screen.height);
            var camera = GetComponent<Camera>();
            camera.depthTextureMode = DepthTextureMode.Depth;

            // If the width is not exactly this value then you will not get pixel perfect rendering to the render texture
            int renderTextureWidth = (int)(map.TotalWidth * 128 * 2);
            int renderTextureHeight = (int)(renderTextureWidth / 4.5f);

            var renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0, RenderTextureFormat.ARGB32, 0);
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.useMipMap = false;
            renderTexture.antiAliasing = 1;

            var depthTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 24, RenderTextureFormat.Depth);

            camera.targetTexture = renderTexture;
            camera.SetTargetBuffers(renderTexture.colorBuffer, depthTexture.depthBuffer);

            render.warpMaterial.mainTexture = renderTexture;
            render.warpMaterial.SetTexture("_Depth", depthTexture);

            // Set the camera size so that the width is 2 times the map width so wrapping magic works.
            camera.orthographicSize = (2 * map.TotalWidth / camera.aspect) / 2.0f;

            float renderQuadHeight;
            float renderQuadWidthMax = Mathf.Min(Camera.main.orthographicSize * 2 * 1.525f, Camera.main.orthographicSize * 2 * Camera.main.aspect * .8f);
            float renderQuadWidthMin = Mathf.Min(Camera.main.orthographicSize * 2 * 1.45f, Camera.main.orthographicSize * 2 * Camera.main.aspect * .8f);
            float w = 2 * camera.orthographicSize * 2 * camera.aspect / Mathf.PI;
            int i = 2;
            while (w > renderQuadWidthMax)
            {
                w = (2 * camera.orthographicSize * 2 * camera.aspect / Mathf.PI) / i;
                i++;
            }
            float renderQuadWidth = Mathf.Max(w, renderQuadWidthMin);
            //renderQuadWidth = (2 * camera.orthographicSize * 2 * camera.aspect / Mathf.PI) / (i - 2);
            renderQuadWidth = pixelSize.x * (int)(renderQuadWidth / pixelSize.x);
            renderQuadHeight = pixelSize.y * (int)(renderQuadWidth / pixelSize.y);
            MapRenderer.instance.transform.localScale = new Vector3(renderQuadWidth, renderQuadHeight, 1);

            float x, y;
            if (MapRenderer.instance.horizontal < 0)
            {
                x = -MapRenderer.instance.transform.localScale.x / 2 + Camera.main.orthographicSize * Camera.main.aspect + MapRenderer.instance.horizontal;
            }
            else
            {
                x = MapRenderer.instance.transform.localScale.x / 2 - Camera.main.orthographicSize * Camera.main.aspect + MapRenderer.instance.horizontal;
            }
            if (MapRenderer.instance.vertical < 0)
            {
                y = -MapRenderer.instance.transform.localScale.y / 2 + Camera.main.orthographicSize + MapRenderer.instance.vertical;
            }
            else
            {
                y = MapRenderer.instance.transform.localScale.y / 2 - Camera.main.orthographicSize + MapRenderer.instance.vertical;
            }

            Vector3 desiredLocation = new Vector3(x, y, MapRenderer.instance.transform.localPosition.z);
            Vector2 bottomLeft = desiredLocation - MapRenderer.instance.transform.localScale / 2;
            bottomLeft.x = pixelSize.x * (int)(bottomLeft.x / pixelSize.x);
            bottomLeft.y = pixelSize.y * (int)(bottomLeft.y / pixelSize.y);
            desiredLocation = bottomLeft + (Vector2)MapRenderer.instance.transform.localScale / 2;
            desiredLocation.z = MapRenderer.instance.transform.localPosition.z;
            MapRenderer.instance.transform.localPosition = desiredLocation;
        }
    }
}