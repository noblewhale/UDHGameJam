using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : DungeonObject
{

    public bool isOpen = false;
    public float openDifficulty = 1;
    public bool isLocked = false;
    public float lockPickDifficulty = 1;

    public GameObject openGlyph;
    public GameObject closedGlyph;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void SetOpen (bool isOpen)
    {
        this.isOpen = isOpen;
        this.isCollidable = false;
        this.blocksLineOfSight = false;
		if (isOpen && ! openGlyph.activeSelf)
        {
            openGlyph.SetActive(true);
            closedGlyph.SetActive(false);
        }
        else
        {
            openGlyph.SetActive(false);
            closedGlyph.SetActive(true);
        }
	}

    public override void Collide()
    {
        if (Random.value * 2 > openDifficulty)
        {
            SetOpen(true);
        }
    }
}
