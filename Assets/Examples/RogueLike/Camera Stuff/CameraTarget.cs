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
            if (owner == null || tile == null) return;
            if (!PlayerCamera.instance) return;

            Vector2Int newPos = tilePosition;

            Vector2Int cameraTile = PlayerCamera.instance.GetTilePosition();
            cameraTile.y -= (int)PlayerCamera.instance.cameraOffset;
            Vector2Int circleDifference = Map.instance.GetDifference(tilePosition, owner.tilePosition);
            if (Math.Abs(circleDifference.y) > thresholdY)
            {
                if (owner.y > y)
                {
                    newPos.y = owner.y - thresholdY;
                }
                else
                {
                    newPos.y = owner.y + thresholdY;
                }
            }
            //newPos.y = Math.Clamp(newPos.y, cameraTile.y - 1, cameraTile.y + 1);

            if (Math.Abs(circleDifference.y) <= thresholdY || thresholdX == 0)
            {
                if (Math.Abs(circleDifference.x) > thresholdX)
                {
                    if (circleDifference.x > 0)
                    {
                        newPos.x = (x + circleDifference.x) - thresholdX;
                    }
                    else
                    {
                        newPos.x = (x + circleDifference.x) + thresholdX;
                    }
                }

                newPos.x = Map.instance.GetXTilePositionOnMap(newPos.x);
                int cameraCircleDifference = Map.instance.GetXDifference(cameraTile.x, newPos.x);
                cameraCircleDifference = Math.Clamp(cameraCircleDifference, -1, 1);
                newPos.x = cameraTile.x + cameraCircleDifference;
                newPos.x = Map.instance.GetXTilePositionOnMap(newPos.x);
            }

            if (newPos != tilePosition && newPos.x >= 0 && newPos.x < Map.instance.width && newPos.y >= 0 && newPos.y < Map.instance.height)
            {
                Map.instance.MoveObject(this, newPos);
            }
        }

        public void UpdatePosition()
        {
            Map.instance.MoveObject(this, owner.tilePosition);
        }
    }
}