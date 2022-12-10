namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    class PlayerCameraCircleWarpLarge : PlayerCamera
    {
        public DungeonObject owner;

        public float rotationMaxSpeed = .01f;
        public float movementMaxSpeed = .1f;
        public float rotation;

        public void Start()
        {
            Player.instance.identity.onSpawn += OnPlayerSpawned;
        }

        private void OnDestroy()
        {
            Player.instance.identity.onSpawn -= OnPlayerSpawned;
        }

        void OnPlayerSpawned()
        {
            CameraTarget.instance.UpdatePosition();   
        }

        void Update()
        {
            if (!owner || owner.tile == null) return;

            camera.transform.position = new Vector3(owner.transform.position.x, owner.transform.position.y, camera.transform.position.z);
            Vector2 cameraCenterPositionRelativeToMap = camera.transform.position - Map.instance.transform.position;
            Vector2 cornerOfCameraRelativeToMap = cameraCenterPositionRelativeToMap;
            cornerOfCameraRelativeToMap.x -= camera.orthographicSize * camera.aspect;
            cornerOfCameraRelativeToMap.y -= camera.orthographicSize;
            Vector2 cameraCornerInNormalizedMapCoords = cornerOfCameraRelativeToMap / new Vector2(Map.instance.TotalWidth, Map.instance.TotalHeight);
            MapRenderer.instance.warpMaterial.SetVector("_CameraPos", cameraCornerInNormalizedMapCoords);
            Vector2 cameraDimensionsInNormalizedMapCoords = new Vector2(camera.orthographicSize * camera.aspect * 2 / Map.instance.TotalWidth, camera.orthographicSize * 2 / (Map.instance.TotalWidth / (2*Mathf.PI)));
            MapRenderer.instance.warpMaterial.SetVector("_CameraDim", cameraDimensionsInNormalizedMapCoords);
        }
    }
}