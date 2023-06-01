namespace Noble.TileEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TimeManager : MonoBehaviour
    {
        public ulong Time { get; private set; }

        public static TimeManager instance;

        public LinkedList<Tickable> tickableObjects = new LinkedList<Tickable>();

        bool isInterrupted = false;
        Tickable interruptingTickable = null;

        LinkedListNode<Tickable> currentTickableNode;

        [NonSerialized]
        public Camera mapCamera;

        void Awake()
        {
            instance = this;
        }

        virtual public void OnDestroy()
        {
            instance = null;
        }

        void Start()
        {
            StartCoroutine(Tick());
        }

        IEnumerator Tick()
        {
            while (true)
            {
                for (currentTickableNode = tickableObjects.Last; currentTickableNode != null; )
                {
                    if (currentTickableNode.Value.markedForRemoval)
                    {
                        currentTickableNode = currentTickableNode.Previous;
                        continue; 
                    }

                    var ob = currentTickableNode.Value;

                    if (Player.Identity.tickable == ob)
                    {
                        while (!PlayerInputHandler.instance.HasInput && PlayerInputHandler.instance.WaitForPlayerInput) yield return new WaitForEndOfFrame();
                        
                        isInterrupted = false;
                    }

                    if (Time >= ob.nextActionTime)
                    {
                        var behaviours = ob.GetBehavioursToExecute();
                        if (behaviours != null && behaviours.Count != 0)
                        {
                            foreach (var behaviour in behaviours)
                            {
                                if (behaviour == null) continue;
                                if (behaviour.IsActionACoroutine())
                                {
                                    yield return behaviour.StartActionCoroutine();
                                }
                                else
                                {
                                    behaviour.StartAction();
                                }
                            }
                        }
                    }

                    var currentBehaviours = ob.currentBehaviours;
                    if (currentBehaviours != null && currentBehaviours.Count != 0)
                    {
                        foreach (var behaviour in currentBehaviours)
                        {
                            if (behaviour == null) continue;
                            behaviour.StartSubAction(Time - ob.lastActionTime);
                        }
                    }

                    bool isOnCamera = mapCamera && mapCamera.Contains(ob.owner.transform.position);

                    if (isOnCamera && ob.owner && ob.owner.tile != null && ob.owner.tile.isInView && ob.owner.tile.IsLit)
                    {
                        if (currentBehaviours != null && currentBehaviours.Count != 0)
                        {
                            foreach (var behaviour in currentBehaviours)
                            {
                                if (behaviour == null) continue;
                                while (true)
                                {
                                    bool isDone = behaviour.ContinueSubAction(Time - ob.lastActionTime);
                                    if (isDone) break;
                                    if (Player.Identity.tickable != ob && isInterrupted) break;
                                    yield return new WaitForEndOfFrame();
                                }
                            }
                        }
                    }
                    if (currentBehaviours != null && currentBehaviours.Count != 0)
                    {
                        foreach (var behaviour in currentBehaviours)
                        {
                            if (behaviour == null) continue;
                            behaviour.FinishSubAction(Time - ob.lastActionTime);
                        }
                    }

                    bool dontTick = false;
                    if (Time >= ob.nextActionTime - 1)
                    {
                        if (currentBehaviours != null && currentBehaviours.Count != 0)
                        {
                            foreach (var behaviour in currentBehaviours)
                            {
                                if (behaviour == null) continue;
                                behaviour.FinishAction();
                            }
                        }
                        if (ob.nextActionTime == Time)
                        {
                            dontTick = true;
                        }
                    }

                    if (isInterrupted && ob == interruptingTickable)
                    {
                        isInterrupted = false;
                        interruptingTickable = null;
                    }

                    if (!dontTick)
                    {
                        currentTickableNode = currentTickableNode.Previous;
                    }
                }

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

                Map.instance.ForEachTile(t => t.RemovePendingObjects());

                Time++;
                //Map.instance.UpdateLighting();

                yield return new WaitForEndOfFrame();
            }
        }

        public void ForceNextAction(Tickable tickable)
        {
            if (currentTickableNode == tickable.listNode) return;
            tickable.nextActionTime = Time;
            tickableObjects.Remove(tickable.listNode);
            tickable.listNode = tickableObjects.AddBefore(currentTickableNode, tickable);
        }

        public void Interrupt(Tickable interruptingTickable)
        {
            this.interruptingTickable = interruptingTickable;
            isInterrupted = true;
        }
    }
}