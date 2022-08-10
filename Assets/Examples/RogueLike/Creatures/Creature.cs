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
    public Transform rightHand;
    public Transform leftHand;
    public Transform chest;
    public Transform helmet;
    public Transform pants;

    public DungeonObject baseObject;
    public Tickable tickable;

    public int x { get { return baseObject.x; } }
    public int y { get { return baseObject.y; } }
    public Map map { get { return baseObject.map; } }
    public Inventory inventory { get { return baseObject.inventory; } }

    public int health { get { return baseObject.health; } }
    public int gold { get { return baseObject.gold; } }

    float attackAnimationTime = 0;
    public float attackAnimationDuration = .5f;
    float attackAnimationScale = 1;
    bool attackWillHit = false;

    Vector3 upLeft;
    Vector3 downLeft;
    Vector3 upRight;
    Vector3 downRight;

    void Awake()
    {
        baseObject = GetComponent<DungeonObject>();
        tickable = GetComponent<Tickable>();
        baseObject.onMove += OnMove;
        baseObject.onPickedUpObject += OnPickedUpObject;
        upLeft = (Vector2.up + Vector2.left).normalized;
        downLeft = (Vector2.down + Vector2.left).normalized;
        upRight = (Vector2.up + Vector2.right).normalized;
        downRight = (Vector2.down + Vector2.right).normalized;
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
        lastDirectionAttackedOrMoved = GetDirection(oldX, oldY, newX, newY);

        map.tileObjects[newY][newX].StepOn(this);
    }

    // TODO: Wrapping
    Direction GetDirection(int oldX, int oldY, int newX, int newY)
    {
        int xDif = oldX - newX;
        int yDif = oldY - newY;
        if (Math.Abs(xDif) == Math.Abs(yDif))
        {
            if (xDif > 0 && yDif > 0) return Direction.UP_RIGHT;
            else if (xDif > 0 && yDif < 0) return Direction.DOWN_RIGHT;
            else if (xDif < 0 && yDif < 0) return Direction.DOWN_LEFT;
            else return Direction.UP_LEFT;
        }
        else if (Math.Abs(xDif) > Math.Abs(yDif))
        {
            if (xDif > 0) return Direction.RIGHT;
            else return Direction.LEFT;
        }
        else
        {
            if (yDif > 0) return Direction.UP;
            else return Direction.DOWN;
        }
    }

    public void StartAttack(DungeonObject dOb)
    {
        Creature creature = dOb.GetComponent<Creature>();

        lastDirectionAttackedOrMoved = GetDirection(dOb.x, dOb.y, x, y);
        
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
                case Direction.UP_LEFT: glyph.transform.localPosition = originalPosition + upLeft * offset; break;
                case Direction.DOWN_LEFT: glyph.transform.localPosition = originalPosition + downLeft * offset; break;
                case Direction.UP_RIGHT: glyph.transform.localPosition = originalPosition + upRight * offset; break;
                case Direction.DOWN_RIGHT: glyph.transform.localPosition = originalPosition + downRight * offset; break;
            }

            if (attackWillHit && creature)
            {
                creature.baseObject.DamageFlash(attackAnimationTime / attackAnimationDuration);
            }

            attackAnimationTime += Time.deltaTime;

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
            creature.baseObject.DamageFlash(1);
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
            rightHandObject.transform.parent = null;
            rightHandObject.transform.position = new Vector3(-666, -666, -666);
            baseObject.glyphs.glyphs.RemoveAll(g => rightHandObject.glyphs.glyphs.Contains(g));
        }
        rightHandObject = ob;
        ob.isWeilded = true;
        ob.transform.parent = rightHand.transform;
        ob.transform.localPosition = Vector3.zero;
        baseObject.glyphs.glyphs.AddRange(ob.glyphs.glyphs);
    }

    public void FaceDirection(Tile tile)
    {
        if (tile.y > y) lastDirectionAttackedOrMoved = Direction.UP;
        if (tile.y < y) lastDirectionAttackedOrMoved = Direction.DOWN;
        if (tile.x > x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        if (tile.x < x) lastDirectionAttackedOrMoved = Direction.LEFT;
    }

}
