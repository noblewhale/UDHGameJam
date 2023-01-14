using Noble.DungeonCrawler;
using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingPoleBehavior : TickableBehaviour
{
    public PowerBehaviour fishingBehaviour;
    public TargetableBehaviour castBehaviour;

    public override bool IsActionACoroutine() => true;
    
    override public IEnumerator StartActionCoroutine()
    {
        yield return castBehaviour.StartActionCoroutine();
        yield return fishingBehaviour.StartActionCoroutine();
    }

    override public void StartSubAction(ulong time) 
    { 
        fishingBehaviour.StartSubAction(time);  
    }
    override public bool ContinueSubAction(ulong time) 
    { 
        return true; 
    }
    override public void FinishSubAction(ulong time) 
    { 
        fishingBehaviour.FinishSubAction(time);
    }

    override public void FinishAction() 
    { 
        fishingBehaviour.FinishAction();
    }
}
