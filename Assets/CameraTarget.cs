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

            if (Math.Abs(owner.y - y) > thresholdY)
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

            if (Math.Abs(owner.y - y) <= thresholdY || thresholdX == 0)
            {
                int circleDifference = Util.GetCircleDifference(x, owner.x);
                //Debug.Log("circle dif " + circleDifference);
                if (Math.Abs(circleDifference) > thresholdX)
                {
                    if (circleDifference > 0)
                    {
                        newX = (x + circleDifference) - thresholdX;
                    }
                    else
                    {
                        newX = (x + circleDifference) + thresholdX;
                    }
                }

                newX = Map.instance.WrapX(newX);
                int cameraCircleDifference = Util.GetCircleDifference(cameraTile.x, newX);
                //Debug.Log("camera circle dif " + cameraCircleDifference);
                cameraCircleDifference = Math.Clamp(cameraCircleDifference, -1, 1);
                newX = cameraTile.x + cameraCircleDifference;
                newX = Map.instance.WrapX(newX);
            }

            if ((newX != x || newY != y) && newX >= 0 && newX < Map.instance.width && newY >= 0 && newY < Map.instance.height)
            {
                //Debug.Log("target pos " + newX + " " + newY);
                Map.instance.MoveObject(this, newX, newY);
            }
        }

        public void UpdatePosition()
        {
            Map.instance.MoveObject(this, owner.x, owner.y);
        }
    }
}