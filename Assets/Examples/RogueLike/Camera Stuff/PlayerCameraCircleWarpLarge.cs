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
            Player.Identity.onSpawn.AddListener(OnPlayerSpawned);
        }

        override public void OnDestroy()
        {
            if (Player.Identity)
            {
                Player.Identity.onSpawn.RemoveListener(OnPlayerSpawned);
            }
        }

        void OnPlayerSpawned(DungeonObject playerOb)
        {
            float originalZ = transform.position.z;
            transform.position = playerOb.tile.position + Map.instance.tileDimensions / 2;
            transform.position += Vector3.forward * originalZ;
        }

        void Update()
        {
            if (!owner || owner.tile == null) return;

            SetPosition(movementMaxSpeed);
        }

        void SetPosition(float maxSpeed)
        {
            float originalZ = transform.position.z;
            // Move towards owner position
            Vector3 targetPos = owner.transform.position;
            // Plus half the tile width/height so that tile is centered
            targetPos += (Vector3)Map.instance.tileDimensions / 2;
            // Plus the vertical camera offset
            targetPos.y += cameraOffset;

            Vector2 direction = Map.instance.GetDifference(transform.position, targetPos);

            // Ok, move
            transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3)direction, Time.deltaTime * maxSpeed);
            transform.position = Map.instance.GetWorldPositionOnMap(transform.position);
            transform.position += Vector3.forward * originalZ;
        }
    }
}