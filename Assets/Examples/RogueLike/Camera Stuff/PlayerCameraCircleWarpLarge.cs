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

        public float weirdThing = 2*Mathf.PI;

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

            camera.transform.position = new Vector3(owner.transform.position.x + .5f, owner.transform.position.y + cameraOffset + .5f, camera.transform.position.z);
            Vector2 cameraCenterPositionRelativeToMap = camera.transform.position - Map.instance.transform.position;
            Vector2 cornerOfCameraRelativeToMap = cameraCenterPositionRelativeToMap;
            cornerOfCameraRelativeToMap.x -= camera.orthographicSize * camera.aspect;
            cornerOfCameraRelativeToMap.y -= camera.orthographicSize;
            Vector2 cameraCornerInNormalizedMapCoords = cornerOfCameraRelativeToMap / new Vector2(Map.instance.TotalWidth, Map.instance.TotalHeight);
            MapRenderer.instance.warpMaterial.SetVector("_CameraPos", cameraCornerInNormalizedMapCoords);
            float cameraWidthInNormalizedMapCoords = camera.orthographicSize * camera.aspect * 2 / Map.instance.TotalWidth;
            float cameraHeightInNormalizedMapCoords = camera.orthographicSize * 2 / GetThatWierdThing();
            Vector2 cameraDimensionsInNormalizedMapCoords = new Vector2(cameraWidthInNormalizedMapCoords, cameraHeightInNormalizedMapCoords);
            MapRenderer.instance.warpMaterial.SetVector("_CameraDim", cameraDimensionsInNormalizedMapCoords);
        }

        public float GetThatWierdThing()
        {
            return Map.instance.TotalWidth / weirdThing + MapRenderer.instance.renderedHeight/2 - cameraOffset;
        }
    }
}