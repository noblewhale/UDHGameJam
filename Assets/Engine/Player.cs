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

        void OnPositionChange(int oldX, int oldY, int newX, int newY)
        {
            Map.instance.Reveal(newX, newY, identity.viewDistance);
        }

        void OnMapLoaded()
        {
            Tile startTile = Map.instance.GetRandomTileThatAllowsSpawn();
            startTile.AddObject(identity);
            Map.instance.UpdateLighting();
            Map.instance.Reveal(identity.x, identity.y, identity.viewDistance);
        }
    }
}