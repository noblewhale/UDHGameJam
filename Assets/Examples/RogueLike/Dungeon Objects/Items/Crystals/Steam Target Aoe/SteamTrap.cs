namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class SteamTrap : Trap
    {
        public override void Awake()
        {
            base.Awake();
        }

        public void OnPreSteppedOn()
        {
            foreach (var dungeonObject in owner.tile.objectList.Reverse())
            {
                var creature = dungeonObject.GetComponent<Creature>();
                if (creature)
                {
                    if (creature.baseObject == Player.instance.identity)
                    {
                        Map.instance.UpdateIsVisible(creature.tilePosition, creature.effectiveViewDistance, false);
                    }
                    creature.AddModifier<BlindModifier>();
                }
            }
        }

        public override void StartAction()
        {
            base.StartAction();
            foreach (var dungeonObject in owner.tile.objectList.Reverse())
            {
                var creature = dungeonObject.GetComponent<Creature>();
                if (creature)
                {
                    if (creature.baseObject == Player.instance.identity)
                    {
                        Map.instance.UpdateIsVisible(creature.tilePosition, creature.effectiveViewDistance, false);
                    }
                    creature.AddModifier<BlindModifier>();
                }
            }
        }
    }
}