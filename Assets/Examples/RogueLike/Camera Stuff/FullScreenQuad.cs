namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class FullScreenQuad : MonoBehaviour
    {
        public string sortingLayerName = string.Empty; //initialization before the methods
        public int orderInLayer = 0;
        public Renderer MyRenderer;
        
        void Start()
        {
            if (sortingLayerName != string.Empty)
            {
                MyRenderer.sortingLayerName = sortingLayerName;
                MyRenderer.sortingOrder = orderInLayer;
            }
        }

        void Update()
        {
            transform.localScale = new Vector3(Camera.main.orthographicSize * Camera.main.aspect * 2, Camera.main.orthographicSize * 2, 1);
        }
    }
}
