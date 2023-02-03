using Noble.DungeonCrawler;
using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingPoleBehaviour : TickableBehaviour
{
    public FishingBehaviour fishingBehaviour;
    public TargetableBehaviour castBehaviour;
    public float catchFishChance = .8f;
    public float catchItemChance = .8f;

    public void Start()
    {
        fishingBehaviour.fishingPoleBehaviour = this;
    }

    public void OnCastAnimationEnd()
    {
        fishingBehaviour.OnCastAnimationEnd();
    }

    public override bool IsActionACoroutine() => true;
    
    override public IEnumerator StartActionCoroutine()
    {
        

        yield return castBehaviour.StartActionCoroutine();
        castBehaviour.FinishAction();

        yield return fishingBehaviour.StartActionCoroutine();
    }

    override public void StartSubAction(ulong time) 
    { 
        fishingBehaviour.StartSubAction(time);  
    }
    override public bool ContinueSubAction(ulong time) 
    {
        return fishingBehaviour.ContinueSubAction(time); 
    }
    override public void FinishSubAction(ulong time) 
    { 
        fishingBehaviour.FinishSubAction(time);
    }

    override public void FinishAction() 
    { 
        fishingBehaviour.FinishAction();
        PlayerInputHandler.instance.WaitForPlayerInput = true;
    }
}
