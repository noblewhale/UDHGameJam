using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricTrap : TickableBehaviour
{
    public float delay = .1f;
    bool shouldTrigger = false;
    Creature creatureThatSteppedOnTrap;
    float actionStartTime;
    ulong lastTriggerTime;
    ulong cooldown = 1;
    
    public void OnSteppedOn(Creature creature)
    {
        //if (TimeManager.instance.time - lastTriggerTime < cooldown) return;
        if (creature == null || creature.baseObject == null) return;

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
                creatureThatSteppedOnTrap.baseObject.DamageFlash(.3f);
            }
            return false;
        }
        else return true;
    }

    public override void FinishAction()
    {
        if (creatureThatSteppedOnTrap)
        {
            creatureThatSteppedOnTrap.baseObject.DamageFlash(1);
            creatureThatSteppedOnTrap.baseObject.TakeDamage(1);
            if (creatureThatSteppedOnTrap.health != 0)
            {
                Map.instance.MoveObject(creatureThatSteppedOnTrap.baseObject, creatureThatSteppedOnTrap.baseObject.previousX, creatureThatSteppedOnTrap.baseObject.previousY);
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
