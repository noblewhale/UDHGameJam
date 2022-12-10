namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera instance;

        new public Camera camera;

        public float cameraOffset = 3;

        virtual public void Awake()
        {
            instance = this;
            camera = GetComponent<Camera>();
        }

        virtual public Vector2Int GetTilePosition()
        {
            return Map.instance.GetTileFromWorldPosition(transform.position - Map.instance.transform.position).tilePosition;
        }
    }
}