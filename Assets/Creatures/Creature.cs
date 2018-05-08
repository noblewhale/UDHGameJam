﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : DungeonObject
{
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int charisma;
    public int ticksPerMove = 1;
    public int ticksPerAttack = 1;
    public float Speed {
        get {
            if (ticksPerMove == 0) return 0;
            return 1 / ticksPerMove;
        }
    }

    public Direction lastDirectionMoved = Direction.UP;
    public ulong nextActionTime = 0;
    public float viewDistance = 4;

    public MovementBehaviour movementBehaviour;

    override protected void Awake()
    {
        base.Awake();
        movementBehaviour = GetComponent<MovementBehaviour>();
    }

    public void SetPosition(int x, int y)
    {
        if (x < base.x) lastDirectionMoved = Direction.LEFT;
        if (x > base.x) lastDirectionMoved = Direction.RIGHT;
        if (y < base.y) lastDirectionMoved = Direction.DOWN;
        if (y > base.y) lastDirectionMoved = Direction.UP;
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
        float shouldMoveConfidence;
        if (movementBehaviour)
        {
            shouldMoveConfidence = movementBehaviour.ShouldMove();
            movementBehaviour.Move();
        }
    }

    virtual public void ContinueAction()
    {
    }
    
    public void Attack(Creature creature)
    {
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
    }
}
