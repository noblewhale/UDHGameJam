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

        override protected void Start()
        {
            base.Start();
            Player.Identity.onSpawn.AddListener(UpdatePosition);
        }

        virtual public void OnDestroy()
        {
            instance = null;
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

            if (Math.Abs(circleDifference.y) <= thresholdY || thresholdX == 0)
            {
                if (Math.Abs(circleDifference.x) > thresholdX)
                {
                    if (circleDifference.x > 0)
                    {
                        newPos.x = owner.x - thresholdY;
                    }
                    else
                    {
                        newPos.x = owner.x + thresholdY;
                    }
                }
            }

            newPos = Map.instance.GetTilePositionOnMap(newPos);

            if (newPos != tilePosition)
            {
                Map.instance.MoveObject(this, newPos);
            }
        }

        public void UpdatePosition(DungeonObject _)
        {
            if (owner.tile == null) return;
            Map.instance.MoveObject(this, owner.tilePosition);
        }
    }
}