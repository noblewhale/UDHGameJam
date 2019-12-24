using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    Map map;
    public bool isFloodFilled;
    bool isRevealed = true;
    float gapBetweenLayers = .1f;

    public bool isInView;

    public LinkedList<DungeonObject> objectList = new LinkedList<DungeonObject>();

    public Creature occupant;

    public void Init(Map map, int x, int y)
    {
        transform.parent = map.transform;
        this.map = map;
        this.x = x;
        this.y = y;
        map.tilesThatAllowSpawn.Add(this);

       // SetRevealed(false);
    }


    public bool ContainsObjectOfType(DungeonObject needle)
    {
        foreach (var hay in objectList)
        {
            if (needle.objectName == hay.objectName) return true;
        }

        return false;
    }

    public void SetOccupant(Creature creature)
    {
        occupant = creature;

        if (creature != null)
        {
            var node = objectList.First;
            while (node != null)
            {
                node.Value.SteppedOn(creature);
                if (node.Next == null) break;
                node = node.Next;
            }
        }
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
        ob.SetPosition(x, y);
        ob.transform.parent = transform;
        ob.transform.localPosition = Vector3.zero;
        objectList.AddFirst(ob);
        if (ob.preventsObjectSpawning)
        {
            map.tilesThatAllowSpawn.Remove(this);
        }
        SetRevealed(isRevealed);
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

        if (occupant != null && occupant.isCollidable) occupant.Collide(collidingObject);
    }
}
