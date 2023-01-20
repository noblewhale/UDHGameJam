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
    public Vector2 endPositionOffset;

    public int bobCount;
    public int maxBobCount = 4;

    public bool didBob;

    private void Start()
    {
        fishingPoleAnimator = GetComponent<Animator>();

    }

    public IEnumerator OnCastAnimationEnd()
    {
        //Turn Bobber on
        bobber.gameObject.SetActive(true);
        bobber.transform.parent = null;

        //Set bobber start position from prefab
        Vector3 startPosition = new Vector3(bobber.transform.position.x, bobber.transform.position.y, -2);
        bobber.transform.position = startPosition;

        //Get target tile
        Tile bobberTarget = GetComponent<TargetableBehaviour>().targetTile;

        //Set Bobber end position
        Vector3 endPosition = bobberTarget.position + Map.instance.tileDimensions / 2;
        endPosition += (Vector3) endPositionOffset;

        endPosition.z = -2;

        float time = 0;

        while (time < 1/castSpeed)
        {
            bobber.transform.position = startPosition + (endPosition - startPosition) * time * castSpeed;

            yield return new WaitForEndOfFrame();
            time += Time.deltaTime;
        }

        bobberAnimator.SetTrigger("DoSplash");

        yield return null;
    }

    public override bool IsActionACoroutine() => true;

    override public IEnumerator StartActionCoroutine()
    {
        fishingPoleAnimator.SetTrigger("CastTrigger");
        yield return null;
    }

    override public void StartSubAction(ulong time)
    {
        Debug.Log("start sub");
    }
    override public bool ContinueSubAction(ulong time)
    {
        Debug.Log("cont sub");

        if (didBob)
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
        bobberAnimator.SetTrigger("FishOn");
    }
    
}
