namespace Noble.TileEngine
{
    using UnityEngine;

    public static class CameraUtil
    {
        static Rect RectOne = new Rect(0, 0, 1, 1);

        public static bool Contains(this Camera camera, Vector3 point)
        {
            Vector3 viewportPos = camera.WorldToViewportPoint(point);
            return RectOne.Contains(viewportPos);
        }
    }
}
