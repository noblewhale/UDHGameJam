namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class FullScreenQuad : MonoBehaviour
    {
        void Update()
        {
            transform.localScale = new Vector3(Camera.main.orthographicSize * Camera.main.aspect * 2, Camera.main.orthographicSize * 2, 1);
        }
    }
}
