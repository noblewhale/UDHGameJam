using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : TickableBehaviour
{
    struct BehaviourAction
    {
        public delegate bool StartAction();
        public delegate bool ContinueAction();
        public delegate void FinishAction();
        public StartAction startAction;
        public ContinueAction continueAction;
        public FinishAction finishAction;
    }

    BehaviourAction nextAction;
    int nextActionTargetX, nextActionTargetY;

    public override bool StartAction()
    {
        DetermineAutoAction(nextActionTargetX, nextActionTargetY);
        if (nextAction.startAction != null)
        {
            return nextAction.startAction();
        }
        return true;
    }

    public override bool ContinueAction()
    {
        if (nextAction.continueAction != null)
        {
            return nextAction.continueAction();
        }
        return true;
    }

    public override void FinishAction()
    {
        if (nextAction.finishAction != null)
        {
            nextAction.finishAction();
        }
    }

    public override float GetActionConfidence()
    {
        return 1;
    }

    public void DetermineAutoAction(int newTileX, int newTileY)
    {
        var tileActingOn = owner.map.tileObjects[newTileY][newTileX];

        nextAction = new BehaviourAction();
        if (!tileActingOn.IsCollidable())
        {
            nextAction.finishAction = () => owner.map.TryMoveObject(owner, newTileX, newTileY);
        }
        else
        {
            var identityCreature = owner.GetComponent<Creature>();
            foreach (var dOb in tileActingOn.objectList)
            {
                if (dOb.isCollidable)
                {
                    nextAction.startAction = () => {
                        identityCreature.StartAttack(dOb);
                        return false;
                    };
                    nextAction.continueAction = () => {
                        return identityCreature.ContinueAttack(dOb);
                    };
                    nextAction.finishAction = () => {
                        owner.map.TryMoveObject(owner, newTileX, newTileY);
                        identityCreature.FinishAttack(dOb);
                    };
                    break;
                }
            }
        }
    }

    public void SetNextActionTarget(int newTileX, int newTileY)
    {
        nextActionTargetX = newTileX;
        nextActionTargetY = newTileY;
    }
}
