namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using UnityEngine;

    public class CameraTarget : DungeonObject
    {
        public DungeonObject owner;

        public int thresholdX = 0;
        public int thresholdY = 0;

        public static CameraTarget instance;

        override protected void Awake()
        {
            base.Awake();
            instance = this;
        }

        void Update()
        {
            if (owner == null) return;

            int newX = x;
            int newY = y;

            Vector2Int cameraTile = PlayerCamera.instance.GetTilePosition();

            Vector2Int circleDifference = Map.instance.GetDifference(new Vector2Int(x, y), new Vector2Int(owner.x, owner.y));

            if (Math.Abs(circleDifference.y) > thresholdY)
            {
                if (owner.y > y)
                {
                    newY = owner.y - thresholdY;
                }
                else
                {
                    newY = owner.y + thresholdY;
                }
            }
            newY = Math.Clamp(newY, cameraTile.y - 1, cameraTile.y + 1);

            if (Math.Abs(circleDifference.y) <= thresholdY || thresholdX == 0)
            {
                if (Math.Abs(circleDifference.x) > thresholdX)
                {
                    if (circleDifference.x > 0)
                    {
                        newX = (x + circleDifference.x) - thresholdX;
                    }
                    else
                    {
                        newX = (x + circleDifference.x) + thresholdX;
                    }
                }

                newX = Map.instance.GetXPositionOnMap(newX);
                int cameraCircleDifference = Map.instance.GetXDifference(cameraTile.x, newX);
                cameraCircleDifference = Math.Clamp(cameraCircleDifference, -1, 1);
                newX = cameraTile.x + cameraCircleDifference;
                newX = Map.instance.GetXPositionOnMap(newX);
            }

            if ((newX != x || newY != y) && newX >= 0 && newX < Map.instance.width && newY >= 0 && newY < Map.instance.height)
            {
                Map.instance.MoveObject(this, newX, newY);
            }
        }

        public void UpdatePosition()
        {
            Map.instance.MoveObject(this, owner.x, owner.y);
        }
    }
}