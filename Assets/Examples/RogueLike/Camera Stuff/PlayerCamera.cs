namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using UnityEngine;

    class PlayerCamera : MonoBehaviour
    {
        public DungeonObject owner;

        public float rotationMaxSpeed = .01f;
        public float movementMaxSpeed = .1f;
        public float cameraOffset = 3;
        public float rotation;
        new public Camera camera;

        public Material polarWarpMaterial;

        public static PlayerCamera instance;

        void Start()
        {
            instance = this;
            camera = GetComponent<Camera>();
            Player.instance.identity.onSpawn += OnPlayerSpawned;
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
            if (!owner) return;
            SetRotation(owner.x, rotationMaxSpeed);
            SetY(owner.y * Map.instance.tileHeight + Map.instance.transform.position.y, movementMaxSpeed);
        }

        public void SetY(float worldY, float maxSpeed)
        {
            cameraOffset = camera.orthographicSize - Map.instance.tileHeight * 5f;
            Vector2 targetPos = new Vector2(camera.transform.position.x, worldY + cameraOffset);
            targetPos = Vector2.MoveTowards(camera.transform.position, targetPos, maxSpeed * Time.deltaTime * 100);
            Vector2 relativePos = targetPos - (Vector2)camera.transform.position;

            if (relativePos.magnitude > maxSpeed * Time.deltaTime * 100)
            {
                relativePos = relativePos.normalized * maxSpeed * Time.deltaTime * 100;
            }

            targetPos = camera.transform.position + (Vector3)relativePos;
            camera.transform.position = new Vector3(targetPos.x, targetPos.y, camera.transform.position.z);
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
            polarWarpMaterial.SetFloat("_Rotation", rotation);
        }

        public Vector2Int GetTilePosition()
        {
            Vector2Int pos = Vector2Int.zero;
            float mapRotation = polarWarpMaterial.GetFloat("_Rotation");
            float percentOfWidth = 1 - mapRotation / (2 * Mathf.PI);
            pos.x = (int)(percentOfWidth * Map.instance.width);
            pos.y = (int)((camera.transform.position.y - .25f - Map.instance.transform.position.y) / Map.instance.tileHeight);

            return pos;
        }
    }
}