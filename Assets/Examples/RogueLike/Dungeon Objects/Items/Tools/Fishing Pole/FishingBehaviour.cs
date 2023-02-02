using Noble.DungeonCrawler;
using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingBehaviour : TickableBehaviour
{
    Animator fishingPoleAnimator;
    public Animator bobberAnimator;
    public SpriteRenderer bobber;

    public float castSpeed = 2;
    public Vector3 startPosition;
    public Vector2 endPositionOffset;
    public bool isOnWater = true;
    public bool isLanded = false;

    public int bobCount;
    public int maxBobCount = 4;

    public bool didBob;

    private void Start()
    {
        fishingPoleAnimator = GetComponent<Animator>();
        startPosition = bobber.transform.localPosition;
    }

    public IEnumerator OnCastAnimationEnd()
    {
        bobber.transform.localPosition = startPosition;
        bobber.transform.parent = null;
        Vector3 bobberAnimationStartPosition = bobber.transform.position;

        //Get target tile
        Tile bobberTarget = GetComponent<TargetableBehaviour>().targetTile;

        //Set Bobber end position
        Vector3 endPosition = bobberTarget.position + Map.instance.tileDimensions / 2;
        endPosition += (Vector3) endPositionOffset;

        endPosition.z = -2;

        float time = 0;

        while (time < 1/castSpeed)
        {
            bobber.transform.position = bobberAnimationStartPosition + (endPosition - bobberAnimationStartPosition) * time * castSpeed;

            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        if (bobberTarget.ContainsObjectOfType("Water"))
        {
            isOnWater = true;
            bobberAnimator.SetTrigger("DoSplash");
        }
        else
        {
            isOnWater = false;
            isLanded = true;
        }
        yield return null;
    }

    public void ResetBobber()
    {
        bobber.transform.parent = gameObject.transform;
        bobber.transform.localPosition = startPosition;
        Debug.Log(bobber.transform.position, bobber.gameObject);
        didBob = false;
    }

    public override bool IsActionACoroutine() => true;

    override public IEnumerator StartActionCoroutine()
    {
        fishingPoleAnimator.SetTrigger("CastTrigger");

        Tile bobberTarget = GetComponent<TargetableBehaviour>().targetTile;

        if (bobberTarget.ContainsObjectOfType("Water"))
        {
            isOnWater = true;
            Player.Identity.GetComponent<Tickable>().nextActionTime = TimeManager.instance.Time + (ulong)maxBobCount;
            PlayerInputHandler.instance.WaitForPlayerInput = false;
        }
        else
        {
            isOnWater = false;
            Player.Identity.GetComponent<Tickable>().nextActionTime = TimeManager.instance.Time + 1;
        }

        yield return null;
    }

    override public bool ContinueSubAction(ulong time)
    {
        if (!isOnWater && isLanded)
        {
            isLanded = false;
            return true;
        }
        else if (didBob)
        {
            didBob = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    override public void FinishAction()
    {
        if (!isOnWater)
        {
            ResetBobber();
        }
        else
        {
            bobberAnimator.SetTrigger("FishOn");
        }
    }
    
}
