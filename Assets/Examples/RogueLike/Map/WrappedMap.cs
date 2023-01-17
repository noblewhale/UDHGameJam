namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using UnityEngine;

    public class WrappedMap : Map 
    {
        override public void Awake()
        {
            base.Awake();
            instance = this;
        }

        override public void Start()
        {
            base.Start();
            this.OnMapLoaded += this.SetupWrapCameras;
        }

        void SetupWrapCameras()
        {
            var cams = FindObjectsOfType<WrapCameraAlt>();
            foreach (var cam in cams)
            {
                cam.SetWrapWidth(totalArea.width, totalArea.center.x);
            }
        }

        override public float GetXDifference(float start, float end)
        {
            float circleDifference1 = end - start;
            float circleDifference2;
            if (start < totalArea.center.x)
            {
                circleDifference2 = (end - totalArea.width) - start;
            }
            else
            {
                circleDifference2 = (end + totalArea.width) - start;
            }

            return Math.Abs(circleDifference1) < Math.Abs(circleDifference2) ? circleDifference1 : circleDifference2;
        }

        override public int GetXDifference(int start, int end)
        {
            int circleDifference1 = end - start;
            int circleDifference2;
            if (start < width / 2)
            {
                circleDifference2 = (end - width) - start;
            }
            else
            {
                circleDifference2 = (end + width) - start;
            }

            return Math.Abs(circleDifference1) < Math.Abs(circleDifference2) ? circleDifference1 : circleDifference2;
        }

        override public float GetXWorldPositionOnMap(float x)
        {
            if (x < totalArea.xMin) x += totalArea.width;
            else if (x > totalArea.xMax) x -= totalArea.width;
            return x;
        }

        override public int GetXTilePositionOnMap(int x)
        {
            if (x < 0) x += width;
            else if (x >= width) x -= width;
            return x;
        }
    }
}
