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
                    if (!isInterrupted && currentAction.owner.tile.isInView)
                    {
                        isWaitingBetweenActions = true;
                        startWaitBetweenActionsTime = Time.time;
                    }
                    currentAction.FinishAction();
                    currentAction = null;
                }
            }

            if (isWaitingBetweenActions)
            {
                if (isInterrupted)
                {
                    isWaitingBetweenActions = false;
                }
                else
                {
                    if (Time.time - startWaitBetweenActionsTime < timeBetweenActions)
                    {
                        yield return new WaitForEndOfFrame();
                        continue;
                    }
                    else
                    {
                        isWaitingBetweenActions = false;
                    }
                }
            }

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
                        bool finishImmediately = ob.StartNewAction();
                        if (finishImmediately || !ob.owner.tile.isInView)
                        {
                            ob.FinishAction();
                            currentAction = null;
                            if (!isInterrupted && ob.owner.tile.isInView)
                            {
                                isWaitingBetweenActions = true;
                                startWaitBetweenActionsTime = Time.time;
                                tickableIndex--;
                                break;
                            }
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
