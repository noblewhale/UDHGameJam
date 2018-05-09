using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public int value;
    Map map;
    public bool isFloodFilled;
    bool isRevealed = true;
    float gapBetweenLayers = .1f;

    public bool isInView;

    public LinkedList<DungeonObject> objectList = new LinkedList<DungeonObject>();

    public Creature occupant;

    public void Init(Map map, int value, int x, int y)
    {
        transform.parent = map.transform;
        this.value = value;
        this.map = map;
        this.x = x;
        this.y = y;

        SetRevealed(false);
    }

    public void SetOccupant(Creature creature)
    {
        occupant = creature;
    }

    public void SetRevealed(bool isRevealed)
    {
        this.isRevealed = isRevealed;
        SetVisible(isRevealed);
        bool nextObjectIsVisible = isRevealed;
        foreach (var ob in objectList)
        {
            ob.gameObject.SetActive(nextObjectIsVisible);
            if (ob.coversObjectsBeneath)
            {
                nextObjectIsVisible = false;
            }
        }
    }

    public void SetVisible(bool isVisible)
    {
        isInView = isVisible;
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

    public void AddObject(DungeonObject ob)
    { 
        ob.SetPosition(x, y);
        ob.transform.parent = transform;
        ob.transform.localPosition = Vector3.zero;
        objectList.AddFirst(ob);
        SetRevealed(isRevealed);
    }

    public void RemoveObject(DungeonObject ob)
    {
        ob.transform.parent = null;
        objectList.Remove(ob);
        SetRevealed(isRevealed);
    }

    public bool IsCollidable()
    {
        foreach (var ob in objectList)
        {
            if (ob.isCollidable) return true;
        }
        if (occupant != null && occupant.isCollidable) return true;
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

    public void Collide()
    {
        foreach (var ob in objectList)
        {
            if (ob.isCollidable) ob.Collide();
        }

        if (occupant != null && occupant.isCollidable) occupant.Collide();
    }
}
