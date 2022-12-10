namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class MapRendererLarge : MapRenderer
    {
        public float vertical;
        public float horizontal;

        override public void PlaceQuad()
        {
            Camera mainCamera = Camera.main;
            // This mess solves a quadratic equation to determine how large the quad needs to be so that the rendered area fills the main camera view area
            // From pythagoras:
            //  (p*R + h)^2 + (w/2)^2 = R^2
            // where:
            //  p = ratio of rendered map height to warped map height
            //  R = total radius
            //  h = height of view
            //  w = width of view
            float mapCameraHeight = PlayerCamera.instance.camera.orthographicSize * 2;
            float warpedMapSectionHeight = Map.instance.TotalWidth / (2 * Mathf.PI);
            float p = 1 - mapCameraHeight / warpedMapSectionHeight;

            float w = mainCamera.orthographicSize * 2 * mainCamera.aspect;
            float h = mainCamera.orthographicSize * 2;

            // Use quadratic formula to solve for R
            // (p^2-1)*R^2 + 2*p*h*R + (h^2 + (w/2)^2) = 0
            float A = p * p - 1;
            float B = 2 * p * h;
            float C = h * h + Mathf.Pow(w / 2, 2);
            // Thank you high school algebra
            float R = (-B - Mathf.Sqrt(B * B - 4 * A * C)) / (2 * A);

            // Actual scale of quad is twice the radius
            transform.localScale = new Vector3(R * 2, R * 2, 1);

            // Positioning is thankfully a bit simpler, the inner radius plus half the view height
            transform.localPosition = new Vector3(0, R * p + h / 2, transform.localPosition.z);
        }
    }
}