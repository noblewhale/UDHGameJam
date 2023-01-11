namespace Noble.TileEngine
{
    using UnityEngine;

    public class Player : MonoBehaviour
    {
        public DungeonObject identity;
        public PlayerInputHandler playerInput;

        public static Player instance;

        // For my sanity and finger joints
        public static DungeonObject Identity => instance?.identity;
        public static Creature Creature => Identity?.Creature;
        public static Equipment Equipment => Creature?.Equipment;

        public virtual void Awake()
        {
            instance = this;

            identity.onSetPosition.AddListener(OnPositionChange);
        }

        public virtual void Start()
        {
            Map.instance.OnMapLoaded += OnMapLoaded;
        }

        void OnPositionChange(DungeonObject ob, Vector2Int oldPos, Vector2Int newPos)
        {
            Map.instance.UpdateIsVisible(oldPos, ob.Creature.effectiveViewDistance, false);
            Map.instance.UpdateIsVisible(newPos, ob.Creature.effectiveViewDistance, true);
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