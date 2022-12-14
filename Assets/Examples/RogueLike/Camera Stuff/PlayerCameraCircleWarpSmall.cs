namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using UnityEngine;

    class PlayerCameraCircleWarpSmall : PlayerCamera
    {
        public DungeonObject owner;

        public float rotationMaxSpeed = .01f;
        public float movementMaxSpeed = .1f;
        public float rotation;

        Vector2 targetPos;

        public override void Awake()
        {
            base.Awake();
        }

        public void Start()
        {
            Player.instance.identity.onSpawn += OnPlayerSpawned;
            targetPos = transform.position;
        }

        private void OnDestroy()
        {
            Player.instance.identity.onSpawn -= OnPlayerSpawned;
        }

        void OnPlayerSpawned()
        {
            CameraTarget.instance.UpdatePosition();

            SetRotation(owner.x, float.MaxValue);
            SetY(owner.y * Map.instance.tileHeight + Map.instance.transform.position.y, float.MaxValue);
        }

        void Update()
        {
            if (!owner || owner.tile == null) return;
            SetRotation(owner.x, rotationMaxSpeed);
            SetY(owner.y * Map.instance.tileHeight + Map.instance.transform.position.y, movementMaxSpeed);
        }

        public void SetY(float worldY, float maxSpeed)
        {
            cameraOffset = camera.orthographicSize - Map.instance.tileHeight * 5f;
            Vector2 nextTargetPos = new Vector2(transform.position.x, worldY + cameraOffset);
            nextTargetPos = Vector2.MoveTowards(targetPos, nextTargetPos, maxSpeed * Time.deltaTime * 100);
            Vector2 relativePos = nextTargetPos - targetPos;

            if (relativePos.magnitude > maxSpeed * Time.deltaTime * 100)
            {
                relativePos = relativePos.normalized * maxSpeed * Time.deltaTime * 100;
            }

            nextTargetPos = targetPos + relativePos;
            float pixelY = nextTargetPos.y;
            if (MapRenderer.instance.warpMaterial.mainTexture)
            {
                float texelUnitSize = MapRenderer.instance.warpMaterial.mainTexture.texelSize.y * camera.orthographicSize * 2;
                //float texelUnitSize = (1.0f / Screen.height) * camera.orthographicSize * 2;
                pixelY = texelUnitSize * Mathf.Round(nextTargetPos.y / texelUnitSize) + texelUnitSize / 2.0f;
            }
            targetPos = nextTargetPos;
            transform.position = new Vector3(nextTargetPos.x, pixelY, transform.position.z);
        }

        public void SetRotation(int x, float maxSpeed)
        {
            // .5 to line up with the middle of the tiles instead of the edges
            float shiftedX = x + .5f;
            // Normalize to the map width, but inverted because that's how the circle warp shader works
            float normalizedX =  1 - shiftedX / Map.instance.width;
            // The normalized x position tells us how for around the circle we are, or what percentage of 2*PI
            float targetRotation = 2 * Mathf.PI * normalizedX;

            float differenceMagnitude = Mathf.Abs(targetRotation - rotation);
            float direction = Mathf.Sign(targetRotation - rotation);

            // If the difference between the current rotation and target rotation is large, then we want to use a "unwrapped" location instead
            // This prevents the camera from going the long way around the circle when the current rotation and target rotation are on opposite sides of the seam
            if (differenceMagnitude > Mathf.PI)
            {
                // This should either be a small negative number or a positive number a bit larger than 2*PI
                targetRotation -= direction * 2 * Mathf.PI; 

                // These will have changed
                differenceMagnitude = Mathf.Abs(targetRotation - rotation);
                direction = Mathf.Sign(targetRotation - rotation);
            }
            
            // This is a basic "Move towards" with a max speed
            float howFarToMove = Mathf.Min(differenceMagnitude, maxSpeed * Time.deltaTime * 100);
            rotation += direction * howFarToMove;

            // Wrap the rotation to keep it between 0 and 2*PI
            if (rotation < 0) rotation = rotation + 2 * Mathf.PI;
            else if (rotation > 2 * Mathf.PI) rotation = rotation - 2*Mathf.PI;

            // Finally set the rotation property on the shader, shifted by PI/2 because that's how the circle warp shader works
            MapRenderer.instance.warpMaterial.SetFloat("_Rotation", rotation);
        }

        override public Vector2Int GetTilePosition()
        {
            Vector2Int pos = Vector2Int.zero;
            float mapRotation = MapRenderer.instance.warpMaterial.GetFloat("_Rotation");
            float percentOfWidth = 1 - mapRotation / (2 * Mathf.PI);
            pos.x = (int)(percentOfWidth * Map.instance.width);
            pos.y = (int)((transform.position.y - .25f - Map.instance.transform.position.y) / Map.instance.tileHeight);
            //pos.y = (int)((transform.position.y - Map.instance.transform.position.y) / Map.instance.tileHeight);

            return pos;
        }
    }
}