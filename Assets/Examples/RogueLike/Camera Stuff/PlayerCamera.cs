﻿namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera instance;

        new public Camera camera;

        public float cameraOffset = 3;

        virtual public void Awake()
        {
            instance = this;
        }

        virtual public void OnDestroy()
        {
            instance = null;
        }

        virtual public Vector2Int GetTilePosition()
        {
            return Map.instance.GetTileFromWorldPosition(transform.position).tilePosition;
        }
    }
}