﻿namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MovementBehaviourRandom : TickableBehaviour
    {

        Tile nextMoveTarget;
        Creature owningCreature;

        override public void Awake()
        {
            base.Awake();
            owningCreature = owner.GetComponent<Creature>();
        }

        public override void StartAction()
        {
            owner.tickable.nextActionTime = TimeManager.instance.Time + owningCreature.ticksPerMove;
        }

        public override bool ContinueSubAction(ulong time)
        {
            return true;
        }

        public override void FinishSubAction(ulong time)
        {
            owner.map.MoveObject(owner, nextMoveTarget.x, nextMoveTarget.y);
        }

        public override float GetActionConfidence()
        {
            List<Tile> adjacentAndOpen = new List<Tile>();

            Tile adjacent;
            if (owner.y < owner.map.height - 1)
            {
                adjacent = owner.map.tileObjects[owner.y + 1][owner.x];
                if (!adjacent.IsCollidable() && !adjacent.GetComponentInChildren<Trap>()) adjacentAndOpen.Add(adjacent);
            }
            if (owner.y > 0)
            {
                adjacent = owner.map.tileObjects[owner.y - 1][owner.x];
                if (!adjacent.IsCollidable() && !adjacent.GetComponentInChildren<Trap>()) adjacentAndOpen.Add(adjacent);
            }

            int wrappedX = owner.map.GetXPositionOnMap(owner.x + 1);
            adjacent = owner.map.tileObjects[owner.y][wrappedX];
            if (!adjacent.IsCollidable() && !adjacent.GetComponentInChildren<Trap>()) adjacentAndOpen.Add(adjacent);

            wrappedX = owner.map.GetXPositionOnMap(owner.x - 1);
            adjacent = owner.map.tileObjects[owner.y][wrappedX];
            if (!adjacent.IsCollidable() && !adjacent.GetComponentInChildren<Trap>()) adjacentAndOpen.Add(adjacent);

            if (adjacentAndOpen.Count > 0)
            {
                nextMoveTarget = adjacentAndOpen[Random.Range(0, adjacentAndOpen.Count)];

                return .5f;
            }

            return 0;
        }
    }
}