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
            //owner.onSetPosition += OnPositionChanged;
        }

        //void OnPositionChanged(Vector2Int oldPosition, Vector2Int newPosition)
        //{
        //    Debug.Log("Position changed");
        //    foreach (var dungeonObject in owner.tile.objectList.Reverse())
        //    {
        //        var creature = dungeonObject.GetComponent<Creature>();
        //        if (creature)
        //        {
        //            if (creature.baseObject == Player.instance.identity)
        //            {
        //                Map.instance.UpdateIsVisible(creature.tilePosition, creature.effectiveViewDistance, false);
        //            }
        //            Debug.Log("Add blind modifier");
        //            creature.AddModifier<BlindModifier>();
        //        }
        //    }
        //}

        public void OnPreSteppedOn()
        {
            Debug.Log("Pre stepped on");
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