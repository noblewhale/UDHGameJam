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
        //Turn Bobber on
        bobber.gameObject.SetActive(true);
        bobber.transform.localPosition = startPosition;
        bobber.transform.parent = null;
        Vector3 bobberAnimationStartPosition = bobber.transform.position;

        ////Set bobber start position from prefab
        //startPosition = new Vector3(bobber.transform.position.x, bobber.transform.position.y, -2);
        //bobber.transform.position = startPosition;




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
        }
        yield return null;
    }

    public void ResetBobber()
    {
        bobber.gameObject.SetActive(true);
        bobber.transform.parent = gameObject.transform;
        bobber.transform.localPosition = startPosition;
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
            Player.Identity.GetComponent<Tickable>().nextActionTime = TimeManager.instance.Time + 4;
            PlayerInputHandler.instance.WaitForPlayerInput = false;
        }
        else
        {
            isOnWater = false;
            Player.Identity.GetComponent<Tickable>().nextActionTime = TimeManager.instance.Time + 1;

        }


        yield return null;
    }

    override public void StartSubAction(ulong time)
    {
        Debug.Log("start sub");
    }
    override public bool ContinueSubAction(ulong time)
    {
        Debug.Log("cont sub");

        if (!isOnWater)
        {
            
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
    override public void FinishSubAction(ulong time)
    {
        Debug.Log("fin sub");
    }

    override public void FinishAction()
    {
        Debug.Log("fin action");
        

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
