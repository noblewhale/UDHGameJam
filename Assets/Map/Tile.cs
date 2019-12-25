﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public Map map;
    public bool isFloodFilled;
    //public bool isRevealed = true;
    public float gapBetweenLayers = .1f;

    public bool isInView = false;

    public LinkedList<DungeonObject> objectList = new LinkedList<DungeonObject>();

    public void Init(Map map, int x, int y)
    {
        transform.parent = map.transform;
        this.map = map;
        this.x = x;
        this.y = y;
        map.tilesThatAllowSpawn.Add(this);

        //SetRevealed(false);
        SetInView(false);
    }


    public bool ContainsObjectOfType(DungeonObject needle)
    {
        foreach (var hay in objectList)
        {
            if (needle.objectName == hay.objectName) return true;
        }

        return false;
    }

    //public void SetRevealed(bool isRevealed)
    //{
    //    this.isRevealed = isRevealed;
    //    foreach (var ob in objectList)
    //    {
    //        if (ob.glyphs)
    //        {
    //            //if (ob.isVisibleWhenNotInSight && !isRevealed) continue;
    //            ob.glyphs.SetRevealed(isRevealed);
    //        }
    //    }
    //}

    public void SetInView(bool isVisible)
    {
        isInView = isVisible;
        if (isInView)
        {
            foreach (var ob in objectList)
            {
                if (ob.glyphsOb)
                {
                    ob.SetInView(true);
                }
                if (ob.coversObjectsBeneath) break;
            }
        }
        else
        {
            foreach (var ob in objectList)
            {
                ob.SetInView(false);
            }
        }
    }

    public void Update()
    {
        if (map == null) return;
        transform.localPosition = new Vector3(x * map.tileWidth, y * map.tileHeight, 0);

        int l = 0;
        foreach (var ob in objectList)
        {
            ob.transform.localPosition = new Vector3(ob.transform.localPosition.x, ob.transform.localPosition.y, l * gapBetweenLayers);
            l++;
        }
    }

    public void SpawnAndAddObject(DungeonObject dungeonObject)
    {
        var ob = Instantiate(dungeonObject).GetComponent<DungeonObject>();
        AddObject(ob);
    }

    public void DestroyAllObjects()
    {
        foreach (var ob in objectList)
        {
            ob.inventory.DestroyAll();
            Destroy(ob);
        }
        if (!map.tilesThatAllowSpawn.Contains(this))
        {
            map.tilesThatAllowSpawn.Add(this);
        }
        objectList.Clear();
    }

    public void AddObject(DungeonObject ob)
    {
        ob.transform.parent = transform;
        ob.transform.localPosition = Vector3.zero;
        ob.SetPosition(x, y);
        objectList.AddFirst(ob);
        if (ob.preventsObjectSpawning)
        {
            map.tilesThatAllowSpawn.Remove(this);
        }
        SetInView(isInView);
    }

    public void RemoveObject(DungeonObject ob, bool destroyObject = false)
    {
        ob.transform.parent = null;
        objectList.Remove(ob);

        if (AllowsSpawn())
        {
            if (!map.tilesThatAllowSpawn.Contains(this))
            {
                map.tilesThatAllowSpawn.Add(this);
            }
        }
        SetInView(isInView);
    }

    public bool IsCollidable()
    {
        foreach (var ob in objectList)
        {
            if (ob.isCollidable) return true;
        }
        //if (occupant != null && occupant.isCollidable) return true;
        return false;
    }

    public bool DoesBlockLineOfSight()
    {
        foreach (var ob in objectList)
        {
            if (ob.blocksLineOfSight) return true;
        }
        return false;
    }

    public bool ContainsCollidableObject()
    {
        return objectList.Any(x => x.isCollidable);
    }

    public bool AllowsSpawn()
    {
        return !objectList.Any(x => x.preventsObjectSpawning);
    }

    public void Collide(DungeonObject collidingObject)
    {
        foreach (var ob in objectList)
        {
            if (ob.isCollidable) ob.Collide(collidingObject);
        }

        //if (occupant != null && occupant.isCollidable) occupant.Collide(collidingObject);
    }
}
