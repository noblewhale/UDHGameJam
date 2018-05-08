using System;
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

    public int Speed {
        get {
            return dexterity + 1;
        }
    }

    public float lastMoveTime;
    public float viewDistance = 4;

    public void SetPosition(int x, int y)
    {
        map.tileObjects[tileY][tileX].SetOccupant(null);
        map.tileObjects[y][x].SetOccupant(this);
        tileX = x;
        tileY = y;
        transform.localPosition = new Vector3(tileX * map.tileWidth, tileY * map.tileHeight, transform.localPosition.z);
    }

    override public void Die()
    {
        map.tileObjects[tileY][tileX].SetOccupant(null);
        Destroy(gameObject);
    }
}
