namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class CreateRenderTextureAtResolution : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            RenderTexture foregroundTexture = new RenderTexture(Screen.width, Screen.height, 16);
            GetComponent<Camera>().targetTexture = foregroundTexture;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}