
namespace Noble.TileEngine
{
    using System;
    using System.Collections.Generic;
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

    }
}
