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

            // We want tiles to be the most square in the middle
            // So the spect ratio needs to be the same as the ratio of the height to the circumference (PI)
            // The inner radius also needs to be accounted for
            //float innerRadius = renderQuad.sharedMaterial.GetFloat("_InnerRadius");
            //float aspect = Mathf.PI * (1 - innerRadius*2);
            // That all goes out the window with the distance magic that happens though so instead we use a nice fudge number picked via experimentation
            float aspect = 2.25f;
            // Times 2 for wrapping magic
            int width = (int)(height * aspect) * 2;
            width = 4 * (width / 4);
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