namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class ElectricTrap : TickableBehaviour
    {
        public float delay = .1f;
        bool shouldTrigger = false;
        DungeonObject creatureThatSteppedOnTrap;
        float actionStartTime;
        ulong lastTriggerTime;
        ulong cooldown = 1;

        public void OnSteppedOn(DungeonObject creature)
        {
            //if (TimeManager.instance.time - lastTriggerTime < cooldown) return;
            if (creature == null) return;

            lastTriggerTime = TimeManager.instance.time;
            shouldTrigger = true;
            creatureThatSteppedOnTrap = creature;
            TimeManager.instance.ForceNextAction(owner.GetComponent<Tickable>());
        }

        public override bool StartAction(out ulong duration)
        {
            shouldTrigger = false;
            duration = 1;
            actionStartTime = Time.time;
            return false;
        }

        public override bool ContinueAction()
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

        public override void FinishAction()
        {
            if (creatureThatSteppedOnTrap)
            {
                creatureThatSteppedOnTrap.DamageFlash(1);
                creatureThatSteppedOnTrap.TakeDamage(1);
                if (creatureThatSteppedOnTrap.health != 0)
                {
                    Map.instance.MoveObject(creatureThatSteppedOnTrap, creatureThatSteppedOnTrap.previousX, creatureThatSteppedOnTrap.previousY);
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