namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using UnityEngine;

    class PlayerCamera : MonoBehaviour
    {
        public DungeonObject owner;

        public float rotationMaxSpeed = .01f;
        public float rotationLerpFactor = .5f;
        public float movementMaxSpeed = .1f;
        public float movementLerpFactor = .5f;
        public float cameraOffset = 3;
        public float rotation;
        float cameraVelocity;
        new public Camera camera;

        public Material polarWarpMaterial;

        public static PlayerCamera instance;

        void Start()
        {
            instance = this;
            camera = GetComponent<Camera>();
            Player.instance.identity.onSpawn += OnPlayerSpawned;
            //Map.instance.OnMapLoaded += OnMapLoaded;
        }

        private void OnDestroy()
        {
            Player.instance.identity.onSpawn -= OnPlayerSpawned;
            //Map.instance.OnMapLoaded -= OnMapLoaded;
        }

        void OnPlayerSpawned()
        {
            CameraTarget.instance.UpdatePosition();

            SetRotation(owner.x, 0, float.MaxValue);
            SetY(owner.y * Map.instance.tileHeight + Map.instance.transform.position.y, 1, float.MaxValue);
        }

        void Update()
        {
            if (!owner) return;
            SetRotation(owner.x, rotationLerpFactor, rotationMaxSpeed * Time.deltaTime * 100);
            SetY(owner.y * Map.instance.tileHeight + Map.instance.transform.position.y, movementLerpFactor, movementMaxSpeed);
        }

        public void SetY(float worldY, float lerpFactor, float maxSpeed)
        {
            cameraOffset = camera.orthographicSize - Map.instance.tileHeight * 5f;
            Vector2 targetPos = new Vector2(camera.transform.position.x, worldY + cameraOffset);
            targetPos = Vector2.Lerp(camera.transform.position, targetPos, lerpFactor * Time.deltaTime * 100);
            Vector2 relativePos = targetPos - (Vector2)camera.transform.position;

            if (relativePos.magnitude > maxSpeed * Time.deltaTime * 100)
            {
                relativePos = relativePos.normalized * maxSpeed * Time.deltaTime * 100;
            }

            targetPos = camera.transform.position + (Vector3)relativePos;
            camera.transform.position = new Vector3(targetPos.x, targetPos.y, camera.transform.position.z);
        }

        public void SetRotation(int x, float lerpFactor, float maxSpeed)
        {
            float percentOfWidth = (float)(x + .5f) / Map.instance.width;
            float targetRotation = 2 * Mathf.PI * (1 - percentOfWidth);
            if (Mathf.Abs(targetRotation - rotation) > Mathf.PI)
            {
                targetRotation = targetRotation - Mathf.Sign(targetRotation - rotation) * (2 * Mathf.PI);
            }
            if (lerpFactor == 0)
            {
                rotation = targetRotation;
            }
            else if (rotation != targetRotation)
            {
                rotation = Mathf.SmoothDampAngle(rotation, targetRotation, ref cameraVelocity, lerpFactor, maxSpeed);
            }
            if (rotation < 0) rotation = rotation + 2 * Mathf.PI;
            else if (rotation > 2 * Mathf.PI) rotation = rotation - 2*Mathf.PI;
            polarWarpMaterial.SetFloat("_Rotation", rotation - Mathf.PI / 2);
        }

        public Vector2Int GetTilePosition()
        {
            Vector2Int pos = Vector2Int.zero;
            float mapRotation = polarWarpMaterial.GetFloat("_Rotation") + Mathf.PI / 2;
            float percentOfWidth = 1 - mapRotation / (2 * Mathf.PI);
            pos.x = (int)(percentOfWidth * Map.instance.width);
            pos.y = (int)((camera.transform.position.y - .25f - Map.instance.transform.position.y) / Map.instance.tileHeight);

            return pos;
        }
    }
}