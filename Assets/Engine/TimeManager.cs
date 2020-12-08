using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public ulong time = 0;
    public float timeBetweenActions = .1f;

    public static TimeManager instance;

    public List<Tickable> tickableObjects = new List<Tickable>();

    Tickable currentAction;
    bool isInterrupted = false;
    int tickableIndex = 0;
    bool isWaitingBetweenActions = false;
    float startWaitBetweenActionsTime = 0;
    Tickable interruptingTickable = null;
    bool isPlayerAction = false;
    public Camera mapCamera;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(Tick());
    }

    IEnumerator Tick()
    {
        tickableIndex = tickableObjects.Count - 1;
        while (true)
        {
            bool isDone = true;
            if (currentAction != null)
            {
                if (!isInterrupted)
                {
                    isDone = currentAction.ContinueAction();
                    if (!isDone)
                    {
                        yield return new WaitForEndOfFrame();
                        continue;
                    }
                }
                if (isInterrupted || isDone)
                {
                    //bool isOnCamera =
                    //    currentAction.owner.transform.position.x > mapCamera.transform.position.x - mapCamera.orthographicSize * mapCamera.aspect &&
                    //    currentAction.owner.transform.position.x < mapCamera.transform.position.x + mapCamera.orthographicSize * mapCamera.aspect &&
                    //    currentAction.owner.transform.position.y > mapCamera.transform.position.y - mapCamera.orthographicSize &&
                    //    currentAction.owner.transform.position.y < mapCamera.transform.position.y + mapCamera.orthographicSize;
                    //if (!isInterrupted && currentAction.owner.tile.isInView && currentAction.owner.tile.isLit && isOnCamera) 
                    //{
                    //    bool isNextTickablePlayer = tickableObjects[tickableIndex] == Player.instance.identity.GetComponent<Tickable>();
                    //    isWaitingBetweenActions = !isPlayerAction && !isNextTickablePlayer;
                    //    startWaitBetweenActionsTime = Time.time;
                    //}
                    currentAction.FinishAction();
                    currentAction = null;
                    isPlayerAction = false;
                }
            }

            //if (isWaitingBetweenActions)
            //{
            //    if (isInterrupted)
            //    {
            //        isWaitingBetweenActions = false;
            //    }
            //    else
            //    {
            //        if (Time.time - startWaitBetweenActionsTime < timeBetweenActions)
            //        {
            //            yield return new WaitForEndOfFrame();
            //            continue;
            //        }
            //        else
            //        {
            //            isWaitingBetweenActions = false;
            //        }
            //    }
            //}

            if (isDone || isInterrupted)
            {
                for (; tickableIndex >= 0; tickableIndex--)
                {
                    var ob = tickableObjects[tickableIndex];
                    if (ob.markedForRemoval)
                    {
                        continue;
                    }
                    if (time >= ob.nextActionTime)
                    {
                        if (Player.instance.identity.GetComponent<Tickable>() == ob)
                        {
                            if (Player.instance.hasReceivedInput)
                            {
                                isPlayerAction = true;
                                isInterrupted = false;
                                Player.instance.hasReceivedInput = false;
                                Player.instance.isWaitingForPlayerInput = false;
                            }
                            else
                            {
                                Player.instance.isWaitingForPlayerInput = true;
                                currentAction = null;
                                break;
                            }
                        }
                        ulong actionDuration = 1;
                        bool finishImmediately = ob.StartNewAction(out actionDuration);
                        ob.nextActionTime = time + actionDuration;
                        bool wasPlayerAction = isPlayerAction;
                        bool isOnCamera =
                            ob.owner.transform.position.x > mapCamera.transform.position.x - mapCamera.orthographicSize * mapCamera.aspect &&
                            ob.owner.transform.position.x < mapCamera.transform.position.x + mapCamera.orthographicSize * mapCamera.aspect &&
                            ob.owner.transform.position.y > mapCamera.transform.position.y - mapCamera.orthographicSize &&
                            ob.owner.transform.position.y < mapCamera.transform.position.y + mapCamera.orthographicSize;
                        if (!isInterrupted && ob.owner.tile.isInView && ob.owner.tile.isLit && isOnCamera)
                        {
                            int nextTickableIndex = tickableIndex - 1;
                            if (nextTickableIndex <= 0)
                            {
                                nextTickableIndex = tickableObjects.Count - 1;
                            }
                            bool isNextTickablePlayer = tickableObjects[nextTickableIndex] == Player.instance.identity.GetComponent<Tickable>();
                            isWaitingBetweenActions = !wasPlayerAction && !isNextTickablePlayer && !finishImmediately;
                            startWaitBetweenActionsTime = Time.time;
                            if (isWaitingBetweenActions)
                            {
                                yield return new WaitForSeconds(timeBetweenActions);
                            }
                            //tickableIndex--;
                            //break;
                        }
                        if (finishImmediately || !ob.owner.tile.isInView || !ob.owner.tile.isLit || isInterrupted || !isOnCamera)
                        {   
                            ob.FinishAction();
                            currentAction = null;
                            isPlayerAction = false;
                            //tickableIndex--;
                        }
                        else
                        {
                            currentAction = ob;
                        }

                        if (isInterrupted && ob == interruptingTickable)
                        {
                            isInterrupted = false;
                            interruptingTickable = null;
                        }

                        if (currentAction != null)
                        {
                            tickableIndex--;
                            break;
                        }
                    }
                }

                if (tickableIndex == -1)
                {
                    for (int ri = tickableObjects.Count - 1; ri >= 0; ri--)
                    {
                        var ob = tickableObjects[ri];
                        if (ob.markedForRemoval)
                        {
                            tickableObjects.RemoveAt(ri);
                        }
                    }

                    time++;
                    tickableIndex = tickableObjects.Count - 1;
                }
            }

            if (currentAction == null) yield return new WaitForEndOfFrame();
        }
    }
    public void Interrupt(Tickable interruptingTickable)
    {
        this.interruptingTickable = interruptingTickable;
        isInterrupted = true;
    }
}
