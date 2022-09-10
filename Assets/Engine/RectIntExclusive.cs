namespace Noble.TileEngine
{
    using System;
    using UnityEngine;

    // Like a RectInt but xMax and yMax do not include width and height
    // So a Rect at 0, 0 with width 1 and height 1 will have xMax and yMax of 0
    // This is designed to work more like arrays which have a max index one less than the array length
    [Serializable]
    public struct RectIntExclusive : IEquatable<RectIntExclusive>
    {
        public int xMin, yMin;
        public int xMax, yMax;

        internal void SetToSquare(Vector2Int center, float radius)
        {
            SetMinMax(
                Mathf.FloorToInt(center.x - radius),
                Mathf.FloorToInt(center.x + radius),
                Mathf.FloorToInt(center.y - radius),
                Mathf.FloorToInt(center.y + radius)
            );
        }

        public int width => xMax - xMin + 1;

        public int height => yMax - yMin + 1;

        public RectIntExclusive(RectIntExclusive copy)
        {
            xMin = copy.xMin;
            yMin = copy.yMin;
            xMax = copy.xMax;
            yMax = copy.yMax;
        }

        public RectIntExclusive(int x, int y, int w, int h)
        {
            xMin = x;
            yMin = y;
            xMax = xMin + w - 1;
            yMax = yMin + h - 1;
        }

        public void SetMinMax(int xMin, int xMax, int yMin, int yMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }

        public int Min(bool horizontal)
        {
            if (horizontal) return this.xMin;
            else return this.yMin;
        }

        public int Max(bool horizontal)
        {
            if (horizontal) return this.xMax;
            else return this.yMax;
        }

        public bool Contains(Vector2Int pos)
        {
            return Contains(pos.x, pos.y);
        }

        public bool Contains(int x, int y)
        {
            return x >= xMin && x <= xMax && y >= yMin && y <= yMax;
        }

        public override string ToString()
        {
            return xMin + " " + xMax + " " + yMin + " " + yMax;
        }

        public bool IsZero()
        {
            return xMin == 0 && xMax == 0 && yMin == 0 && yMax == 0;
        }

        public bool Equals(RectIntExclusive other)
        {
            return this.xMin == other.xMin && this.xMax == other.xMax && this.yMin == other.yMin && this.yMax == other.yMax;
        }
    }
}