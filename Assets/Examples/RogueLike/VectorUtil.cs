
namespace Noble.TileEngine
{
    using UnityEngine;

    public class VectorUtil
    {
        public static readonly Vector3 UpLeft;
        public static readonly Vector3 UpRight;
        public static readonly Vector3 DownLeft;
        public static readonly Vector3 DownRight;

        static VectorUtil()
        {
            UpLeft = (Vector3.up + Vector3.left).normalized;
            UpRight = (Vector3.up + Vector3.right).normalized;
            DownLeft = (Vector3.down + Vector3.left).normalized;
            DownRight = (Vector3.down + Vector3.right).normalized;
        }

    }
}
