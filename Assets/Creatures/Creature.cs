using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : DungeonObject
{
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
    public Coroutine attackAnimationProcess;

    override protected void Awake()
    {
        base.Awake();
        movementBehaviour = GetComponent<MovementBehaviour>();
        attackBehaviour = GetComponent<AttackBehaviour>();
        TimeManager.OnTick += OnTick;
    }

    override public void Update()
    {
        base.Update();

        if (map.tileObjects[y][x].isInView)
        {
            for (int i = 0; i < glyphs.Length; i++)
            {
                if (!glyphs[i].gameObject.activeSelf)
                {
                    glyphs[i].gameObject.SetActive(true);
                }
            }
        }
        else
        {
            for (int i = 0; i < glyphs.Length; i++)
            {
                if (glyphs[i].gameObject.activeSelf)
                {
                    glyphs[i].gameObject.SetActive(false);
                }
            }
        }
    }

    override public void SetPosition(int x, int y, bool isAction = true)
    {
        if (x < base.x) lastDirectionAttackedOrMoved = Direction.LEFT;
        if (x > base.x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        if (y < base.y) lastDirectionAttackedOrMoved = Direction.DOWN;
        if (y > base.y) lastDirectionAttackedOrMoved = Direction.UP;
        map.tileObjects[base.y][base.x].SetOccupant(null);
        map.tileObjects[y][x].SetOccupant(this);

        base.SetPosition(x, y, isAction);

        if (isAction)
        {
            nextActionTime = TimeManager.time + (ulong)ticksPerMove;
        }
    }

    override public void Die()
    {
        map.tileObjects[y][x].SetOccupant(null);
        CreatureSpawner.instance.RemoveCreature(this);
        DropItems();
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
        if (creature.x == map.width - 1 && x == 0) lastDirectionAttackedOrMoved = Direction.LEFT;
        else if (creature.x == 0 && x == map.width - 1) lastDirectionAttackedOrMoved = Direction.RIGHT;
        else if (creature.x < x) lastDirectionAttackedOrMoved = Direction.LEFT;
        else if (creature.x > x) lastDirectionAttackedOrMoved = Direction.RIGHT;
        else if (creature.y < y) lastDirectionAttackedOrMoved = Direction.DOWN;
        else if (creature.y > y) lastDirectionAttackedOrMoved = Direction.UP;
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
        List<DungeonObject> itemsToRemove = new List<DungeonObject>();
        List<DungeonObject> itemsToDestroy = new List<DungeonObject>();
        foreach (var ob in map.tileObjects[y][x].objectList)
        {
            if (ob.canBePickedUp)
            {
                itemsToRemove.Add(ob);
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

        foreach (var ob in itemsToRemove) map.tileObjects[y][x].RemoveObject(ob);
        foreach (var ob in itemsToDestroy) Destroy(ob);
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
            for (int i = 0; i < glyphs.Length; i++)
            {
                Vector3 originalPosition = originalGlyphPositions[i];
                var glyph = glyphs[i];
                glyph.transform.localPosition = originalPosition;
            }
        }
    }

    IEnumerator DoAttackAnimation(float scale = 1, float duration = .5f)
    {
        float t = 0;
        for (int i = 0; i < glyphs.Length; i++)
        {
            var glyph = glyphs[i];
            Vector3 originalPosition = originalGlyphPositions[i];
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
        }
        attackAnimationProcess = null;
    }
}
