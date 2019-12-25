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
    public float viewDistance = 4;

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

    void Awake()
    {
        TimeManager.OnTick += OnTick;
        baseObject = GetComponent<DungeonObject>();
        tickable = GetComponent<Tickable>();
        baseObject.onSetPosition += SetPosition;
    }

    void SetPosition(int x, int y)
    {
        if (x < baseObject.x) lastDirectionAttackedOrMoved = Direction.LEFT;
        if (x > baseObject.x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        if (y < baseObject.y) lastDirectionAttackedOrMoved = Direction.DOWN;
        if (y > baseObject.y) lastDirectionAttackedOrMoved = Direction.UP;

        if (map.tileObjects[y][x].isInView)
        {
            if (baseObject.glyphsOb && !baseObject.glyphsOb.activeSelf)
            {
                baseObject.glyphsOb.SetActive(true);
            }
        }
        else
        {
            if (baseObject.glyphsOb && baseObject.glyphsOb.activeSelf)
            {
                baseObject.glyphsOb.SetActive(false);
            }
        }
    }

    //override public void Die()
    //{
    //    //map.tileObjects[y][x].RemoveObject(this);
    //    //map.tileObjects[y][x].SetOccupant(null);
    //    //CreatureSpawner.instance.RemoveCreature(this);
    //    //DropItems();
    //    //Destroy(gameObject);
    //}

    public void Attack(Creature creature)
    { 
        if (creature.x == map.width - 1 && x == 0) lastDirectionAttackedOrMoved = Direction.LEFT;
        else if (creature.x == 0 && x == map.width - 1) lastDirectionAttackedOrMoved = Direction.RIGHT;
        else if (creature.x < x) lastDirectionAttackedOrMoved = Direction.LEFT;
        else if (creature.x > x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        else if (creature.y < y) lastDirectionAttackedOrMoved = Direction.DOWN;
        else if (creature.y > y) lastDirectionAttackedOrMoved = Direction.UP;

        Weapon weapon = null;
        if (rightHandObject != null)
        {
            weapon = rightHandObject.GetComponent<Weapon>();
        }

        float roll = UnityEngine.Random.Range(0, 20);
        roll += dexterity;
        if (roll > creature.dexterity)
        {
            // Hit, but do we do damange?
            if (roll > creature.dexterity + creature.defense)
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
        }

        tickable.nextActionTime = TimeManager.time + (ulong)ticksPerAttack;

        AttackAnimation();
    }

    public void AttackAnimation(float scale = 1, float duration = .5f)
    {
        if (attackMovementAnimation.length != 0)
        {
            if (attackAnimationProcess != null) StopCoroutine(attackAnimationProcess);
            attackAnimationProcess = StartCoroutine(DoAttackAnimation(scale, duration));
        }
    }

    public void PickUpAll()
    {
        List<DungeonObject> itemsToRemoveFromTile = new List<DungeonObject>();
        List<DungeonObject> itemsToDestroy = new List<DungeonObject>();
        foreach (var ob in map.tileObjects[y][x].objectList)
        {
            if (ob.canBePickedUp)
            {
                if (ob.GetComponent<Weapon>() != null)
                {
                    WeildRightHand(ob);
                }
                itemsToRemoveFromTile.Add(ob);
                DungeonObject existingOb;
                bool success = inventory.items.TryGetValue(ob.objectName, out existingOb);
                if (success)
                {
                    existingOb.quantity += ob.quantity;
                    itemsToDestroy.Add(ob);
                }
                else
                {
                    inventory.items.Add(ob.objectName, ob);
                }
                ob.transform.position = new Vector3(-666, -666, -666);
            }
        }

        foreach (var ob in itemsToRemoveFromTile) map.tileObjects[y][x].RemoveObject(ob);
        foreach (var ob in itemsToDestroy) Destroy(ob);
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

    private void OnDestroy()
    {
        TimeManager.OnTick -= OnTick;
    }

    public void OnTick()
    {
        if (attackAnimationProcess != null)
        {
            StopCoroutine(attackAnimationProcess);
            attackAnimationProcess = null;

            Vector3 originalPosition = baseObject.originalGlyphPosition;
            baseObject.glyphs.transform.localPosition = originalPosition;
        }
    }

    IEnumerator DoAttackAnimation(float scale = 1, float duration = .5f)
    {
        float t = 0;
        var glyph = baseObject.glyphs;
        
        if (!glyph) yield break;

        Vector3 originalPosition = baseObject.originalGlyphPosition;
        while (t < duration)
        {
            float offset = attackMovementAnimation.Evaluate(t / duration) * scale;

            switch (lastDirectionAttackedOrMoved)
            {
                case Direction.UP: glyph.transform.localPosition = originalPosition + Vector3.up * offset; break;
                case Direction.DOWN: glyph.transform.localPosition = originalPosition + Vector3.down * offset; break;
                case Direction.RIGHT: glyph.transform.localPosition = originalPosition + Vector3.right * offset; break;
                case Direction.LEFT: glyph.transform.localPosition = originalPosition + Vector3.left * offset; break;
            }
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
        }

        glyph.transform.localPosition = originalPosition;
        attackAnimationProcess = null;
    }
}
