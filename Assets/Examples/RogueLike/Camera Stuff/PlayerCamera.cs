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
        public Vector3 rotation;
        float cameraVelocity;
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

            SetRotation(owner.x, 0, float.MaxValue);
            SetY(owner.y * Map.instance.tileHeight + Map.instance.transform.position.y, 1, float.MaxValue);
        }

        void Update()
        {
            if (!owner) return;
            SetRotation(owner.x, rotationLerpFactor, rotationMaxSpeed);
            SetY(owner.y * Map.instance.tileHeight + Map.instance.transform.position.y, movementLerpFactor, movementMaxSpeed);
        }

        public void SetY(float worldY, float lerpFactor, float maxSpeed)
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

        public void SetRotation(int x, float lerpFactor, float maxSpeed)
        {
            float percentOfWidth = (float)(x + .5f) / Map.instance.width;
            Vector3 targetRotation = new Vector3(0, 0, 2 * Mathf.PI * (1 - percentOfWidth));

            if (Mathf.Abs(targetRotation.z - rotation.z) > Mathf.PI)
            {
                targetRotation.z = targetRotation.z - Mathf.Sign(targetRotation.z - rotation.z) * (2 * Mathf.PI);
            }
            if (lerpFactor == 0)
            {
                rotation.z = targetRotation.z;
            }
            else if (rotation.z != targetRotation.z)
            {
                float mag = Mathf.Min(Mathf.Abs(targetRotation.z - rotation.z), maxSpeed * Time.deltaTime * 100);
                rotation.z += Mathf.Sign(targetRotation.z - rotation.z) * mag;
            }
            if (rotation.z < 0) rotation.z = rotation.z + 2 * Mathf.PI;
            else if (rotation.z > 2 * Mathf.PI) rotation.z = rotation.z - 2*Mathf.PI;
            polarWarpMaterial.SetFloat("_Rotation", rotation.z - Mathf.PI / 2);
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