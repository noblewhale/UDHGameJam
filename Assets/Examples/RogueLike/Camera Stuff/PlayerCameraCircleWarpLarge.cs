namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    class PlayerCameraCircleWarpLarge : PlayerCamera
    {
        public DungeonObject owner;

        public float movementMaxSpeed = .1f;

        public void Start()
        {
            Player.Identity.onSpawn += OnPlayerSpawned;
        }

        private void OnDestroy()
        {
            Player.Identity.onSpawn -= OnPlayerSpawned;
        }

        void OnPlayerSpawned()
        {
            CameraTarget.instance.UpdatePosition();

            SetPosition(float.MaxValue);
        }

        void Update()
        {
            if (!owner || owner.tile == null) return;

            SetPosition(movementMaxSpeed);
        }

        void SetPosition(float maxSpeed)
        {
            // Move towards owner position
            Vector3 targetPos = owner.transform.position;
            // Plus half the tile width/height so that tile is centered
            targetPos += (Vector3)Map.instance.tileDimensions / 2;
            // Plus the vertical camera offset
            targetPos.y += cameraOffset;
            // But never change the camera's z position
            targetPos.z = transform.position.z;
            // Ok, move
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * maxSpeed);
        }
    }
}