namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class MapRenderer : MonoBehaviour
    {
        public float vertical;
        public float horizontal;

        public static MapRenderer instance;
        public Material material;

        void Awake()
        {
            instance = this;

            material = GetComponent<MeshRenderer>().sharedMaterial;

            float maxWidth = Mathf.Min(Camera.main.orthographicSize * 2 * 1.525f, Camera.main.orthographicSize * 2 * Camera.main.aspect * .8f);
            transform.localScale = new Vector3(maxWidth, maxWidth, 1);
            float x, y;
            if (horizontal < 0)
            {
                x = -transform.localScale.x / 2 + Camera.main.orthographicSize * Camera.main.aspect + horizontal;
            }
            else
            {
                x = transform.localScale.x / 2 - Camera.main.orthographicSize * Camera.main.aspect + horizontal;
            }
            if (vertical < 0)
            {
                y = -transform.localScale.y / 2 + Camera.main.orthographicSize + vertical;
            }
            else
            {
                y = transform.localScale.y / 2 - Camera.main.orthographicSize + vertical;
            }
            transform.localPosition = new Vector3(x, y, transform.localPosition.z);
        }
    }
}