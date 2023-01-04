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
            Map.instance.UpdateIsVisible(oldPos, identity.GetComponent<Creature>().effectiveViewDistance, false);
            Map.instance.UpdateIsVisible(newPos, identity.GetComponent<Creature>().effectiveViewDistance, true);
        }

        void OnMapLoaded()
        {
            Tile startTile = Map.instance.GetRandomTileThatAllowsSpawn();
            startTile.AddObject(identity, false, 2);
            Map.instance.UpdateLighting();
            Map.instance.UpdateIsVisible(identity.tilePosition, identity.GetComponent<Creature>().effectiveViewDistance, true);
        }
    }
}