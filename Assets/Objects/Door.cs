using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : DungeonObject
{

    public bool isOpen = false;
    public float openDifficulty = 1;
    public bool isLocked = false;
    public float lockPickDifficulty = 1;

    public UnityEngine.GameObject openGlyph;
    public UnityEngine.GameObject closedGlyph;
    public UnityEngine.GameObject lockedGlyph;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void SetOpen(bool isOpen)
    {
        this.isOpen = isOpen;
        this.isCollidable = !isOpen;
        this.blocksLineOfSight = !isOpen;
        this.coversObjectsBeneath = !isOpen;
		if (isOpen && !openGlyph.activeSelf)
        {
            map.tileObjects[y][x].SetVisible(map.tileObjects[y][x].isInView);
            openGlyph.SetActive(true);
            closedGlyph.SetActive(false);
            lockedGlyph.SetActive(false);
        }
        else if (!isOpen && openGlyph.activeSelf)
        {
            map.tileObjects[y][x].SetVisible(map.tileObjects[y][x].isInView);
            openGlyph.SetActive(false);
            closedGlyph.SetActive(true);
            lockedGlyph.SetActive(false);
        }
	}

    public void SetLocked(bool isLocked)
    {
        this.isLocked = isLocked;
        if (isLocked && !lockedGlyph.activeSelf)
        {
            SetOpen(false);
            openGlyph.SetActive(false);
            closedGlyph.SetActive(false);
            lockedGlyph.SetActive(true);
        }
        else
        {
            openGlyph.SetActive(true);
            closedGlyph.SetActive(false);
            lockedGlyph.SetActive(true);
        }
    }

    public override void Collide(DungeonObject ob)
    {
        if (isLocked)
        {
            if (ob.GetType() == typeof(Creature) || ob.GetType().IsSubclassOf(typeof(Creature)))
            {
                var creature = (Creature)ob;
                DungeonObject key;
                bool hasKey = creature.inventory.items.TryGetValue("Key", out key);
                if (hasKey)
                {
                    key.quantity--;
                    if (key.quantity == 0)
                    { 
                        creature.inventory.items.Remove("Key");
                    }

                    SetOpen(true);
                }
            }
        }
        else
        {
            if (Random.value * 2 > openDifficulty)
            {
                SetOpen(true);
            }
        }
    }
}
