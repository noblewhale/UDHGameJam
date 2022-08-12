namespace Noble.TileEngine
{
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
            while (true)
            {
                for (currentTickableNode = tickableObjects.Last; currentTickableNode != null; currentTickableNode = currentTickableNode.Previous)
                {
                    if (currentTickableNode.Value.markedForRemoval) continue;

                    var ob = currentTickableNode.Value;

                    if (Player.instance.identity.tickable == ob)
                    {
                        while (!PlayerInput.instance.HasInput) yield return new WaitForEndOfFrame();
                        
                        isInterrupted = false;
                    }

                    if (Time >= ob.nextActionTime)
                    {
                        var behaviour = ob.GetBehaviourToExecute();
                        if (behaviour != null)
                        {
                            if (behaviour.IsActionACoroutine())
                            {
                                yield return ob.StartActionCoroutine();
                            }
                            else
                            {
                                ob.StartAction();
                            }
                        }
                    }

                    ob.StartSubAction();

                    bool isOnCamera = mapCamera.Contains(ob.owner.transform.position);

                    if (isOnCamera && ob.owner.tile.isInView && ob.owner.tile.isLit)
                    {
                        while (!isInterrupted)
                        {
                            bool isDone = ob.ContinueSubAction();
                            if (isDone) break;
                            yield return new WaitForEndOfFrame();
                        }
                    }

                    ob.FinishSubAction();

                    if (Time == ob.nextActionTime - 1)
                    {
                        ob.FinishAction();
                    }

                    if (isInterrupted && ob == interruptingTickable)
                    {
                        isInterrupted = false;
                        interruptingTickable = null;
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

                Time++;

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