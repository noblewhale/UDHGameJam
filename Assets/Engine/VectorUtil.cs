
namespace Noble.TileEngine
{
    using UnityEngine;

    public static class VectorUtil
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

        public static Vector2Int ToFloor(this Vector2 floatVector)
        {
            return new Vector2Int(Mathf.FloorToInt(floatVector.x), Mathf.FloorToInt(floatVector.y));
        }

        public static void Clamp(ref this Vector2 floatVector, Vector2 minValues, Vector2 maxValues)
        {
            floatVector.x = Mathf.Clamp(floatVector.x, minValues.x, maxValues.x);
            floatVector.y = Mathf.Clamp(floatVector.y, minValues.y, maxValues.y);
        }

    }
}
