using Noble.DungeonCrawler;
using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingPoleBehavior : TargetAoeShapeBehaviour
{
    public override bool IsActionACoroutine()
    {
        return true;
    }
    override public IEnumerator StartActionCoroutine()
    {
        Debug.Log("F has begun");
        yield return StartCoroutine(base.StartActionCoroutine());
    }

    override public void StartSubAction(ulong time) { }
    override public bool ContinueSubAction(ulong time) { return true; }
    override public void FinishSubAction(ulong time) { }

    override public void FinishAction() { }
}
