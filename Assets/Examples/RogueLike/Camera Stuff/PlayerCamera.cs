namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera instance;
        new public Camera camera;
        public float cameraOffset = 3;
        public DungeonObject owner;
        public float movementMaxSpeed = 8f;

        virtual public void Awake()
        {
            instance = this;
        }

        virtual public void Start()
        {
            Player.Identity.onSpawn.AddListener(OnPlayerSpawned);
        }

        virtual public void OnDestroy()
        {
            instance = null;
            if (Player.Identity)
            {
                Player.Identity.onSpawn.RemoveListener(OnPlayerSpawned);
            }
        }

        virtual public Vector2Int GetTilePosition()
        {
            return Map.instance.GetTileFromWorldPosition(transform.position).tilePosition;
        }

        void OnPlayerSpawned(DungeonObject playerOb)
        {
            float originalZ = transform.position.z;
            transform.position = playerOb.tile.position + Map.instance.tileDimensions / 2;
            transform.position += Vector3.forward * originalZ;
        }

        virtual public void Update()
        {
            if (!owner || owner.tile == null) return;

            SetPosition(movementMaxSpeed);
        }

        virtual public void SetPosition(float maxSpeed)
        {
            float originalZ = transform.position.z;
            // Move towards owner position
            Vector3 targetPos = owner.transform.position;
            // Plus half the tile width/height so that tile is centered
            targetPos += (Vector3)Map.instance.tileDimensions / 2;
            // Plus the vertical camera offset
            targetPos.y += cameraOffset;
            targetPos.z = originalZ;

            Vector2 direction = Map.instance.GetDifference(transform.position, targetPos);

            // Ok, move
            transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3)direction, Time.deltaTime * maxSpeed);
            transform.position = Map.instance.GetWorldPositionOnMap(transform.position);
            transform.position = new Vector3(transform.position.x, transform.position.y, originalZ);
        }
    }
}