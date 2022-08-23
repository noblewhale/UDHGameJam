namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using UnityEngine;

    public class Trap : TickableBehaviour
    {
        public float delay = .1f;
        public bool knockBack = true;
        bool shouldTrigger = false;
        DungeonObject creatureThatSteppedOnTrap;
        float actionStartTime;
        ulong lastTriggerTime;

        public void OnSteppedOn(DungeonObject creature)
        {
            //if (TimeManager.instance.time - lastTriggerTime < cooldown) return;
            if (creature == null) return;

            lastTriggerTime = TimeManager.instance.Time;
            shouldTrigger = true;
            creatureThatSteppedOnTrap = creature;
            TimeManager.instance.ForceNextAction(owner.GetComponent<Tickable>());
        }

        public override void StartAction()
        {
            shouldTrigger = false;
            owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
            actionStartTime = Time.time;
        }

        public override bool ContinueSubAction(ulong time)
        {
            if ((Time.time - actionStartTime) < delay)
            {
                if (creatureThatSteppedOnTrap)
                {
                    creatureThatSteppedOnTrap.DamageFlash(.3f);
                }
                return false;
            }
            else return true;
        }

        public override void FinishSubAction(ulong time)
        {
            if (creatureThatSteppedOnTrap)
            {
                creatureThatSteppedOnTrap.DamageFlash(1);
                creatureThatSteppedOnTrap.TakeDamage(1);
                if (knockBack && creatureThatSteppedOnTrap.health != 0)
                {
                    Map.instance.MoveObject(creatureThatSteppedOnTrap, creatureThatSteppedOnTrap.previousTilePosition);
                }
            }
            creatureThatSteppedOnTrap = null;
        }

        public override float GetActionConfidence()
        {
            if (shouldTrigger)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}