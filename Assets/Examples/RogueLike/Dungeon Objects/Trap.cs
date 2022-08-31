namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Linq;
    using UnityEngine;

    public class Trap : TickableBehaviour
    {
        public float delay = .1f;
        public bool knockBack = true;
        float actionStartTime;
        public float minDamage = 1;
        public float maxDamage = 1;
        public float damageChance = 1;
        bool doDamage = false;

        public override void StartAction()
        {
            owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
            actionStartTime = Time.time;
            doDamage = Random.value < damageChance;
        }

        virtual public void OnSteppedOn()
        {
            owner.GetComponent<Tickable>().nextBehaviour = this;
            TimeManager.instance.ForceNextAction(owner.GetComponent<Tickable>());
        }

        public override bool ContinueSubAction(ulong time)
        {
            if (doDamage)
            {
                foreach (var dungeonObject in owner.tile.objectList)
                {
                    if (dungeonObject.canTakeDamage)
                    {
                        if ((Time.time - actionStartTime) < delay)
                        {
                            dungeonObject.DamageFlash(.3f);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public override void FinishSubAction(ulong time)
        {
            if (doDamage)
            {
                foreach (var dungeonObject in owner.tile.objectList.Reverse())
                {
                    if (dungeonObject.canTakeDamage)
                    {
                        dungeonObject.DamageFlash(1);
                        dungeonObject.TakeDamage(Mathf.RoundToInt(Random.Range(minDamage, maxDamage)));
                        if (knockBack && dungeonObject.health != 0)
                        {
                            Map.instance.MoveObject(dungeonObject, dungeonObject.previousTilePosition);
                        }
                    }
                }
            }
        }

        public override float GetActionConfidence()
        {
            foreach (var dungeonObject in owner.tile.objectList)
            {
                if (dungeonObject.GetComponent<Creature>())
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}