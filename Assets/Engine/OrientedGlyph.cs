using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientedGlyph : MonoBehaviour
{
    Creature owner;
    public Direction orientation;

	void Start ()
    {
        owner = GetComponentInParent<Creature>();
	}
	
	void Update ()
    {
        if (owner)
        {
            orientation = owner.lastDirectionAttackedOrMoved;
        }
        switch (orientation)
        {
            case Direction.UP:
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.DOWN:
                transform.localRotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.RIGHT:
                transform.localRotation = Quaternion.Euler(0, 0, -90);
                break;
            case Direction.LEFT:
                transform.localRotation = Quaternion.Euler(0, 0, 90);
                break;
        }
        
	}
}
