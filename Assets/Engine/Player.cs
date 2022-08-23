namespace Noble.TileEngine
{
    using UnityEngine;

    public class Player : MonoBehaviour
    {
        public DungeonObject identity;
        public PlayerInputHandler playerInput;

        public static Player instance;

        public virtual void Awake()
        {
            instance = this;

            identity.onSetPosition += OnPositionChange;
        }

        public virtual void Start()
        {
            Map.instance.OnMapLoaded += OnMapLoaded;
        }

        void OnPositionChange(Vector2Int oldPos, Vector2Int newPos)
        {
            Map.instance.Reveal(newPos, identity.viewDistance);
        }

        void OnMapLoaded()
        {
            Tile startTile = Map.instance.GetRandomTileThatAllowsSpawn();
            startTile.AddObject(identity);
            Map.instance.UpdateLighting();
            Map.instance.Reveal(identity.tilePosition, identity.viewDistance);
        }
    }
}