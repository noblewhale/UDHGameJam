using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public ulong time = 0;
    public float timeBetweenActions = .1f;

    public static TimeManager instance;

    public LinkedList<Tickable> tickableObjects = new LinkedList<Tickable>();

    Tickable currentAction;
    bool isInterrupted = false;
    //int tickableIndex = 0;
    LinkedListNode<Tickable> currentTickableNode;
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
        //tickableIndex = tickableObjects.Count - 1;
        currentTickableNode = tickableObjects.Last;
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
                    currentAction.FinishAction();
                    currentAction = null;
                    isPlayerAction = false;
                }
            }

            if (isDone || isInterrupted)
            {
                for (;  currentTickableNode != null; currentTickableNode = currentTickableNode.Previous)
                {
                    var ob = currentTickableNode.Value;
                    if (ob.markedForRemoval)
                    {
                        continue;
                    }
                    if (time >= ob.nextActionTime)
                    {
                        if (Player.instance.identity.GetComponent<Tickable>() == ob)
                        {
                            if (PlayerInput.instance.HasInput)
                            {
                                isPlayerAction = true;
                                isInterrupted = false;
                            }
                            else
                            {
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
                            bool isNextTickablePlayer = false;
                            if (currentTickableNode.Previous != null)
                            {
                                isNextTickablePlayer = currentTickableNode.Previous.Value == Player.instance.identity.GetComponent<Tickable>();
                            }
                            isWaitingBetweenActions = !wasPlayerAction && !isNextTickablePlayer && !finishImmediately;
                            startWaitBetweenActionsTime = Time.time;
                            if (isWaitingBetweenActions)
                            {
                                yield return new WaitForSeconds(timeBetweenActions);
                            }
                        }
                        if (finishImmediately || !ob.owner.tile.isInView || !ob.owner.tile.isLit || isInterrupted || !isOnCamera)
                        {
                            currentAction = ob;
                            ob.FinishAction();
                            currentAction = null;
                            isPlayerAction = false;
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
                            currentTickableNode = currentTickableNode.Previous;
                            break;
                        }
                    }
                }

                if (currentTickableNode == null)
                {
                    var node = tickableObjects.Last;
                    while (node != null)
                    {
                        var nodeToRemove = node;
                        node = node.Previous;
                        var ob = nodeToRemove.Value;
                        if (ob.markedForRemoval)
                        {
                            tickableObjects.Remove(nodeToRemove);
                        }
                    }

                    time++;
                    currentTickableNode = tickableObjects.Last;
                }
            }

            if (currentAction == null) yield return new WaitForEndOfFrame();
        }
    }

    public void ForceNextAction(Tickable tickable)
    {
        if (currentTickableNode == tickable.listNode || (currentTickableNode == currentAction.listNode && currentTickableNode.Previous == tickable.listNode)) return;
        tickable.nextActionTime = time;
        tickableObjects.Remove(tickable.listNode);
        if (currentTickableNode == currentAction.listNode)
        {
            tickable.listNode = tickableObjects.AddBefore(currentAction.listNode, tickable);
        }
        else
        {
            tickable.listNode = tickableObjects.AddAfter(currentTickableNode, tickable);
            currentTickableNode = tickable.listNode;
        }
    }

    public void Interrupt(Tickable interruptingTickable)
    {
        this.interruptingTickable = interruptingTickable;
        isInterrupted = true;
    }
}
