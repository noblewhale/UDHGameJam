using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Tickable))]
[RequireComponent(typeof(DungeonObject))]
public class Creature : MonoBehaviour
{
    public Race race;

    public int mana;
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int charisma;

    public int defense = 1;

    public ulong ticksPerMove = 1;
    public ulong ticksPerAttack = 1;
    public AnimationCurve attackMovementAnimation;
    public float Speed {
        get {
            if (ticksPerMove == 0) return 0;
            return 1 / ticksPerMove;
        }
    }

    public Direction lastDirectionAttackedOrMoved = Direction.UP;

    public Coroutine attackAnimationProcess;

    public DungeonObject leftHandObject;
    public DungeonObject rightHandObject;

    public DungeonObject baseObject;
    public Tickable tickable;

    public int x { get { return baseObject.x; } }
    public int y { get { return baseObject.y; } }
    public Map map { get { return baseObject.map; } }
    public Inventory inventory { get { return baseObject.inventory; } }

    public int health { get { return baseObject.health; } }
    public int gold { get { return baseObject.gold; } }

    float attackAnimationTime = 0;
    float attackAnimationDuration = .5f;
    float attackAnimationScale = 1;
    bool attackWillHit = false;

    void Awake()
    {
        baseObject = GetComponent<DungeonObject>();
        tickable = GetComponent<Tickable>();
        baseObject.onMove += OnMove;
        baseObject.onPickedUpObject += OnPickedUpObject;
    }

    void OnPickedUpObject(DungeonObject ob)
    {
        if (ob.GetComponent<Weapon>() != null)
        {
            WeildRightHand(ob);
        }
    }

    void OnMove(int oldX, int oldY, int newX, int newY)
    {
        if (newX == map.width - 1 && oldX == 0) lastDirectionAttackedOrMoved = Direction.LEFT;
        else if (newX == 0 && oldX == map.width - 1) lastDirectionAttackedOrMoved = Direction.RIGHT;
        else if (oldX > newX) lastDirectionAttackedOrMoved = Direction.LEFT;
        else if (oldX < newX) lastDirectionAttackedOrMoved = Direction.RIGHT;
        else if (oldY > newY) lastDirectionAttackedOrMoved = Direction.DOWN;
        else if (oldY < newY) lastDirectionAttackedOrMoved = Direction.UP;

        //baseObject.PickUpAll();
        //tickable.nextActionTime = TimeManager.instance.time + (ulong)ticksPerMove;
    }

    public void StartAttack(DungeonObject dOb)
    {
        Creature creature = dOb.GetComponent<Creature>();

        if (dOb.x == map.width - 1 && x == 0) lastDirectionAttackedOrMoved = Direction.LEFT;
        else if (dOb.x == 0 && x == map.width - 1) lastDirectionAttackedOrMoved = Direction.RIGHT;
        else if (dOb.x < x) lastDirectionAttackedOrMoved = Direction.LEFT;
        else if (dOb.x > x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        else if (dOb.y < y) lastDirectionAttackedOrMoved = Direction.DOWN;
        else if (dOb.y > y) lastDirectionAttackedOrMoved = Direction.UP;
        
        attackWillHit = false;

        if (creature != null)
        {
            float roll = UnityEngine.Random.Range(0, 20);
            roll += dexterity;
            if (roll > creature.dexterity)
            {
                // Hit, but do we do damange?
                if (roll > creature.dexterity + creature.defense)
                {
                    // Got past armor / defense
                    attackWillHit = true;
                }
            }
        }

        attackAnimationTime = 0;
    }

    public bool ContinueAttack(DungeonObject dOb)
    {
        Creature creature = dOb.GetComponent<Creature>();
        var glyph = baseObject.glyphs;

        if (!glyph) return true;

        Vector3 originalPosition = baseObject.originalGlyphPosition;
        if (attackAnimationTime < attackAnimationDuration)
        {
            float offset = attackMovementAnimation.Evaluate(attackAnimationTime / attackAnimationDuration) * attackAnimationScale;

            switch (lastDirectionAttackedOrMoved)
            {
                case Direction.UP: glyph.transform.localPosition = originalPosition + Vector3.up * offset; break;
                case Direction.DOWN: glyph.transform.localPosition = originalPosition + Vector3.down * offset; break;
                case Direction.RIGHT: glyph.transform.localPosition = originalPosition + Vector3.right * offset; break;
                case Direction.LEFT: glyph.transform.localPosition = originalPosition + Vector3.left * offset; break;
            }
            attackAnimationTime += Time.deltaTime;

            if (attackWillHit && creature)
            {
                creature.baseObject.DamageFlash(attackAnimationTime);
            }

            return false;
        }

        return true;
    }

    public void FinishAttack(DungeonObject dOb)
    {
        Creature creature = dOb.GetComponent<Creature>();
        Weapon weapon = null;
        if (rightHandObject != null)
        {
            weapon = rightHandObject.GetComponent<Weapon>();
        }

        if (attackWillHit && creature)
        { 
            // Got past armor / defense
            if (weapon == null)
            {
                creature.baseObject.TakeDamage(1);
            }
            else
            {
                int damage = UnityEngine.Random.Range(weapon.minBaseDamage, weapon.maxBaseDamage + 1);
                creature.baseObject.TakeDamage(damage);
            }
        }

        Vector3 originalPosition = baseObject.originalGlyphPosition;
        baseObject.glyphs.transform.localPosition = originalPosition;

        //tickable.nextActionTime = TimeManager.instance.time + (ulong)ticksPerAttack;
    }

    public void WeildRightHand(DungeonObject ob)
    {
        if (rightHandObject != null)
        {
            rightHandObject.isWeilded = false;
        }
        rightHandObject = ob;
        ob.isWeilded = true;
    }

    public void FaceDirection(Tile tile)
    {
        if (tile.y > y) lastDirectionAttackedOrMoved = Direction.UP;
        if (tile.y < y) lastDirectionAttackedOrMoved = Direction.DOWN;
        if (tile.x > x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        if (tile.x < x) lastDirectionAttackedOrMoved = Direction.LEFT;
    }

}
