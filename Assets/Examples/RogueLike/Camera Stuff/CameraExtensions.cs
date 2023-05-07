namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public static class CameraExtensions
    {
        public static Vector2 GetSize(this Camera camera)
        {
            return new Vector2(camera.orthographicSize * 2 * camera.aspect, camera.orthographicSize * 2);
        }
    }
}
