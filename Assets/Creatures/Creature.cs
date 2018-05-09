using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : DungeonObject
{
    public GameObject mainGlyph;

    public Race race;

    public int mana;
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int charisma;
    public int ticksPerMove = 1;
    public int ticksPerAttack = 1;
    public AnimationCurve attackMovementAnimation;
    public float Speed {
        get {
            if (ticksPerMove == 0) return 0;
            return 1 / ticksPerMove;
        }
    }

    public Direction lastDirectionAttackedOrMoved = Direction.UP;
    public ulong nextActionTime = 0;
    public float viewDistance = 4;

    public MovementBehaviour movementBehaviour;
    public AttackBehaviour attackBehaviour;

    override protected void Awake()
    {
        base.Awake();
        movementBehaviour = GetComponent<MovementBehaviour>();
        attackBehaviour = GetComponent<AttackBehaviour>();
    }

    public void SetPosition(int x, int y)
    {
        if (x < base.x) lastDirectionAttackedOrMoved = Direction.LEFT;
        if (x > base.x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        if (y < base.y) lastDirectionAttackedOrMoved = Direction.DOWN;
        if (y > base.y) lastDirectionAttackedOrMoved = Direction.UP;
        map.tileObjects[base.y][base.x].SetOccupant(null);
        map.tileObjects[y][x].SetOccupant(this);
        base.x = x;
        base.y = y;
        transform.localPosition = new Vector3(base.x * map.tileWidth, base.y * map.tileHeight, transform.localPosition.z);

        nextActionTime = TimeManager.time + (ulong)ticksPerMove;
    }

    override public void Die()
    {
        map.tileObjects[y][x].SetOccupant(null);
        CreatureSpawner.instance.RemoveCreature(this);
        Destroy(gameObject);
    }

    virtual public void StartNewAction()
    {
        float shouldMoveConfidence = 0;
        float shouldAttackConfidence = 0;
        if (movementBehaviour)
        {
            shouldMoveConfidence = movementBehaviour.ShouldMove();
        }
        if (attackBehaviour)
        {
            shouldAttackConfidence = attackBehaviour.ShouldAttack();
        }

        if (shouldMoveConfidence == 0 && shouldAttackConfidence == 0) return;

        float totalConfidence = shouldMoveConfidence + shouldAttackConfidence;
        float random = UnityEngine.Random.Range(0, totalConfidence);

        if (random < shouldMoveConfidence)
        {
            movementBehaviour.Move();
        }
        else
        { 
            attackBehaviour.Attack();
        }
    }

    virtual public void ContinueAction()
    {
    }
    
    public void Attack(Creature creature)
    {
        if (creature.x < x) lastDirectionAttackedOrMoved = Direction.LEFT;
        if (creature.x > x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        if (creature.y < y) lastDirectionAttackedOrMoved = Direction.DOWN;
        if (creature.y > y) lastDirectionAttackedOrMoved = Direction.UP;
        float roll = UnityEngine.Random.Range(0, 20);
        roll += dexterity;
        if (roll > creature.dexterity)
        {
            // Hit, but do we do damange?
            if (roll > creature.dexterity + creature.defense)
            {
                // Got past armor / defense
                creature.TakeDamage(1);
            }
        }
        nextActionTime = TimeManager.time + (ulong)ticksPerAttack;

        if (attackMovementAnimation.length != 0)
        {
            StartCoroutine(DoAttackAnimation());
        }
    }

    IEnumerator DoAttackAnimation()
    {
        float t = 0;
        float duration = .5f;
        Vector3 originalPosition = mainGlyph.transform.localPosition;
        while (t < duration)
        {
            float offset = attackMovementAnimation.Evaluate(t/duration);

            switch (lastDirectionAttackedOrMoved)
            {
                case Direction.UP: mainGlyph.transform.localPosition = originalPosition + Vector3.up * offset; break;
                case Direction.DOWN: mainGlyph.transform.localPosition = originalPosition + Vector3.down * offset; break;
                case Direction.RIGHT: mainGlyph.transform.localPosition = originalPosition + Vector3.right * offset; break;
                case Direction.LEFT: mainGlyph.transform.localPosition = originalPosition + Vector3.left * offset; break;
            }
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }

        mainGlyph.transform.localPosition = originalPosition;
    }
}
