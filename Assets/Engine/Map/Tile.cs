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
    public float gapBetweenLayers = .1f;

    public bool isInView = false;
    public bool isLit = true;
    public bool isAlwaysLit = false;

    public LinkedList<DungeonObject> objectList = new LinkedList<DungeonObject>();

    public void Init(Map map, int x, int y)
    {
        transform.parent = map.transform;
        this.map = map;
        this.x = x;
        this.y = y;
        map.tilesThatAllowSpawn.Add(this);

        //SetInView(true);
    }

    public bool ContainsObjectOfType(DungeonObject needle)
    {
        foreach (var hay in objectList)
        {
            if (needle.objectName == hay.objectName) return true;
        }

        return false;
    }

    public bool ContainsObjectOfType(DungeonObject[] needles)
    {
        foreach (var hay in objectList)
        {
            foreach (var needle in needles) if (needle.objectName == hay.objectName) return true;
        }

        return false;
    }

    public bool ContainsObjectOfType(string needle)
    {
        foreach (var hay in objectList)
        {
            if (needle == hay.objectName) return true;
        }

        return false;
    }

    public bool ContainsObjectOfType(string[] needles)
    {
        foreach (var hay in objectList)
        {
            foreach (var needle in needles) if (needle == hay.objectName) return true;
        }

        return false;
    }

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

    public void SetLit(bool isLit)
    {
        if (isAlwaysLit && isLit == false) return;

        this.isLit = isLit;
        if (isLit)
        {
            foreach (var ob in objectList)
            {
                if (ob.glyphsOb)
                {
                    ob.SetLit(true);
                }
                if (ob.coversObjectsBeneath) break;
            }
        }
        else
        {
            foreach (var ob in objectList)
            {
                ob.SetLit(false);
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

    public DungeonObject SpawnAndAddObject(DungeonObject dungeonObject, int quantity = 1)
    {
        var ob = Instantiate(dungeonObject).GetComponent<DungeonObject>();
        ob.quantity = quantity;
        AddObject(ob);

        return ob;
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

    public void AddObject(DungeonObject ob, bool isMove = false)
    {
        ob.transform.parent = transform;
        ob.transform.localPosition = Vector3.zero;
        objectList.AddFirst(ob);
        if (isMove)
        {
            ob.Move(x, y);
        }
        else
        {
            ob.SetPosition(x, y);
        }
        if (ob.preventsObjectSpawning)
        {
            map.tilesThatAllowSpawn.Remove(this);
        }
        SetInView(isInView);
        SetLit(isLit);
    }

    public void RemoveObject(DungeonObject ob, bool destroyObject = false)
    {
        ob.transform.parent = null;
        objectList.Remove(ob);
        if (destroyObject)
        {
            Destroy(ob.gameObject);
        }

        if (AllowsSpawn())
        {
            if (!map.tilesThatAllowSpawn.Contains(this))
            {
                map.tilesThatAllowSpawn.Add(this);
            }
        }
        SetInView(isInView);
        SetLit(isLit);
    }

    public void RemoveAllObjects()
    {
        foreach (var dOb in objectList)
        {
            Destroy(dOb.gameObject);
        }

        objectList.Clear();
        if (!map.tilesThatAllowSpawn.Contains(this))
        {
            map.tilesThatAllowSpawn.Add(this);
        }
    }

    public bool IsCollidable()
    {
        foreach (var ob in objectList)
        {
            if (ob.isCollidable) return true;
        }
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
}
