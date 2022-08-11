namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class PlayerCamera : MonoBehaviour
    {
        public DungeonObject owner;

        public float rotationMaxSpeed = .01f;
        public float rotationLerpFactor = .5f;
        public float movementMaxSpeed = .1f;
        public float movementLerpFactor = .5f;
        public float cameraOffset = 3;
        public float rotation;
        float cameraVelocity;
        public Camera camera;

        public Material polarWarpMaterial;

        public static PlayerCamera instance;

        void Start()
        {
            instance = this;
            camera = GetComponent<Camera>();
            Map.instance.OnMapLoaded += OnMapLoaded;
        }

        private void OnDestroy()
        {
            Map.instance.OnMapLoaded -= OnMapLoaded;
        }

        void OnMapLoaded()
        {
            SetRotation(Player.instance.identity.x, Player.instance.identity.y, float.Epsilon, float.MaxValue);
            SetY(Player.instance.identity.transform.position.y, 1, float.MaxValue);
        }

        void Update()
        {
            if (!owner) return;

            SetRotation(owner.x, owner.y, rotationLerpFactor, rotationMaxSpeed * Time.deltaTime * 100);
            SetY(owner.transform.position.y, movementLerpFactor, movementMaxSpeed);
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

        public void SetRotation(int x, int y, float lerpFactor, float maxSpeed)
        {
            float percentOfWidth = (float)(x + .5f) / Map.instance.width;
            float targetRotation = 2 * Mathf.PI * (1 - percentOfWidth);
            if (rotation < 0) rotation = 2 * Mathf.PI;
            else if (rotation > 2 * Mathf.PI) rotation = 0;
            if (Mathf.Abs(targetRotation - rotation) > Mathf.PI)
            {
                targetRotation = targetRotation - Mathf.Sign(targetRotation - rotation) * (2 * Mathf.PI);
            }
            rotation = Mathf.SmoothDampAngle(rotation, targetRotation, ref cameraVelocity, lerpFactor, maxSpeed);
            polarWarpMaterial.SetFloat("_Rotation", rotation - Mathf.PI / 2);
        }
    }
}