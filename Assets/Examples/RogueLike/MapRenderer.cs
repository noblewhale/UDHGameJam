namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class MapRenderer : MonoBehaviour
    {
        public float vertical;
        public float horizontal;

        public static MapRenderer instance;
        public Material warpMaterial;

        void Awake()
        {
            instance = this;
            warpMaterial = GetComponent<MeshRenderer>().material;
        }

        public void PlaceQuad()
        {
            // The main camera sees the warped map quad and the UI. It renders to the screen.
            Camera mainCamera = Camera.main;
            float mainCameraHeight = mainCamera.orthographicSize * 2;
            float mainCameraWidth = mainCamera.orthographicSize * 2 * mainCamera.aspect;

            // It is important that we force the quad's width and height to be integer multiples of the screen's pixel size.
            // This, along with careful positioning, ensures that the quad will allign with the actual screen pixels and prevent unwanted aliasing
            Vector2 pixelSize = new Vector2(mainCameraWidth / Screen.width, mainCameraHeight / Screen.height);

            Vector2 desiredSize = Vector2.one;

            // The render quad should be a bit larger than the height of the screen so the top is cut off, but not more than 80% of the width to leave room for the GUI stuff.
            // If we weren't circle warping we would want to limit this to values that cause texels in the quad to line up with pixels in the render texture to avoid unwanted aliasing.
            // Unortunately with the circle warp it's not possible because the render texture is not sampled in a uniform way.
            // I've experimented with forcing the width to be such that the half radius of the circle has a circumference that is the
            // width of the render texture (so that the area where the player is at least aligns) but the results were not compelling.
            // The quad was not able to be a reasonable size at some common resolutions and it didn't seem to help the rendering much anyway.
            desiredSize.x = Mathf.Min(mainCameraHeight * 1.525f, mainCameraWidth * .8f);

            // Force the width to line up with an integer number of pixels.
            // This may seem pointless given the issues above but it actually does appear to significantly reduce unwanted aliasing
            desiredSize.x = pixelSize.x * (int)(desiredSize.x / pixelSize.x);

            // The quad should be a square since it's rendering a circle, but the pixels may not be square so adjust slightly to line up.
            // This is never more than one pixel height less than the width so the quad still appears square (and the circle circular)
            desiredSize.y = pixelSize.y * (int)(desiredSize.x / pixelSize.y);

            // Position the quad relative to one of the corners.
            float x, y;
            if (horizontal < 0)
            {
                x = (-desiredSize.x + mainCameraWidth) / 2 + horizontal;
            }
            else
            {
                x = (desiredSize.x - mainCameraWidth) / 2 + horizontal;
            }
            if (vertical < 0)
            {
                y = (-desiredSize.y + mainCameraHeight) / 2 + vertical;
            }
            else
            {
                y = (desiredSize.y - mainCameraHeight) / 2 + vertical;
            }

            // Adjust the location to align with screen pixels
            // I originally tried doing this by simply lining up the center with the center or edge of a pixel. This is temptingly simple so of course it's totally wrong.
            // The correct way is to find a corner of the quad and line that up with a pixel, then add half the size back to find the center position.
            Vector2 desiredLocation = new Vector2(x, y);
            Vector2 bottomLeft = desiredLocation - desiredSize / 2;
            bottomLeft.x = pixelSize.x * (int)(bottomLeft.x / pixelSize.x);
            bottomLeft.y = pixelSize.y * (int)(bottomLeft.y / pixelSize.y);
            desiredLocation = bottomLeft + desiredSize / 2;

            // My will be done, on Earth as it is in Hades. As above. So Below.
            transform.localScale = new Vector3(desiredSize.x, desiredSize.y, 1);
            transform.localPosition = new Vector3(desiredLocation.x, desiredLocation.y, transform.localPosition.z);
        }
    }
}