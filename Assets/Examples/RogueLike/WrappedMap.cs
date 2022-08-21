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
        }

        override public float GetXDifference(float start, float end)
        {
            float circleDifference1 = end - start;
            float circleDifference2;
            if (start < TotalWidth / 2)
            {
                circleDifference2 = (end - TotalWidth) - start;
            }
            else
            {
                circleDifference2 = (end + TotalWidth) - start;
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

        override public float GetXPositionOnMap(float x)
        {
            if (x < 0) x += TotalWidth;
            else if (x >= TotalWidth) x -= TotalWidth;
            return x;
        }

        override public int GetXPositionOnMap(int x)
        {
            if (x < 0) x += width;
            else if (x >= width) x -= width;
            return x;
        }
    }
}
