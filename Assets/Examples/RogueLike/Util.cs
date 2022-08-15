
namespace Noble.TileEngine
{
    using System;
    using UnityEngine;

    public static class Util
    {
        public static readonly Vector3 UpLeft;
        public static readonly Vector3 UpRight;
        public static readonly Vector3 DownLeft;
        public static readonly Vector3 DownRight;

        static Util()
        {
            UpLeft = (Vector2.up + Vector2.left).normalized;
            UpRight = (Vector2.up + Vector2.right).normalized;
            DownLeft = (Vector2.down + Vector2.left).normalized;
            DownRight = (Vector2.down + Vector2.right).normalized;
        }

        public static int GetCircleDifference(int posA, int posB)
        {
            int circleDifference1 = posB - posA;
            int circleDifference2;
            if (posA < Map.instance.width / 2)
            {
                circleDifference2 = (posB - Map.instance.width) - posA;
            }
            else
            {
                circleDifference2 = (posB + Map.instance.width) - posA;
            }

            return Math.Abs(circleDifference1) < Math.Abs(circleDifference2) ? circleDifference1 : circleDifference2;
        }

    }
}
